﻿using TMSPS.Core.Common;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords
{
    public interface IRatingAdapter : IBaseAdapter
    {
        double? Vote(string login, int challengeID, ushort rating);
        double? GetVoteByLogin(string login, int challengeID);
        double? GetAverageVote(int challengeID);
    }
}