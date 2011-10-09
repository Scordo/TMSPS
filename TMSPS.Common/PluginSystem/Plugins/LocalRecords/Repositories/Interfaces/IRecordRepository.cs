using System.Collections.Generic;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords.Repositories.Interfaces
{
    public interface IRecordRepository : IRepositoryBase
    {
        RecordState CheckAndWriteNewRecord(string login, int challengeID, int timeOrScore);
        List<RankEntry> GetTopRecordsForChallenge(int challengeID, uint maxRecords);
        uint? GetBestTime(string login, int challengeID);
    }
}
