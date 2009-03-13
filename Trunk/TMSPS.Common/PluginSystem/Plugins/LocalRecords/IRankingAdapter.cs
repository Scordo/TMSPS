﻿using System.Collections.Generic;
using TMSPS.Core.Common;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords
{
    public interface IRankingAdapter : IBaseAdapter
    {
        Ranking Deserialize_ByLogin(string login);
        List<Ranking> Deserialize_List(uint top);
        List<RankingStats> DeserializeListByMost(uint top, uint rankLimit);
        void ReCreateAll();
        void UpdateForChallenge(int challengeID);
        void UpdateForChallenge(string uniqueChallengeID);
    }
}