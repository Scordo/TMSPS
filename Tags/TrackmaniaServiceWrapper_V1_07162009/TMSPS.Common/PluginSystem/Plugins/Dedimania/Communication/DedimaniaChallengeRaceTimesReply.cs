using System;
using System.Collections.Generic;
using CookComputing.XmlRpc;

namespace TMSPS.Core.PluginSystem.Plugins.Dedimania.Communication
{
    public class DedimaniaChallengeRaceTimesReply
    {
        public string Uid { get; set; }
        public int TotalRaces { get; set; }
        public int TotalPlayers { get; set; }
        public int TimeAttackRaces { get; set; }
        public int TimeAttackPlayers { get; set; }
        public int NumberOfChecks { get; set; }
        public int ServerMaxRecords { get; set; }
        public List<DedimaniaRecordNew> Records { get; set; }

        public static DedimaniaChallengeRaceTimesReply Parse(object multiCallResultElement)
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
                !replyStruct.ContainsKey("NumberOfChecks") || !replyStruct.ContainsKey("ServerMaxRecords") ||
                !replyStruct.ContainsKey("Records"))
                return null;

            return new DedimaniaChallengeRaceTimesReply
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

        private static List<DedimaniaRecordNew> ParseRecords(object multiCallResultElement)
        {
            List<DedimaniaRecordNew> result = new List<DedimaniaRecordNew>();

            if (multiCallResultElement == null || !multiCallResultElement.GetType().IsArray)
                return result;

            object[] rootElements = (object[])multiCallResultElement;

            foreach (XmlRpcStruct recordStruct in rootElements)
            {
                DedimaniaRecordNew record = DedimaniaRecordNew.Parse(recordStruct);

                if (recordStruct != null)
                    result.Add(record);
            }

            return result;
        }
    }
}
