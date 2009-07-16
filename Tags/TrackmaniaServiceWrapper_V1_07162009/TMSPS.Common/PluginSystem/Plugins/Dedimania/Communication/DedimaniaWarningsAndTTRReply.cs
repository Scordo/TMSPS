using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using CookComputing.XmlRpc;

namespace TMSPS.Core.PluginSystem.Plugins.Dedimania.Communication
{
    public class DedimaniaWarningsAndTTRReply
    {
        [XmlRpcMember("globalTTR")]
        public double GlobalTTR { get; set; }
        [XmlRpcMember("methods")]
        public DedimaniaWarningsAndTTRElement[] Methods { get; set; }

        public static DedimaniaWarningsAndTTRReply Parse(object multiCallResultElement)
        {
            if (multiCallResultElement == null || !multiCallResultElement.GetType().IsArray)
                return null;

            object[] rootObjects = (object[])multiCallResultElement;
            if (rootObjects.Length == 0 || rootObjects[0].GetType() != typeof(XmlRpcStruct))
                return null;

            XmlRpcStruct rootStruct = (XmlRpcStruct)rootObjects[0];
            if (!rootStruct.ContainsKey("globalTTR") || !rootStruct.ContainsKey("methods") || !rootStruct["methods"].GetType().IsArray)
                return null;

            DedimaniaWarningsAndTTRReply result = new DedimaniaWarningsAndTTRReply();
            result.GlobalTTR = Convert.ToDouble(rootStruct["globalTTR"], new CultureInfo("en-us"));

            List<DedimaniaWarningsAndTTRElement> warnings = new List<DedimaniaWarningsAndTTRElement>();
            foreach (object warningElement in (IEnumerable)rootStruct["methods"])
            {
                warnings.Add(DedimaniaWarningsAndTTRElement.Parse(warningElement));
            }

            result.Methods = warnings.ToArray();

            return result;
        }
    }
}
