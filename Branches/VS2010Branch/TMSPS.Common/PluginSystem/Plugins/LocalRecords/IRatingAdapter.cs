using TMSPS.Core.Common;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords
{
    public interface IRatingAdapter : IBaseAdapter
    {
        Pair<double?, int> Vote(string login, int challengeID, ushort rating);
        double? GetVoteByLogin(string login, int challengeID);
        Pair<double?, int> GetAverageVote(int challengeID);
    }
}