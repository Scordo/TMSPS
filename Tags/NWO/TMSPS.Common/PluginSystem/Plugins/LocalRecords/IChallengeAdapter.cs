﻿using System.Collections.Generic;
using TMSPS.Core.Common;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords
{
    public interface IChallengeAdapter : IBaseAdapter
    {
    	void TryCreate(Challenge challenge);
		Challenge Deserialize(int challengeID);
		Challenge Deserialize(string uniqueID);
		int? IncreaseRaces(int challengeID);
		int? IncreaseRaces(string uniqueID);
    	void IncreaseRaces(Challenge challenge);
        void DeleteTracksNotInProvidedList(List<string> uniqueTrackIDs);
    }
}