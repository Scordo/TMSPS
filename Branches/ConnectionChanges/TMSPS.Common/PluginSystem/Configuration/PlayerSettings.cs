using System;
using System.Text.RegularExpressions;
using TMSPS.Core.Common;
using TMSPS.Core.Communication.ProxyTypes;
using PlayerInfo=TMSPS.Core.Communication.EventArguments.Callbacks.PlayerInfo;

namespace TMSPS.Core.PluginSystem.Configuration
{
    public class PlayerSettings
    {
        #region Properties

        public string Login { get; private set; }
        public string NickName { get; private set; }
        public int PlayerID { get; private set; }
        public int TeamID { get; private set; }
        public int LadderRanking { get; private set; }
        public bool IsInOfficialMode { get; set; }
        public PlayerSpectatorStatus SpectatorStatus { get; private set; }
        public PlayerFlags Flags { get; private set; }
        public int OnlineRights { get; private set; }
        public bool IsUnitedAccount { get { return OnlineRights == 3; } }
        public string IPAddress { get; private set; }
        public bool IsReferee { get; private set; }
        public string Language { get; private set; }
        public PlayerSettingsDetailMode DetailMode { get; private set; }
        public PluginSettingsStore PluginSettings { get; private set; }
        public ManiaLinkPageHashStore ManiaLinkPageHashStore { get; private set; }
        

        #endregion

        #region Constructor

        internal PlayerSettings()
        {
            PluginSettings = new PluginSettingsStore(); 
            ManiaLinkPageHashStore = new ManiaLinkPageHashStore();
            SpectatorStatus = new PlayerSpectatorStatus();
            Flags = null;
            DetailMode = PlayerSettingsDetailMode.BasicPlayerInfo;
        }

        #endregion

        #region Public Methods

        public void UpdateFromPlayerInfo(PlayerInfoBase playerInfo)
        {
            if (playerInfo == null)
                return;

            if (Login != null && string.Compare(playerInfo.Login, Login, StringComparison.OrdinalIgnoreCase) != 0)
                return;

            Login = playerInfo.Login;
            NickName = playerInfo.NickName;
            SpectatorStatus.IsSpectator = playerInfo.IsSpectator;
            PlayerID = playerInfo.PlayerId;
            TeamID = playerInfo.TeamId;
            IsInOfficialMode = playerInfo.IsInOfficialMode;
            DetailMode |= PlayerSettingsDetailMode.BasicPlayerInfo;
        }

        public void UpdateFromPlayerInfo(PlayerInfo playerInfo)
        {
            if (playerInfo == null)
                return;

            if (Login != null && string.Compare(playerInfo.Login, Login, StringComparison.OrdinalIgnoreCase) != 0)
                return;

            Login = playerInfo.Login;
            NickName = playerInfo.NickName;
            SpectatorStatus = playerInfo.SpectatorStatusObject.Clone();
            Flags = playerInfo.FlagsObject.Clone();
            PlayerID = playerInfo.PlayerID;
            TeamID = playerInfo.TeamID;

            if (playerInfo.LadderRanking > 0)
                LadderRanking = LadderRanking;

            DetailMode |= PlayerSettingsDetailMode.CallbackPlayerInfo;
        }

        public void UpdateFromPlayerInfo(DetailedPlayerInfo playerInfo)
        {
            UpdateFromPlayerInfo((PlayerInfoBase) playerInfo);
            IsReferee = playerInfo.IsReferee;
            IPAddress = Regex.Replace(playerInfo.IPAddress, @":\d+", string.Empty, RegexOptions.Compiled);
            OnlineRights = playerInfo.OnlineRights;
            Language = playerInfo.Language;

            PlayerRanking worldRanking = playerInfo.LadderStats.PlayerRankings.Find(ranking => ranking.Path == "World");
            if (worldRanking != null)
                LadderRanking = worldRanking.Ranking;

            DetailMode |= PlayerSettingsDetailMode.DetailedPlayerInfo;
        }

        #endregion
    }

    [Flags]
    public enum PlayerSettingsDetailMode { BasicPlayerInfo = 1, CallbackPlayerInfo = 2, DetailedPlayerInfo = 4 }

    public static class PlayerSettingsDetailModeExtender
    {
        #region Public Methods

        public static bool HasCallbackPlayerInfo(this PlayerSettingsDetailMode mode)
        {
            return (mode & PlayerSettingsDetailMode.CallbackPlayerInfo) == PlayerSettingsDetailMode.CallbackPlayerInfo;
        }

        public static bool HasDetailedPlayerInfo(this PlayerSettingsDetailMode mode)
        {
            return (mode & PlayerSettingsDetailMode.DetailedPlayerInfo) == PlayerSettingsDetailMode.DetailedPlayerInfo;
        }

        #endregion
    }
}