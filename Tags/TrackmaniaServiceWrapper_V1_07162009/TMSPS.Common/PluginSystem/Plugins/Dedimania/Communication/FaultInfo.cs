using System;
using CookComputing.XmlRpc;

namespace TMSPS.Core.PluginSystem.Plugins.Dedimania.Communication
{
    public class FaultInfo
    {
        public string FaultCode { get; private set; }
        public string FaultString { get; private set; }

        private FaultInfo()
        {
        }

        public static FaultInfo Parse(object multiCallResultElement)
        {
            if (multiCallResultElement == null || multiCallResultElement.GetType() != typeof(XmlRpcStruct))
                return null;

            XmlRpcStruct data = (XmlRpcStruct)multiCallResultElement;
            if (!data.ContainsKey("faultCode") || !data.ContainsKey("faultString"))
                return null;

            return new FaultInfo { FaultCode = Convert.ToString(data["faultCode"]), FaultString = Convert.ToString(data["faultString"]) };
        }
    }
}
