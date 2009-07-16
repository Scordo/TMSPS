using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace TMSPS.Core.Common
{
    public interface IDump { }

    public static class IDumpExtender
    {
        public static string GetDumpString(this IDump instance)
        {
            StringBuilder result = new StringBuilder();
            instance.AddDumpString(result, 0);

            return result.ToString();
        }

        public static void AddDumpString(this IDump instance, StringBuilder output, int depth)
        {
            AddDumpString(output, depth, instance);
        }

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

            if (!(instance is IDump)) 
                return;

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
}