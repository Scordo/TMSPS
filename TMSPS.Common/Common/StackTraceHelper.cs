using System;
using System.Text;
using System.Reflection;
using System.Diagnostics;

namespace TMSPS.Core.Common
{
    public class StackTraceHelper
    {
        public static string GetCurrentStackTrace()
        {
            return GetStackTrace(2);
        }

        private static string GetStackTrace(int intDepth)
        {
            StringBuilder result = new StringBuilder();
            StackTrace stack = new StackTrace();

            for (int i = 0; i < stack.FrameCount; i++)
            {
                StackFrame frame = stack.GetFrame(i);

                if (i >= intDepth)
                {
                    AddMethodSignature(frame.GetMethod(), result);

                    int lineNumber = frame.GetFileLineNumber();
                    if (lineNumber != 0)
                        result.AppendFormat(" at line {0}", lineNumber);

                    result.Append(Environment.NewLine);
                }
            }

            return result.ToString();
        }

        public static string GetMethodSignature(MethodBase method)
        {
            StringBuilder result = new StringBuilder();

            AddMethodSignature(method, result);

            return result.ToString();
        }

        public static void AddMethodSignature(MethodBase method, StringBuilder builder)
        {
            builder.AppendFormat("\t{0}.", method.ReflectedType.FullName);
            builder.AppendFormat("{0}(", method.Name);

            ParameterInfo[] methodParameters = method.GetParameters();
            for (int i = 0; i < methodParameters.Length; i++)
            {
                ParameterInfo parameterInfo = methodParameters[i];

                if (i == 0)
                    builder.AppendFormat("{0} {1}", parameterInfo.Name, parameterInfo.ParameterType.Name);
                else
                    builder.AppendFormat(", {0} {1}", parameterInfo.Name, parameterInfo.ParameterType.Name);
            }

            builder.Append(")");
        }
    }
}
