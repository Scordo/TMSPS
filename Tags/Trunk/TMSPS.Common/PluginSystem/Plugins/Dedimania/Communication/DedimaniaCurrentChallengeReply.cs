using System;
using System.Collections.Generic;
using CookComputing.XmlRpc;

namespace TMSPS.Core.PluginSystem.Plugins.Dedimania.Communication
{
    public class DedimaniaCurrentChallengeReply
    {
        public string Uid { get; set; }
        public int TotalRaces { get; set; }
        public int TotalPlayers { get; set; }
        public int TimeAttackRaces { get; set; }
        public int TimeAttackPlayers { get; set; }
        public int NumberOfChecks { get; set; }
        public int ServerMaxRecords { get; set; }
        public List<DedimaniaRecord> Records { get; set; }

        public static DedimaniaCurrentChallengeReply Parse(object multiCallResultElement)
        {
            if (multiCallResultElement == null || !multiCallResultElement.GetType().IsArray)
                return null;

            object[] rootElements = (object[])multiCallResultElement;
            if (rootElements.Length != 1 || rootElements[0].GetType() != typeof(XmlRpcStruct))
                return null;

            XmlRpcStruct replyStruct = (XmlRpcStruct)rootElements[0];

            if (!replyStruct.ContainsKey("Uid") || !replyStruct.ContainsKey("TotalRaces") ||
                !replyStruct.ContainsKey("TotalPlayers") || !replyStruct.ContainsKey("TimeAttackRaces") ||
                !replyStruct.ContainsKey("TimeAttackPlayers") || !replyStruct.ContainsKey("TimeAttackPlayers") ||
                !replyStruct.ContainsKey("NumberOfChecks") || !replyStruct.ContainsKey("ServerMaxRecords")||
                !replyStruct.ContainsKey("Records"))
                return null;

            return new DedimaniaCurrentChallengeReply
            {
                Uid = Convert.ToString(replyStruct["Uid"]),
                TotalRaces = Convert.ToInt32(replyStruct["TotalRaces"]),
                TotalPlayers = Convert.ToInt32(replyStruct["TotalPlayers"]),
                TimeAttackRaces = Convert.ToInt32(replyStruct["TimeAttackRaces"]),
                TimeAttackPlayers = Convert.ToInt32(replyStruct["TimeAttackPlayers"]),
                NumberOfChecks = Convert.ToInt32(replyStruct["NumberOfChecks"]),
                ServerMaxRecords = Convert.ToInt32(replyStruct["ServerMaxRecords"]),
                Records = ParseRecords(replyStruct["Records"])
            };
        }

        private static List<DedimaniaRecord> ParseRecords(object multiCallResultElement)
        {
            List<DedimaniaRecord> result = new List<DedimaniaRecord>();

            if (multiCallResultElement == null || !multiCallResultElement.GetType().IsArray)
                return result;

            object[] rootElements = (object[])multiCallResultElement;

            foreach (XmlRpcStruct recordStruct in rootElements)
            {
                DedimaniaRecord record = DedimaniaRecord.Parse(recordStruct);

                if (recordStruct != null)
                    result.Add(record);
            }

            return result;
        }
    }
}
