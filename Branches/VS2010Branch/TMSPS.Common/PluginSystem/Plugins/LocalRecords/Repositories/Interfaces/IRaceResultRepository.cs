using System.Collections.Generic;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords.Entities;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords.Repositories.Interfaces
{
    public interface IRaceResultRepository : IRepositoryBase
    {
        void AddResult(RaceResultEntity raceResult);
        List<PositionStats> DeserializeListByMost(uint top, uint positionLimit);
    }
}
