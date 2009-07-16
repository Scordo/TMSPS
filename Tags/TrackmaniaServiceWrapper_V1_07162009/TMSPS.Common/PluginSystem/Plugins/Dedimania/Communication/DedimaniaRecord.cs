using System;
using System.Collections.Generic;
using CookComputing.XmlRpc;

namespace TMSPS.Core.PluginSystem.Plugins.Dedimania.Communication
{
    public class DedimaniaRecord
    {
        public string Login { get; set; }
        public string Nickname { get; set; }
        public TimeSpan BestTime { get; set; }
        public int Rank { get; set; }
        public List<TimeSpan> CheckPoints { get; set; }
        public int Vote { get; set; }

        public static DedimaniaRecord Parse(XmlRpcStruct recordStruct)
        {
            if (recordStruct == null)
                return null;

            if (!recordStruct.ContainsKey("Best") || !recordStruct.ContainsKey("Rank") ||
                !recordStruct.ContainsKey("NickName") || !recordStruct.ContainsKey("Vote") ||
                !recordStruct.ContainsKey("Login") || !recordStruct.ContainsKey("Checks"))
                return null;

            return new DedimaniaRecord
            {
                Nickname = Convert.ToString(recordStruct["NickName"]),
                Login = Convert.ToString(recordStruct["Login"]),
                BestTime = TimeSpan.FromMilliseconds(Convert.ToInt32(recordStruct["Best"])),
                Rank = Convert.ToInt32(recordStruct["Rank"]),
                Vote = Convert.ToInt32(recordStruct["Vote"]),
                CheckPoints = GetCheckPointList(recordStruct["Checks"])
            };
        }

        private static List<TimeSpan> GetCheckPointList(object checkPointsValue)
        {
            if (checkPointsValue as int[] == null)
                return new List<TimeSpan>();

            return new List<TimeSpan>(Array.ConvertAll((int[])checkPointsValue, checkPointTime => TimeSpan.FromMilliseconds(checkPointTime)));
        }
    }
}