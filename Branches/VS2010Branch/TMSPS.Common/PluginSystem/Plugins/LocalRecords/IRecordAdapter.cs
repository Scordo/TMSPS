using System.Collections.Generic;
using TMSPS.Core.Common;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords
{
    public interface IRecordAdapter : IBaseAdapter
    {
        void CheckAndWriteNewRecord(string login, int challengeID, int timeOrScore, out uint? oldLocalRecordPosition, out uint? newLocalRecordPosition, out bool newBest);
        List<RankEntry> GetTopRecordsForChallenge(int challengeID, uint maxRecords);
        uint? GetBestTime(string login, int challengeID);
    }
}