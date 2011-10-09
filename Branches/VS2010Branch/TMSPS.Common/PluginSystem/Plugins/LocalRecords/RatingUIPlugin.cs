using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Xml.Linq;
using TMSPS.Core.Common;
using TMSPS.Core.ManiaLinking;
using TMSPS.Core.PluginSystem.Configuration;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords.Entities;
using SettingsBase=TMSPS.Core.Common.SettingsBase;
using Version=System.Version;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords
{
    public class RatingUIPlugin : LocalRecordsPluginPlugin
    {
        #region Properties

        public override Version Version { get { return new Version("1.0.0.0"); } }
        public override string Author { get { return "Jens Hofmann"; } }
        public override string Name { get { return "Rating UI Plugin"; } }
        public override string Description { get { return "Displays the rating of tracks in a userinterface."; } }
        public override string ShortName { get { return "RatingUI"; } }

        protected RatingUIPluginSettings Settings { get; private set; }
        private double? _lastAverageVoteValue;
        private int _lastVotesCount;
        private const string _allVoteRatingManiaLinkPageID = "RatingUIAllVotePanelID";
        private const string _ownVoteRatingManiaLinkPageID = "RatingUIOwnVotePanelID";

        #endregion

        #region Constructor

        protected RatingUIPlugin(string pluginDirectory) : base(pluginDirectory)
        {
            
        }

	    #endregion

        protected override void Init()
        {
            Settings = RatingUIPluginSettings.ReadFromFile(PluginSettingsFilePath);
            Pair<double?, int> voteInfo = HostPlugin.RatingRepository.Average(HostPlugin.CurrentChallengeID);
            _lastAverageVoteValue = voteInfo.Value1;
            _lastVotesCount = voteInfo.Value2;
            SendAllVoteManiaLinkPage(_lastAverageVoteValue, _lastVotesCount);
            SendOwnVoteManiaLinkPageToAll(HostPlugin.CurrentChallengeID);

            HostPlugin.ChallengeChanged += HostPlugin_ChallengeChanged;
            Context.RPCClient.Callbacks.PlayerChat += Callbacks_PlayerChat;
            Context.RPCClient.Callbacks.PlayerConnect += Callbacks_PlayerConnect;
        }

        protected override void Dispose(bool connectionLost)
        {
            HostPlugin.ChallengeChanged -= HostPlugin_ChallengeChanged;
            Context.RPCClient.Callbacks.PlayerChat -= Callbacks_PlayerChat;
            Context.RPCClient.Callbacks.PlayerConnect -= Callbacks_PlayerConnect;

            if (!connectionLost)
            {
                SendEmptyManiaLinkPage(_allVoteRatingManiaLinkPageID);
                SendEmptyManiaLinkPage(_ownVoteRatingManiaLinkPageID);
            }
        }

        private void Callbacks_PlayerConnect(object sender, Communication.EventArguments.Callbacks.PlayerConnectEventArgs e)
        {
            if (e.Handled)
                return;

            SendAllVoteManiaLinkPageToLogin(e.Login, _lastAverageVoteValue, _lastVotesCount);

            int playerId = PlayerCache.Get(e.Login).Id.Value;
            RatingEntity rating = HostPlugin.RatingRepository.Get(playerId, HostPlugin.CurrentChallengeID);
            SendOwnVoteManiaLinkPageToLogin(e.Login, rating == null ? (int?) null : rating.Value);
        }

        private void HostPlugin_ChallengeChanged(object sender, EventArgs e)
        {
            Pair<double?, int> voteInfo = HostPlugin.RatingRepository.Average(HostPlugin.CurrentChallengeID);
            _lastAverageVoteValue = voteInfo.Value1;
            _lastVotesCount = voteInfo.Value2;

            SendAllVoteManiaLinkPage(_lastAverageVoteValue, _lastVotesCount);
            SendOwnVoteManiaLinkPageToAll(HostPlugin.CurrentChallengeID);
        }

        private void Callbacks_PlayerChat(object sender, Communication.EventArguments.Callbacks.PlayerChatEventArgs e)
        {
            if (e.IsServerMessage || e.Text.IsNullOrTimmedEmpty() || e.IsRegisteredCommand)
                return;

            string message = e.Text.Trim();
            ushort? voteValue;

            Dictionary<string, ushort?> voteValues = new Dictionary<string, ushort?> {{ "++", 8 }, { "--", 0 }, { "+-", 4 }, { "-+", 4 }, 
	                                                                                  { "+1", 1 }, { "+2", 2 }, { "+3", 3 }, { "+4", 4 }, 
	                                                                                  { "+5", 5 }, { "+6", 6 }, { "+7", 7 }, { "+8", 8 }};

            voteValues.TryGetValue(message, out voteValue);

            if (voteValue.HasValue)
            {

                HostPlugin.RatingRepository.Rate(e.PlayerID, HostPlugin.CurrentChallengeID, voteValue.Value);
                Pair<double?, int> voteInfo = HostPlugin.RatingRepository.Average(HostPlugin.CurrentChallengeID);
                double? averageVote = voteInfo.Value1;

                if (averageVote.HasValue)
                    OnPlayerVoted(e.Login, voteValue.Value, averageVote.Value, voteInfo.Value2);
            }
        }

        private void SendOwnVoteManiaLinkPageToAll(int currentChallengeID)
        {
            foreach (PlayerSettings playerSettings in Context.PlayerSettings.GetAllAsList())
            {
                int playerId = PlayerCache.Get(playerSettings.Login).Id.Value;
                RatingEntity rating = HostPlugin.RatingRepository.Get(playerId, currentChallengeID);
                SendOwnVoteManiaLinkPageToLogin(playerSettings.Login, rating == null ? (int?) null : rating.Value);
            }
        }

        private void SendOwnVoteManiaLinkPageToLogin(string login, int? voteValue)
        {
            Context.RPCClient.Methods.SendDisplayManialinkPageToLogin(login, GetOwnVoteManiaLinkPageContent(voteValue), 0, false);
        }

        private void SendAllVoteManiaLinkPage(double? voteValue, int votesCount)
        {
            Context.RPCClient.Methods.SendDisplayManialinkPage(GetAllVoteManiaLinkPageContent(voteValue, votesCount), 0, false);
        }

        private void SendAllVoteManiaLinkPageToLogin(string login, double? voteValue, int votesCount)
        {
            Context.RPCClient.Methods.SendDisplayManialinkPageToLogin(login, GetAllVoteManiaLinkPageContent(voteValue, votesCount), 0, false);
        }

        protected override void OnManiaLinkPageAnswer(string login, int playerID, TMAction action)
        {
            if (!action.IsAreaAction || action.AreaID != (byte) RatingArea.MainArea)
                return;

            byte ownVoteValue = Convert.ToByte(action.AreaActionID - 1);

            int playerEntityId = PlayerCache.Get(login).Id.Value;
            HostPlugin.RatingRepository.Rate(playerEntityId, HostPlugin.CurrentChallengeID, ownVoteValue);
            Pair<double?, int> voteInfo = HostPlugin.RatingRepository.Average(HostPlugin.CurrentChallengeID);
            double? averageVote = voteInfo.Value1;

            if (averageVote.HasValue)
                OnPlayerVoted(login, ownVoteValue, averageVote.Value, voteInfo.Value2);
        }

        private string GetOwnVoteManiaLinkPageContent(int? voteValue)
        {
            if (!voteValue.HasValue)
                voteValue = -1;

            List<string> replaceValues = new List<string> {"ManiaLinkID", _ownVoteRatingManiaLinkPageID};
            for (int i = 0; i < 9; i++)
            {
                replaceValues.Add(i.ToString());
                replaceValues.Add(i <= voteValue ? Settings.ActiveVoteStyle : Settings.InactiveVoteStyle);
            }

            const byte areaID = (byte) RatingArea.MainArea;
            for (int i = 0; i < 9; i++)
            {
                replaceValues.Add("A"+i);
                replaceValues.Add(TMAction.CalculateActionID(ID, areaID, Convert.ToByte(i+1)).ToString());
            }

            return ReplaceMessagePlaceHolders(Settings.OwnVoteTemplate, replaceValues.ToArray());
        }

        private string GetAllVoteManiaLinkPageContent(double? voteValue, int votesCount)
        {
            if (!voteValue.HasValue)
                voteValue = -1;

            List<string> replaceValues = new List<string> { "ManiaLinkID", _allVoteRatingManiaLinkPageID };
            for (int i = 0; i < 9; i++)
            {
                replaceValues.Add(i.ToString());
                replaceValues.Add(i <= voteValue ? Settings.ActiveVoteStyle : Settings.InactiveVoteStyle);
            }

            replaceValues.Add("VotesCount");
            replaceValues.Add(votesCount.ToString());

            return ReplaceMessagePlaceHolders(Settings.AllVoteTemplate, replaceValues.ToArray());
        }

        private enum RatingArea
        {
            MainArea = 1
        }

        private void OnPlayerVoted(string login, ushort voteValue, double averageVoteValue, int votesCount)
        {
            int lastAverageVoteValue = Convert.ToInt32(Math.Floor(_lastAverageVoteValue ?? -1));
            int currentAverageVoteValue = Convert.ToInt32(Math.Floor(averageVoteValue));
            _lastAverageVoteValue = averageVoteValue;

            SendFormattedMessageToLogin(login, Settings.VoteAcceptedMessage, "AverageVote", averageVoteValue.ToString("F", CultureInfo.InvariantCulture));
            SendOwnVoteManiaLinkPageToLogin(login, voteValue);

            if (currentAverageVoteValue != lastAverageVoteValue)
                SendAllVoteManiaLinkPage(averageVoteValue, votesCount);
        }
    }

    public class RatingUIPluginSettings : SettingsBase
    {
        #region Constants

        public const string VOTE_ACCEPTED_MESSAGE = "{[#ServerStyle]}> {[#MessageStyle]}Vote accepted! Average vote is: {[#HighlightStyle]}{[AverageVote]}";
        public const string VOTE_ACTIVE_STYLE = "TagTypeGold";
        public const string VOTE_INACTIVE_STYLE = "TagTypeNone";

        #endregion

        #region Properties

        public string OwnVoteTemplate { get; protected internal set; }
        public string AllVoteTemplate { get; protected internal set; }
        public string VoteAcceptedMessage { get; private set; }
        public string ActiveVoteStyle { get; private set; }
        public string InactiveVoteStyle { get; private set; }

        

        #endregion

        #region Public Methods

        public static RatingUIPluginSettings ReadFromFile(string xmlConfigurationFile)
        {
            XDocument configDocument = XDocument.Load(xmlConfigurationFile);

            if (configDocument.Root == null)
                throw new ConfigurationErrorsException("Could not find root node in file: " + xmlConfigurationFile);

            return new RatingUIPluginSettings
            {
               VoteAcceptedMessage = ReadConfigString(configDocument.Root, "VoteAcceptedMessage", VOTE_ACCEPTED_MESSAGE, xmlConfigurationFile),
               ActiveVoteStyle = ReadConfigString(configDocument.Root, "ActiveVoteStyle", VOTE_ACTIVE_STYLE, xmlConfigurationFile),
               InactiveVoteStyle = ReadConfigString(configDocument.Root, "InactiveVoteStyle", VOTE_INACTIVE_STYLE, xmlConfigurationFile),
               OwnVoteTemplate = ReadConfigString(configDocument.Root, "OwnVoteTemplate", xmlConfigurationFile),
               AllVoteTemplate = ReadConfigString(configDocument.Root, "AllVoteTemplate", xmlConfigurationFile)
            };
        }

        #endregion
    }
}