using System.Collections.Generic;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords.Entities;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords.Repositories.Interfaces
{
    public interface IChallengeRepository : IRepositoryBase
    {
        ChallengeEntity Get(string uid);
        ChallengeEntity IncreaseRaces(ChallengeEntity challenge);
        int DeleteTracksNotInProvidedList(List<string> uniqueTrackIDs);
        List<string> GetDrivenUniqueTrackIDs(int playerId);
        int Count();
    }
}