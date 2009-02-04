using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.Communication
{
    public class RPCCommand
    {
        #region Public Methods

        public static XDocument GetMethodCallElementWithXmlDeclaration(string method, params object[] parameters)
        {
            return new XDocument(new XDeclaration("1.0", "utf-8", "yes"), GetMethodCallElement(method, parameters));
        }

        public static XElement GetMethodCallElement(string method, params object[] parameters)
        {
            if (method == null)
                throw new ArgumentNullException("method");

            XElement methodCallElement = new XElement("methodCall");

            XElement methodNameElement = new XElement("methodName", method);
            methodCallElement.Add(methodNameElement);

            XElement paramsElement = new XElement("params");
            methodCallElement.Add(paramsElement);

            foreach (object parameter in parameters)
            {
                XElement paramElement = new XElement("param");
				paramElement.Add(GetValueElement(parameter));

				paramsElement.Add(paramElement);
            }

            return methodCallElement;
        }

        public static XElement GetValueElement(object value)
        {
            if (value is int || value is uint || value is short | value is ushort || value is byte || value is sbyte || value is long || value is ulong)
                return new XElement("value", new XElement("int", value));

            if (value is float || value is double || value is decimal)
                return new XElement("value", new XElement("double", value));

            if (value is bool)
                return new XElement("value", new XElement("boolean", (bool)value ? "1" : "0"));

            if (value == null || value is string)
                return new XElement("value", new XElement("string", value));

            if (value is byte[])
                return new XElement("value", new XElement("base64", Convert.ToBase64String(value as byte[])));

            if (value is IEnumerable)
            {
                XElement arrayElement = new XElement("array");
                XElement dataElement = new XElement("data");
				arrayElement.Add(dataElement);

                foreach (object childValue in (value as IEnumerable))
                {
					dataElement.Add((GetValueElement(childValue)));
                }

            	return arrayElement;
            }

            if (value is DateTime)
                return new XElement("value", new XElement("dateTime.iso8601", ((DateTime)value).ToString("yyyyMMddThh:mm:ss")));

			if (value == null)
				throw new InvalidOperationException("Null values are not supported");

			if (value.GetType().IsClass)
			{
				XElement result = new XElement("struct");

				List<RPCParamInfo> members = RPCParamInfo.GetFromType(value.GetType(), RPCParamInfo.RevtrievalMode.OnlyWithMemberName);

				foreach (RPCParamInfo member in members)
				{
					XElement memberElement = new XElement("member", new XElement("name", member.MemberName));
					memberElement.Add(GetValueElement(member.PropertyInfo.GetValue(value, null)));

					result.Add(memberElement);
				}

				return result;
			}

            throw new InvalidOperationException("Specified type is not supported: " + value.GetType());
        }

        public static T GetInstance<T>(XElement paramsMessageElement) where T : class
        {
            return (T)GetInstance(typeof(T), paramsMessageElement);
        }

        #endregion

        #region Non Public Methods

        private static object GetInstance(Type type, XContainer paramsMessageElement)
        {
            object result = Activator.CreateInstance(type);
            List<XElement> paramElements = new List<XElement>(paramsMessageElement.Elements("param"));
            List<RPCParamInfo> parameters = RPCParamInfo.GetFromType(type, RPCParamInfo.RevtrievalMode.OnlyWithIndexes);

            if (paramElements.Count != parameters.Count)
                throw new InvalidOperationException("The amount of parameters in the xml does differ from the amount of properties declared on type: " + type);

            for (int i = 0; i < paramElements.Count; i++)
            {
                GetParameterElementInstance(parameters, i, paramElements[i], result);
            }

            return result;
        }

        internal static void GetParameterElementInstance(List<RPCParamInfo> parameters, int i, XElement paramElement, object result)
        {
            RPCParamInfo paramInfo = parameters[i];
            XElement valueElement = paramElement.Element("value");

            if (valueElement == null)
                throw new InvalidOperationException(string.Format("Could not find value element for property '{0}'.", paramInfo.PropertyInfo.Name));

            object value = GetValueInstance(valueElement, paramInfo.PropertyInfo.PropertyType);

            paramInfo.PropertyInfo.SetValue(result, value, null);
        }

        private static object GetValueInstance(XContainer valueElement, Type valueType)
        {
            if (valueElement == null)
                throw new ArgumentNullException("valueElement");

            string elementValueName = GetElementValueName(valueType);

            switch (elementValueName)
            {
                case "struct":
                    return GetClassInstance(valueElement, valueType);
                case "array":
                    object list = GetListInstance(valueElement, valueType);
                    return list;
                default:
                    XElement valueTypeElement = valueElement.Element(elementValueName);

                    if (valueTypeElement == null)
                        throw new InvalidOperationException(string.Format("Could not find element '{0}' for property '{1}'.", elementValueName, valueType));

                    return ConvertStringToType(valueTypeElement.Value, valueType);
            }
        }

        private static object GetClassInstance(XContainer valueElement, Type valueType)
        {
            XElement structElement = valueElement.Element("struct");

            if (structElement == null)
                throw new InvalidOperationException("Could not find 'struct' element.");

            object result = Activator.CreateInstance(valueType);

            List<RPCParamInfo> members = RPCParamInfo.GetFromType(valueType, RPCParamInfo.RevtrievalMode.OnlyWithMemberName);
            List<XElement> memberElements = new List<XElement>(structElement.Elements("member"));

            if (members.Count != memberElements.Count)
                throw new InvalidOperationException("The amount of member elements in the xml does differ from the amount of members declared on type: " + valueType);

            foreach (XElement memberElement in memberElements)
            {
                XElement nameElement = memberElement.Element("name");

                if (nameElement == null)
                    throw new InvalidOperationException("Could not find 'name' element.");

                string memberName = nameElement.Value;
                RPCParamInfo memberParam = members.FirstOrDefault(x => string.Compare(x.MemberName, memberName, StringComparison.OrdinalIgnoreCase) == 0);

                if (memberParam == null)
                    throw new InvalidOperationException(string.Format("Could not find member '{0}' on type '{1}'.", memberName, valueType));

                XElement memberValueElement = memberElement.Element("value");

                if (memberValueElement == null)
                    throw new InvalidOperationException("Could not find 'value' element.");

                object memberValue = GetValueInstance(memberValueElement, memberParam.PropertyInfo.PropertyType);
                memberParam.PropertyInfo.SetValue(result, memberValue, null);
            }

            return result;
        }

        private static object GetListInstance(XContainer valueElement, Type valueType)
        {
            Type listContentType = valueType.GetGenericArguments()[0];
            object list = Activator.CreateInstance(valueType);

            XElement arrayElement = valueElement.Element("array");

            if (arrayElement == null)
                throw new InvalidOperationException("Could not find array element");

            XElement arrayDataElement = arrayElement.Element("data");

            if (arrayDataElement == null)
                throw new InvalidOperationException("Could not find data element in array element");

            foreach (XElement currentValueElement in arrayDataElement.Elements("value"))
            {
                valueType.InvokeMember("Add", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public, null, list, new[] { GetValueInstance(currentValueElement, listContentType) });
            }
            return list;
        }

        private static string GetElementValueName(Type type)
        {
            if (type == typeof(int) || type == typeof(uint) || type == typeof(short) | type == typeof(ushort) || type == typeof(byte) || type == typeof(sbyte) || type == typeof(long) || type == typeof(long))
                return "i4";

            if (type == typeof(float) || type == typeof(double) || type == typeof(decimal))
                return "double";

            if (type == typeof(bool))
                return "boolean";

            if (type == typeof(string))
                return "string";

            if (type == typeof(DateTime))
                return "dateTime.iso8601";

            if (type == typeof(byte[]))
                return "base64";

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                return "array";

            if (type.IsClass)
                return "struct";

            throw new InvalidOperationException("Specified type is not supported: " + type);
        }

        private static object ConvertStringToType(string value, Type type)
        {
            if (type == typeof(Boolean))
                return (value == "1");

            if (type == typeof(byte[]))
                return Convert.FromBase64String(value);

            return Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
        }

        #endregion
    }
}