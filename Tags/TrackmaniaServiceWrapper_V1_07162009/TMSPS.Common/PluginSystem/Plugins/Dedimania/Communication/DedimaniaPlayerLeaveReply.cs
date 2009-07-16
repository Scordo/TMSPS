using System;
using CookComputing.XmlRpc;

namespace TMSPS.Core.PluginSystem.Plugins.Dedimania.Communication
{
    public class DedimaniaPlayerLeaveReply
    {
        [XmlRpcMember("Login")]
        public string Login { get; set; }

        public static DedimaniaPlayerLeaveReply Parse(object multiCallResultElement)
        {
            if (multiCallResultElement == null || !multiCallResultElement.GetType().IsArray)
                return null;

            object[] rootElements = (object[]) multiCallResultElement;
            if (rootElements.Length != 1 || rootElements[0].GetType() != typeof(XmlRpcStruct))
                return null;

            XmlRpcStruct replyStruct = (XmlRpcStruct) rootElements[0];

            if (!replyStruct.ContainsKey("Login"))
                return null;

            return new DedimaniaPlayerLeaveReply { Login = Convert.ToString(replyStruct["Login"]) };
        }
    }
}
