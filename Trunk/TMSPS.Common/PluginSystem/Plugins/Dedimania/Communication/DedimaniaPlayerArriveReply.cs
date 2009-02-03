using System;
using System.Collections.Generic;
using CookComputing.XmlRpc;

namespace TMSPS.Core.PluginSystem.Plugins.Dedimania.Communication
{
    public class DedimaniaPlayerArriveReply
    {
        public string Nation { get; private set; }
        public string TeamName { get; private set; }
        public string Login { get; private set; }
        public List<OptionValueTool> Options { get; private set; }
        public List<AliasTextTool> Aliases { get; private set; }

        public static DedimaniaPlayerArriveReply Parse(object multiCallResultElement)
        {
            if (multiCallResultElement == null || !multiCallResultElement.GetType().IsArray)
                return null;

            object[] rootElements = (object[])multiCallResultElement;
            if (rootElements.Length != 1 || rootElements[0].GetType() != typeof(XmlRpcStruct))
                return null;

            XmlRpcStruct replyStruct = (XmlRpcStruct)rootElements[0];

            if (!replyStruct.ContainsKey("Options") || !replyStruct.ContainsKey("Nation") || !replyStruct.ContainsKey("TeamName") || !replyStruct.ContainsKey("Login") || !replyStruct.ContainsKey("Aliases"))
                return null;

            DedimaniaPlayerArriveReply result = new DedimaniaPlayerArriveReply();
            result.Aliases = new List<AliasTextTool>();
            result.Options = new List<OptionValueTool>();
            result.Login = Convert.ToString(replyStruct["Login"]);
            result.TeamName = Convert.ToString(replyStruct["TeamName"]);
            result.Nation = Convert.ToString(replyStruct["Nation"]);

            return result;
        }
    }
}