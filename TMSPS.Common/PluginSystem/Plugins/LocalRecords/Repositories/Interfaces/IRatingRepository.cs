using TMSPS.Core.Common;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords.Entities;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords.Repositories.Interfaces
{
    public interface IRatingRepository : IRepositoryBase
    {
        void Rate(int playerId, int challengeId, ushort rating);
        RatingEntity Get(int playerId, int challengeId);
        /// <returns>AverageVote + VotesCount</returns>
        Pair<double?, int> Average(int challengeID);
    }
}