using TMSPS.Core.PluginSystem.Plugins.LocalRecords.Entities;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords.Repositories.Interfaces
{
    public interface ILaptResultRepository : IRepositoryBase
    {
        void AddLapResult(LapResultEntity lapResult);
    }
}