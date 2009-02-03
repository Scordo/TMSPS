using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using TMSPS.Core.Common;

namespace TMSPS.Core.Communication.ResponseHandling
{
    public abstract class ResponseBase<T> : IDump where T : ResponseBase<T>
    {
        #region Properties

        public bool Erroneous { get { return Fault != null; } }
        public FaultResponse Fault { get; private set; }

        #endregion

        #region Public Methods


        public static T Parse(XElement messageElement)
        {
            T result;

            XElement faultElement = messageElement.Element("fault");

            if (faultElement != null)
            {
                result = (T)Activator.CreateInstance(typeof(T));
                result.Fault = FaultResponse.GetFromXml(faultElement);
            }
            else
            {
                result = RPCCommand.GetInstance<T>(messageElement.Element("params"));
            }

            return result;
        }

        public string GetDumpString()
        {
            StringBuilder result = new StringBuilder();
            AddDumpString(result, 0, this);

            return result.ToString();
        }

        #endregion

        #region Non Public Methods

        private static void AddDumpString(StringBuilder output, int depth, object instance)
        {
            if (instance == null)
                return;

            if (instance.GetType().IsGenericType && instance.GetType().GetGenericTypeDefinition() == typeof(List<>))
            {
                int counter = 0;
                foreach (object listItem in (IEnumerable)instance)
                {
                    string propertyString = listItem is IDump ? string.Empty : Convert.ToString(listItem);
                    output.AppendFormat("{0}[{1}]: {2}{3}", string.Empty.PadLeft(depth, '\t'), counter, propertyString, Environment.NewLine);

                    if (listItem is IDump)
                        AddDumpString(output, depth + 1, listItem);

                    counter++;
                }
            }

            if (instance is IDump)
            {
                foreach (PropertyInfo property in instance.GetType().GetProperties())
                {
                    object propertyValue = property.GetValue(instance, null);

                    if (propertyValue != null && propertyValue.GetType().IsGenericType && propertyValue.GetType().GetGenericTypeDefinition() == typeof(List<>))
                    {
                        output.AppendFormat("{0}{1}: {2}", string.Empty.PadLeft(depth, '\t'), property.Name, Environment.NewLine);
                        AddDumpString(output, depth + 1, propertyValue);
                    }
                    else
                    {
                        string propertyString = propertyValue is IDump ? string.Empty : Convert.ToString(propertyValue);
                        output.AppendFormat("{0}{1}: {2}{3}", string.Empty.PadLeft(depth, '\t'), property.Name, propertyString, Environment.NewLine);

                        if (propertyValue is IDump)
                            AddDumpString(output, depth + 1, propertyValue);
                    }
                }
            }
        }

        #endregion
    }
}
