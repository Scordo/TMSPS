using System;
using System.Globalization;
using CookComputing.XmlRpc;

namespace TMSPS.Core.PluginSystem.Plugins.Dedimania.Communication
{
    public class DedimaniaWarningsAndTTRElement
    {
        [XmlRpcMember("methodName")]
        public string MethodName { get; set; }
        [XmlRpcMember("errors")]
        public string Error { get; set; }
        [XmlRpcMember("TTR")]
        public double TTR { get; set; }

        public static DedimaniaWarningsAndTTRElement Parse(object multiCallResultElement)
        {
            if (multiCallResultElement == null || multiCallResultElement.GetType() != typeof(XmlRpcStruct))
                return null;

            XmlRpcStruct warningsStruct = (XmlRpcStruct)multiCallResultElement;
            if (!warningsStruct.ContainsKey("errors") || !warningsStruct.ContainsKey("methodName") || !warningsStruct.ContainsKey("TTR"))
                return null;

            return new DedimaniaWarningsAndTTRElement
            {
                Error = Convert.ToString(warningsStruct["errors"]),
                MethodName = Convert.ToString(warningsStruct["methodName"]),
                TTR = Convert.ToDouble(warningsStruct["TTR"], new CultureInfo("en-us"))
            };
        }
    }
}
