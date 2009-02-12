using System;
using System.Collections.Generic;
using TMSPS.Core.Communication.ResponseHandling;
using TMSPS.Core.Communication.ProxyTypes;
using Version=TMSPS.Core.Communication.ProxyTypes.Version;

namespace TMSPS.Core.Communication
{
    public class Methods
    {
        #region Members

        private readonly TrackManiaRPCClient _client;

        #endregion

        #region Constructor

        public Methods(TrackManiaRPCClient client)
        {
            if (client == null)
                throw new ArgumentNullException("client");

            _client = client;
        }

        #endregion

        #region Public Methods

        public GenericResponse<bool> Authenticate(string login, string password)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.Authenticate.ToString(), login, password);
        }

        public GenericResponse<bool> EnableCallbacks(bool enable)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.EnableCallbacks.ToString(), enable);
        }

        public GenericResponse<string> GetServerPackMask()
        {
            return (GenericResponse<string>)_client.SendMethod<GenericResponse<string>>(TrackManiaMethod.GetServerPackMask.ToString());
        }

        /// <summary>
        /// Kick the player with the specified login, with an optional message.
        /// </summary>
        public GenericResponse<bool> Kick(string login, string reason)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.Kick.ToString(), login, reason);
        }

        /// <summary>
        /// Kick the player with the specified PlayerId, with an optional message.
        /// </summary>
        public GenericResponse<bool> KickID(int id, string reason)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.KickId.ToString(), id, reason);
        }

        /// <summary>
        /// Ban the player with the specified login, with an optional message.
        /// </summary>
        public GenericResponse<bool> Ban(string login, string reason)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.Ban.ToString(), login, reason);
        }

        /// <summary>
        /// Ban the player with the specified PlayerId, with an optional message.
        /// </summary>
        public GenericResponse<bool> BanId(int id, string reason)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.BanId.ToString(), id, reason);
        }

        /// <summary>
        /// Ban the player with the specified login, with a message. Add it to the black list, and optionally save the new list.
        /// </summary>
        public GenericResponse<bool> BanAndBlackList(string login, string reason, bool saveBlackList)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.BanAndBlackList.ToString(), login, reason, saveBlackList);
        }

        /// <summary>
        /// Unban the player with the specified client name.
        /// </summary>
        public GenericResponse<bool> UnBan(string login)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.UnBan.ToString(), login);
        }

        /// <summary>
        ///  Clean the ban list of the server.
        /// </summary>
        public GenericResponse<bool> CleanBanList()
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.CleanBanList.ToString());
        }

        /// <summary>
        /// Returns the list of banned players. This method takes two parameters. The first parameter specifies the maximum number of infos to be returned, and the second one the starting index in the list.
        /// </summary>
        public GenericListResponse<BanEntry> GetBanList(int maxEntriestToReceive, int startingIndex)
        {
            return (GenericListResponse<BanEntry>)_client.SendMethod<GenericListResponse<BanEntry>>(TrackManiaMethod.GetBanList.ToString(), maxEntriestToReceive, startingIndex);
        }

        /// <summary>
        /// Blacklist the player with the specified login.
        /// </summary>
        public GenericResponse<bool> BlackList(string login)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.BlackList.ToString(), login);
        }

        /// <summary>
        /// Blacklist the player with the specified PlayerId.
        /// </summary>
        public GenericResponse<bool> BlackListId(int userID)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.BlackListId.ToString(), userID);
        }

        /// <summary>
        /// UnBlackList the player with the specified login.
        /// </summary>
        public GenericResponse<bool> UnBlackList(string login)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.UnBlackList.ToString(), login);
        }

        /// <summary>
        /// Clean the blacklist of the server.
        /// </summary>
        public GenericResponse<bool> CleanBlackList()
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.CleanBlackList.ToString());
        }

        /// <summary>
        ///  Returns the list of blacklisted players. This method takes two parameters. The first parameter specifies the maximum number of infos to be returned, and the second one the starting index in the list.
        /// </summary>
        public GenericListResponse<LoginResponse> GetBlackList(int maxEntriestToReceive, int startingIndex)
        {
            return (GenericListResponse<LoginResponse>)_client.SendMethod<GenericListResponse<LoginResponse>>(TrackManiaMethod.GetBlackList.ToString(), maxEntriestToReceive, startingIndex);
        }

        /// <summary>
        /// Load the black list file with the specified file name.
        /// </summary>
        public GenericResponse<bool> LoadBlackList(string filename)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.LoadBlackList.ToString(), filename);
        }

        /// <summary>
        /// Save the black list in the file with specified file name.
        /// </summary>
        public GenericResponse<bool> SaveBlackList(string filename)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SaveBlackList.ToString(), filename);
        }

        /// <summary>
        /// Add the player with the specified login on the guest list.
        /// </summary>
        public GenericResponse<bool> AddGuest(string login)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.AddGuest.ToString(), login);
        }

        /// <summary>
        /// Add the player with the specified PlayerId on the guest list.
        /// </summary>
        public GenericResponse<bool> AddGuestId(int userID)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.AddGuestId.ToString(), userID);
        }

        /// <summary>
        /// Remove the player with the specified login from the guest list.
        /// </summary>
        public GenericResponse<bool> RemoveGuest(string login)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.RemoveGuest.ToString(), login);
        }

        /// <summary>
        /// Remove the player with the specified PlayerId from the guest list.
        /// </summary>
        public GenericResponse<bool> RemoveGuestId(int userID)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.RemoveGuestId.ToString(), userID);
        }

        /// <summary>
        /// Clean the guest list of the server.
        /// </summary>
        public GenericResponse<bool> CleanGuestList()
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.CleanGuestList.ToString());
        }

        /// <summary>
        /// Returns the list of players on the guest list. This method takes two parameters. The first parameter specifies the maximum number of infos to be returned, and the second one the starting index in the list.
        /// </summary>
        public GenericListResponse<LoginResponse> GetGuestList(int maxEntriestToReceive, int startingIndex)
        {
            return (GenericListResponse<LoginResponse>)_client.SendMethod<GenericListResponse<LoginResponse>>(TrackManiaMethod.GetGuestList.ToString(), maxEntriestToReceive, startingIndex);
        }

        /// <summary>
        /// Load the guest list file with the specified file name.
        /// </summary>
        public GenericResponse<bool> LoadGuestList(string filename)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.LoadGuestList.ToString(), filename);
        }

        /// <summary>
        /// Save the guest list in the file with specified file name.
        /// </summary>
        public GenericResponse<bool> SaveGuestList(string filename)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SaveGuestList.ToString(), filename);
        }

        public GenericResponse<PlayerInfo> GetPlayerInfo(string loginName)
        {
            return (GenericResponse<PlayerInfo>)_client.SendMethod<GenericResponse<PlayerInfo>>(TrackManiaMethod.GetPlayerInfo.ToString(), loginName);
        }

        public GenericResponse<DetailedPlayerInfo> GetDetailedPlayerInfo(string loginName)
        {
            return (GenericResponse<DetailedPlayerInfo>)_client.SendMethod<GenericResponse<DetailedPlayerInfo>>(TrackManiaMethod.GetDetailedPlayerInfo.ToString(), loginName);
        }

        public GenericResponse<Version> GetVersion()
        {
            return (GenericResponse<Version>)_client.SendMethod<GenericResponse<Version>>(TrackManiaMethod.GetVersion.ToString());
        }

        public GenericResponse<ServerOptions> GetServerOptions()
        {
            return (GenericResponse<ServerOptions>)_client.SendMethod<GenericResponse<ServerOptions>>(TrackManiaMethod.GetServerOptions.ToString());
        }

        public GenericResponse<GameInfo> GetCurrentGameInfo()
        {
            return (GenericResponse<GameInfo>)_client.SendMethod<GenericResponse<GameInfo>>(TrackManiaMethod.GetCurrentGameInfo.ToString());
        }

        public GenericListResponse<PlayerInfo> GetPlayerList()
        {
            return GetPlayerList(500, 0);
        }

        public GenericListResponse<PlayerInfo> GetPlayerList(int maxPlayersToReceive, int startingIndex)
        {
            return (GenericListResponse<PlayerInfo>)_client.SendMethod<GenericListResponse<PlayerInfo>>(TrackManiaMethod.GetPlayerList.ToString(), maxPlayersToReceive, startingIndex);
        }

        public GenericResponse<int> GetNextChallengeIndex()
        {
            return (GenericResponse<int>)_client.SendMethod<GenericResponse<int>>(TrackManiaMethod.GetNextChallengeIndex.ToString());
        }

        public GenericListResponse<ChallengeListSingleInfo> GetChallengeList(int challengeIndex)
        {
            return GetChallengeList(1, challengeIndex);
        }

        public GenericListResponse<ChallengeListSingleInfo> GetChallengeList(int maxChallengesToReceive, int challengeIndex)
        {
            return (GenericListResponse<ChallengeListSingleInfo>)_client.SendMethod<GenericListResponse<ChallengeListSingleInfo>>(TrackManiaMethod.GetChallengeList.ToString(), maxChallengesToReceive, challengeIndex);
        }

        public GenericResponse<bool> SendDisplayManialinkPage(string maniaLinkXML, int timeout, bool hideOnOptionClick)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SendDisplayManialinkPage.ToString(), maniaLinkXML, timeout, hideOnOptionClick);
        }

        public GenericResponse<bool> SendDisplayManialinkPageToId(int userID, string maniaLinkXML, int timeout, bool hideOnOptionClick)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SendDisplayManialinkPageToId.ToString(), userID, maniaLinkXML, timeout, hideOnOptionClick);
        }

        public GenericResponse<bool> SendDisplayManialinkPageToLogin(string login, string maniaLinkXML, int timeout, bool hideOnOptionClick)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SendDisplayManialinkPageToLogin.ToString(), login, maniaLinkXML, timeout, hideOnOptionClick);
        }

        public GenericResponse<bool> SendHideManialinkPage()
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SendHideManialinkPage.ToString());
        }

        public GenericResponse<bool> SendHideManialinkPageToId(int userID)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SendHideManialinkPageToId.ToString(), userID);
        }

        public GenericResponse<bool> SendHideManialinkPageToLogin(string login)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SendHideManialinkPageToLogin.ToString(), login);
        }

        public GenericListResponse<ManiaLinkAnswer> GetManialinkPageAnswers()
        {
            return (GenericListResponse<ManiaLinkAnswer>)_client.SendMethod<GenericListResponse<ManiaLinkAnswer>>(TrackManiaMethod.GetManialinkPageAnswers.ToString());
        }

        /// <summary>
        ///  Sets whether buddy notifications should be sent in the chat. login is the login of the player, or '' for global setting, and enabled is the value.
        /// </summary>
        public GenericResponse<bool> SetBuddyNotification(string login, bool setEnabled)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SetBuddyNotification.ToString(), login, setEnabled);
        }

        /// <summary>
        /// Gets whether buddy notifications are enabled for login, or '' to get the global setting.
        /// </summary>
        public GenericResponse<bool> GetBuddyNotification(string login)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.GetBuddyNotification.ToString(), login);
        }

        /// <summary>
        /// Write the data to the specified file. The filename is relative to the Tracks path.
        /// </summary>
        public GenericResponse<bool> WriteFile(string filename, byte[] fileContent)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.WriteFile.ToString(), filename, fileContent);
        }

        /// <summary>
        /// Send the data to the specified player.
        /// </summary>
        public GenericResponse<bool> TunnelSendDataToId(int userID, byte[] data)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.TunnelSendDataToId.ToString(), userID, data);
        }

        /// <summary>
        /// Send the data to the specified player.
        /// </summary>
        public GenericResponse<bool> TunnelSendDataToLogin(string login, byte[] data)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.TunnelSendDataToLogin.ToString(), login, data);
        }

        /// <summary>
        /// Just log the parameters and invoke a callback. Can be used to talk to other xmlrpc clients connected, or to make custom votes. If used in a callvote, the first parameter will be used as the vote message on the clients.
        /// </summary>
        public GenericResponse<bool> Echo(string param1, string param2)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.Echo.ToString(), param1, param2);
        }

        /// <summary>
        /// Ignore the player with the specified login.
        /// </summary>
        public GenericResponse<bool> Ignore(string login)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.Ignore.ToString(), login);
        }

        /// <summary>
        /// Ignore the player with the specified PlayerId.
        /// </summary>
        public GenericResponse<bool> IgnoreId(int userID)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.IgnoreId.ToString(), userID);
        }

        /// <summary>
        /// Unignore the player with the specified login.
        /// </summary>
        public GenericResponse<bool> UnIgnore(string login)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.UnIgnore.ToString(), login);
        }

        /// <summary>
        /// Unignore the player with the specified PlayerId.
        /// </summary>
        public GenericResponse<bool> UnIgnoreId(int userID)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.UnIgnoreId.ToString(), userID);
        }

        /// <summary>
        /// Clean the ignore list of the server.
        /// </summary>
        public GenericResponse<bool> CleanIgnoreList()
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.CleanIgnoreList.ToString());
        }

        /// <summary>
        /// Returns the list of ignored players. This method takes two parameters. The first parameter specifies the maximum number of infos to be returned, and the second one the starting index in the list.
        /// </summary>
        public GenericListResponse<LoginResponse> GetIgnoreList(int maxEntriestToReceive, int startingIndex)
        {
            return (GenericListResponse<LoginResponse>)_client.SendMethod<GenericListResponse<LoginResponse>>(TrackManiaMethod.GetIgnoreList.ToString(), maxEntriestToReceive, startingIndex);
        }

        /// <summary>
        /// Pay coppers from the server account to a player, returns the BillId. This method takes three parameters: Login of the payee, Coppers to pay and a Label to send with the payment. The creation of the transaction itself may cost coppers, so you need to have coppers on the server account.
        /// </summary>
        public GenericResponse<int> Pay(string login, int amount, string label)
        {
            return (GenericResponse<int>)_client.SendMethod<GenericResponse<int>>(TrackManiaMethod.Pay.ToString(), login, amount, label);
        }

        /// <summary>
        /// Create a bill, send it to a player, and return the BillId. This method takes four parameters: LoginFrom of the payer, Coppers the player has to pay, Label of the transaction and an optional LoginTo of the payee (if empty string, then the server account is used). The creation of the transaction itself may cost coppers, so you need to have coppers on the server account.
        /// </summary>
        public GenericResponse<int> SendBill(string loginFrom, int amount, string label, string loginTo)
        {
            return (GenericResponse<int>)_client.SendMethod<GenericResponse<int>>(TrackManiaMethod.SendBill.ToString(), loginFrom, amount, label, loginTo);
        }

        /// <summary>
        /// Returns the current state of a bill. This method takes one parameter, the BillId. Returns a struct containing State, StateName and TransactionId. Possible enum values are: CreatingTransaction, Issued, ValidatingPayement, Payed, Refused, Error.
        /// </summary>
        public GenericResponse<BillState> GetBillState(int billID)
        {
            return (GenericResponse<BillState>)_client.SendMethod<GenericResponse<BillState>>(TrackManiaMethod.GetBillState.ToString(), billID);
        }

        /// <summary>
        /// Returns the current number of coppers on the server account.
        /// </summary>
        public GenericResponse<int> GetServerCoppers()
        {
            return (GenericResponse<int>)_client.SendMethod<GenericResponse<int>>(TrackManiaMethod.GetServerCoppers.ToString());
        }

        /// <summary>
        /// Set a new server name in utf8 format.
        /// </summary>
        public GenericResponse<bool> SetServerName(string serverName)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SetServerName.ToString(), serverName);
        }

        /// <summary>
        /// Get the server name in utf8 format.
        /// </summary>
        public GenericResponse<string> GetServerName()
        {
            return (GenericResponse<string>)_client.SendMethod<GenericResponse<string>>(TrackManiaMethod.GetServerName.ToString());
        }

        /// <summary>
        ///  Set a new server comment in utf8 format.
        /// </summary>
        public GenericResponse<bool> SetServerComment(string serverComment)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SetServerComment.ToString(), serverComment);
        }

        /// <summary>
        /// Get the server comment in utf8 format.
        /// </summary>
        public GenericResponse<string> GetServerComment()
        {
            return (GenericResponse<string>)_client.SendMethod<GenericResponse<string>>(TrackManiaMethod.GetServerComment.ToString());
        }

        /// <summary>
        /// Set whether the server should be hidden from the public server list (0 = visible, 1 = always hidden, 2 = hidden from nations).
        /// </summary>
        public GenericResponse<bool> SetHideServer(int hiddenState)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SetHideServer.ToString(), hiddenState);
        }

        /// <summary>
        /// Get whether the server wants to be hidden from the public server list.
        /// </summary>
        public GenericResponse<int> GetHideServer()
        {
            return (GenericResponse<int>)_client.SendMethod<GenericResponse<int>>(TrackManiaMethod.GetHideServer.ToString());
        }

        /// <summary>
        ///  Returns true if this is a relay server.
        /// </summary>
        public GenericResponse<bool> IsRelayServer()
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.IsRelayServer.ToString());
        }

        /// <summary>
        /// Set a new password for the server.
        /// </summary>
        public GenericResponse<bool> SetServerPassword(string serverPassword)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SetServerPassword.ToString(), serverPassword);
        }

        /// <summary>
        /// Get the server password if called as Admin or Super Admin, else returns if a password is needed or not.
        /// </summary>
        public GenericResponse<string> GetServerPassword()
        {
            return (GenericResponse<string>)_client.SendMethod<GenericResponse<string>>(TrackManiaMethod.GetServerPassword.ToString());
        }

        /// <summary>
        /// Set a new password for the spectator mode.
        /// </summary>
        public GenericResponse<bool> SetServerPasswordForSpectator(string serverPassword)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SetServerPasswordForSpectator.ToString(), serverPassword);
        }

        /// <summary>
        /// Get the password for spectator mode if called as Admin or Super Admin, else returns if a password is needed or not.
        /// </summary>
        public GenericResponse<string> GetServerPasswordForSpectator()
        {
            return (GenericResponse<string>)_client.SendMethod<GenericResponse<string>>(TrackManiaMethod.GetServerPasswordForSpectator.ToString());
        }

        /// <summary>
        /// Set a new maximum number of players. Only available to Admin. Requires a challenge restart to be taken into account.
        /// </summary>
        public GenericResponse<bool> SetMaxPlayers(int maxPlayers)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SetMaxPlayers.ToString(), maxPlayers);
        }

        /// <summary>
        /// Set a new maximum number of Spectators. Only available to Admin. Requires a challenge restart to be taken into account.
        /// </summary>
        public GenericResponse<bool> SetMaxSpectators(int maxSpectators)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SetMaxSpectators.ToString(), maxSpectators);
        }

        /// <summary>
        ///  Enable or disable peer-to-peer upload from server.
        /// </summary>
        public GenericResponse<bool> EnableP2PUpload(bool setEnabled)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.EnableP2PUpload.ToString(), setEnabled);
        }

        /// <summary>
        /// Returns if the peer-to-peer upload from server is enabled.
        /// </summary>
        public GenericResponse<bool> IsP2PUpload()
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.IsP2PUpload.ToString());
        }

        /// <summary>
        /// Enable or disable peer-to-peer download for server.
        /// </summary>
        public GenericResponse<bool> EnableP2PDownload(bool setEnabled)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.EnableP2PDownload.ToString(), setEnabled);
        }

        /// <summary>
        /// Returns if the peer-to-peer download for server is enabled.
        /// </summary>
        public GenericResponse<bool> IsP2PDownload()
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.IsP2PDownload.ToString());
        }

        /// <summary>
        /// Allow clients to download challenges from the server.
        /// </summary>
        public GenericResponse<bool> AllowChallengeDownload(bool allow)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.AllowChallengeDownload.ToString(), allow);
        }

        /// <summary>
        /// Returns if clients can download challenges from the server.
        /// </summary>
        public GenericResponse<bool> IsChallengeDownloadAllowed()
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.IsChallengeDownloadAllowed.ToString());
        }

        /// <summary>
        /// Enable the autosaving of all replays (vizualisable replays with all players, but not validable) on the server.
        /// </summary>
        public GenericResponse<bool> AutoSaveReplays(bool setEnabled)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.AutoSaveReplays.ToString(), setEnabled);
        }

        /// <summary>
        /// Returns if autosaving of all replays is enabled on the server.
        /// </summary>
        public GenericResponse<bool> IsAutoSaveReplaysEnabled()
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.IsAutoSaveReplaysEnabled.ToString());
        }

        /// <summary>
        /// Enable the autosaving on the server of validation replays, every time a player makes a new time.
        /// </summary>
        public GenericResponse<bool> AutoSaveValidationReplays(bool setEnabled)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.AutoSaveValidationReplays.ToString(), setEnabled);
        }

        /// <summary>
        /// Returns if autosaving of validation replays is enabled on the server.
        /// </summary>
        public GenericResponse<bool> IsAutoSaveValidationReplaysEnabled()
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.IsAutoSaveValidationReplaysEnabled.ToString());
        }

        /// <summary>
        /// Saves the current replay (vizualisable replays with all players, but not validable). pass a filename, or '' for an automatic filename.
        /// </summary>
        public GenericResponse<bool> SaveCurrentReplay(string filename)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SaveCurrentReplay.ToString(), filename);
        }

        /// <summary>
        /// Saves a replay with the ghost of all the players' best race. First parameter is the login of the player (or '' for all players), Second parameter is the filename, or '' for an automatic filename.
        /// </summary>
        public GenericResponse<bool> SaveBestGhostsReplay(string login, string filename)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SaveBestGhostsReplay.ToString(), login, filename);
        }

        /// <summary>
        /// Returns a replay containing the data needed to validate the current best time of the player.
        /// </summary>
        public GenericResponse<byte[]> GetValidationReplay(string login)
        {
            return (GenericResponse<byte[]>)_client.SendMethod<GenericResponse<byte[]>>(TrackManiaMethod.GetValidationReplay.ToString(), login);
        }

        /// <summary>
        ///  Set a new ladder mode between ladder disabled (0), forced (1). Only available to Admin. Requires a challenge restart to be taken into account.
        /// </summary>
        public GenericResponse<bool> SetLadderMode(int ladderMode)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SetLadderMode.ToString(), ladderMode);
        }

        /// <summary>
        /// Set the network vehicle quality to Fast (0) or High (1). Only available to Admin. Requires a challenge restart to be taken into account.
        /// </summary>
        public GenericResponse<bool> SetVehicleNetQuality(int quality)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SetVehicleNetQuality.ToString(), quality);
        }

        /// <summary>
        /// Defines the packmask of the server. Can be 'United', 'Nations', 'Sunrise', 'Original', or any of the environment names. (Only challenges matching the packmask will be allowed on the server, so that player connecting to it know what to expect.) Only available when the server is stopped.
        /// </summary>
        public GenericResponse<bool> SetServerPackMask(string packmask)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SetServerPackMask.ToString(), packmask);
        }

        /// <summary>
        /// Set the music to play on the clients. Parameters: Override, if true even the challenges with a custom music will be overridden by the server setting, and a UrlOrFileName for the music. Requires a challenge restart to be taken into account.
        /// </summary>
        public GenericResponse<bool> SetForcedMusic(bool doOverride, string urlOrFilename)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SetForcedMusic.ToString(), doOverride, urlOrFilename);
        }

        /// <summary>
        /// Returns the last error message for an internet connection.
        /// </summary>
        public GenericResponse<string> GetLastConnectionErrorMessage()
        {
            return (GenericResponse<string>)_client.SendMethod<GenericResponse<string>>(TrackManiaMethod.GetLastConnectionErrorMessage.ToString());
        }


        /// <summary>
        ///  Set a new password for the referee mode.
        /// </summary>
        public GenericResponse<bool> SetRefereePassword(string password)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SetRefereePassword.ToString(), password);
        }

        /// <summary>
        /// Get the password for referee mode if called as Admin or Super Admin, else returns if a password is needed or not.
        /// </summary>
        public GenericResponse<string> GetRefereePassword()
        {
            return (GenericResponse<string>)_client.SendMethod<GenericResponse<string>>(TrackManiaMethod.GetRefereePassword.ToString());
        }

        /// <summary>
        /// Set the referee validation mode. 0 = validate the top3 players, 1 = validate all players.
        /// </summary>
        public GenericResponse<bool> SetRefereeMode(int refereeMode)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SetRefereeMode.ToString(), refereeMode);
        }

        /// <summary>
        /// Get the referee validation mode.
        /// </summary>
        public GenericResponse<int> GetRefereeMode()
        {
            return (GenericResponse<int>)_client.SendMethod<GenericResponse<int>>(TrackManiaMethod.GetRefereeMode.ToString());
        }

        /// <summary>
        /// Set whether the game should use a variable validation seed or not. Only available to Admin. Requires a challenge restart to be taken into account.
        /// </summary>
        public GenericResponse<bool> SetUseChangingValidationSeed(bool setEnabled)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SetUseChangingValidationSeed.ToString(), setEnabled);
        }

        /// <summary>
        /// Sets whether the server is in warm-up phase or not.
        /// </summary>
        public GenericResponse<bool> SetWarmUp(bool setEnabled)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SetWarmUp.ToString(), setEnabled);
        }

        /// <summary>
        /// Returns whether the server is in warm-up phase.
        /// </summary>
        public GenericResponse<bool> GetWarmUp()
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.GetWarmUp.ToString());
        }

        /// <summary>
        /// estarts the challenge, with an optional boolean parameter DontClearCupScores (only available in cup mode).
        /// </summary>
        public GenericResponse<bool> ChallengeRestart()
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.ChallengeRestart.ToString());
        }

        /// <summary>
        /// Restarts the challenge, with an optional boolean parameter DontClearCupScores (only available in cup mode).
        /// </summary>
        public GenericResponse<bool> RestartChallenge()
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.RestartChallenge.ToString());
        }

        /// <summary>
        /// Switch to next challenge, with an optional boolean parameter DontClearCupScores (only available in cup mode).
        /// </summary>
        public GenericResponse<bool> NextChallenge()
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.NextChallenge.ToString());
        }

        /// <summary>
        ///  Stop the server.
        /// </summary>
        public GenericResponse<bool> StopServer()
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.StopServer.ToString());
        }

        /// <summary>
        ///  In Rounds or Laps mode, force the end of round without waiting for all players to giveup/finish.
        /// </summary>
        public GenericResponse<bool> ForceEndRound()
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.ForceEndRound.ToString());
        }

        /// <summary>
        /// Set a new game mode between Rounds (0), TimeAttack (1), Team (2), Laps (3), Stunts (4) and Cup (5). Only available to Admin. Requires a challenge restart to be taken into account.
        /// </summary>
        public GenericResponse<bool> SetGameMode(int mode)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SetGameMode.ToString(), mode);
        }

        /// <summary>
        ///  Get the current game mode.
        /// </summary>
        public GenericResponse<int> GetGameMode()
        {
            return (GenericResponse<int>)_client.SendMethod<GenericResponse<int>>(TrackManiaMethod.GetGameMode.ToString());
        }

        /// <summary>
        /// Set a new chat time value in milliseconds (actually 'chat time' is the duration of the end race podium, 0 means no podium displayed.).
        /// </summary>
        public GenericResponse<bool> SetChatTime(int timeInMilliseconds)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SetChatTime.ToString(), timeInMilliseconds);
        }

        /// <summary>
        /// Get the current and next chat time. The struct returned contains two fields CurrentValue and NextValue.
        /// </summary>
        public GenericResponse<CNPair<int>> GetChatTime()
        {
            return (GenericResponse<CNPair<int>>)_client.SendMethod<GenericResponse<CNPair<int>>>(TrackManiaMethod.SetChatTime.ToString());
        }

        /// <summary>
        /// Set a new finish timeout (for rounds/laps mode) value in milliseconds. 0 means default. 1 means adaptative to the duration of the challenge. Only available to Admin. Requires a challenge restart to be taken into account.
        /// </summary>
        public GenericResponse<bool> SetFinishTimeout(int timeInMilliseconds)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SetFinishTimeout.ToString(), timeInMilliseconds);
        }

        /// <summary>
        /// Get the current and next FinishTimeout. The struct returned contains two fields CurrentValue and NextValue.
        /// </summary>
        public GenericResponse<CNPair<int>> GetFinishTimeout()
        {
            return (GenericResponse<CNPair<int>>)_client.SendMethod<GenericResponse<CNPair<int>>>(TrackManiaMethod.GetFinishTimeout.ToString());
        }

        /// <summary>
        /// Set whether to enable the automatic warm-up phase in all modes. 0 = no, otherwise it's the duration of the phase, expressed in number of rounds (in rounds/team mode), or in number of times the gold medal time (other modes). Only available to Admin. Requires a challenge restart to be taken into account.
        /// </summary>
        public GenericResponse<bool> SetAllWarmUpDuration(int duration)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SetAllWarmUpDuration.ToString(), duration);
        }

        /// <summary>
        ///  Get whether the automatic warm-up phase is enabled in all modes. The struct returned contains two fields CurrentValue and NextValue.
        /// </summary>
        public GenericResponse<CNPair<int>> GetAllWarmUpDuration()
        {
            return (GenericResponse<CNPair<int>>)_client.SendMethod<GenericResponse<CNPair<int>>>(TrackManiaMethod.GetAllWarmUpDuration.ToString());
        }

        /// <summary>
        /// Set whether to disallow players to respawn. Only available to Admin. Requires a challenge restart to be taken into account.
        /// </summary>
        public GenericResponse<bool> SetDisableRespawn(bool setEnabled)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SetDisableRespawn.ToString(), setEnabled);
        }

        /// <summary>
        /// Get whether players are disallowed to respawn. The struct returned contains two fields CurrentValue and NextValue.
        /// </summary>
        public GenericResponse<CNPair<bool>> GetDisableRespawn()
        {
            return (GenericResponse<CNPair<bool>>)_client.SendMethod<GenericResponse<CNPair<bool>>>(TrackManiaMethod.GetDisableRespawn.ToString());
        }

        /// <summary>
        /// Set whether to override the players preferences and always display all opponents (0=no override, 1=show all, other value=minimum number of opponents). Only available to Admin. Requires a challenge restart to be taken into account.
        /// </summary>
        public GenericResponse<bool> SetForceShowAllOpponents(int mode)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SetForceShowAllOpponents.ToString(), mode);
        }

        /// <summary>
        /// Get whether players are forced to show all opponents. The struct returned contains two fields CurrentValue and NextValue.
        /// </summary>
        public GenericResponse<CNPair<int>> GetForceShowAllOpponents()
        {
            return (GenericResponse<CNPair<int>>)_client.SendMethod<GenericResponse<CNPair<int>>>(TrackManiaMethod.GetForceShowAllOpponents.ToString());
        }

        /// <summary>
        /// Set a new time limit for time attack mode. Only available to Admin. Requires a challenge restart to be taken into account.
        /// </summary>
        public GenericResponse<bool> SetTimeAttackLimit(int timeLimitInMilliseconds)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SetTimeAttackLimit.ToString(), timeLimitInMilliseconds);
        }

        /// <summary>
        /// Get the current and next time limit for time attack mode. The struct returned contains two fields CurrentValue and NextValue.
        /// </summary>
        public GenericResponse<CNPair<int>> GetTimeAttackLimit()
        {
            return (GenericResponse<CNPair<int>>)_client.SendMethod<GenericResponse<CNPair<int>>>(TrackManiaMethod.GetTimeAttackLimit.ToString());
        }

        /// <summary>
        /// Set a new synchronized start period for time attack mode. Only available to Admin. Requires a challenge restart to be taken into account.
        /// </summary>
        public GenericResponse<bool> SetTimeAttackSynchStartPeriod(int periodInMilliseconds)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SetTimeAttackSynchStartPeriod.ToString(), periodInMilliseconds);
        }

        /// <summary>
        /// Get the current and synchronized start period for time attack mode. The struct returned contains two fields CurrentValue and NextValue.
        /// </summary>
        public GenericResponse<CNPair<int>> GetTimeAttackSynchStartPeriod()
        {
            return (GenericResponse<CNPair<int>>)_client.SendMethod<GenericResponse<CNPair<int>>>(TrackManiaMethod.GetTimeAttackSynchStartPeriod.ToString());
        }

        /// <summary>
        /// Set a new time limit for laps mode. Only available to Admin. Requires a challenge restart to be taken into account.
        /// </summary>
        public GenericResponse<bool> SetLapsTimeLimit(int timeLimitInMilliseconds)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SetLapsTimeLimit.ToString(), timeLimitInMilliseconds);
        }

        /// <summary>
        /// Get the current and next time limit for laps mode. The struct returned contains two fields CurrentValue and NextValue.
        /// </summary>
        public GenericResponse<CNPair<int>> GetLapsTimeLimit()
        {
            return (GenericResponse<CNPair<int>>)_client.SendMethod<GenericResponse<CNPair<int>>>(TrackManiaMethod.GetLapsTimeLimit.ToString());
        }

        /// <summary>
        /// Set a new number of laps for laps mode. Only available to Admin. Requires a challenge restart to be taken into account.
        /// </summary>
        public GenericResponse<bool> SetNbLaps(int numberOfLaps)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SetNbLaps.ToString(), numberOfLaps);
        }

        /// <summary>
        /// Get the current and next number of laps for laps mode. The struct returned contains two fields CurrentValue and NextValue.
        /// </summary>
        public GenericResponse<CNPair<int>> GetNbLaps()
        {
            return (GenericResponse<CNPair<int>>)_client.SendMethod<GenericResponse<CNPair<int>>>(TrackManiaMethod.GetNbLaps.ToString());
        }

        /// <summary>
        /// Set a new number of laps for rounds mode (0 = default, use the number of laps from the challenges, otherwise forces the number of rounds for multilaps challenges). Only available to Admin. Requires a challenge restart to be taken into account.
        /// </summary>
        public GenericResponse<bool> SetRoundForcedLaps(int numberOfLaps)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SetRoundForcedLaps.ToString(), numberOfLaps);
        }

        /// <summary>
        /// Get the current and next number of laps for rounds mode. The struct returned contains two fields CurrentValue and NextValue.
        /// </summary>
        public GenericResponse<CNPair<int>> GetRoundForcedLaps()
        {
            return (GenericResponse<CNPair<int>>)_client.SendMethod<GenericResponse<CNPair<int>>>(TrackManiaMethod.GetRoundForcedLaps.ToString());
        }

        /// <summary>
        /// Set a new points limit for rounds mode (value set depends on UseNewRulesRound). Only available to Admin. Requires a challenge restart to be taken into account.
        /// </summary>
        public GenericResponse<bool> SetRoundPointsLimit(int pointLimit)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SetRoundPointsLimit.ToString(), pointLimit);
        }

        /// <summary>
        /// Get the current and next points limit for rounds mode (values returned depend on UseNewRulesRound). The struct returned contains two fields CurrentValue and NextValue.
        /// </summary>
        public GenericResponse<CNPair<int>> GetRoundPointsLimit()
        {
            return (GenericResponse<CNPair<int>>)_client.SendMethod<GenericResponse<CNPair<int>>>(TrackManiaMethod.GetRoundPointsLimit.ToString());
        }

        /// <summary>
        /// Set if new rules are used for rounds mode. Only available to Admin. Requires a challenge restart to be taken into account.
        /// </summary>
        public GenericResponse<bool> SetUseNewRulesRound(bool setEnabled)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SetUseNewRulesRound.ToString(), setEnabled);
        }

        /// <summary>
        /// Get if the new rules are used for rounds mode (Current and next values). The struct returned contains two fields CurrentValue and NextValue.
        /// </summary>
        public GenericResponse<CNPair<bool>> GetUseNewRulesRound()
        {
            return (GenericResponse<CNPair<bool>>)_client.SendMethod<GenericResponse<CNPair<bool>>>(TrackManiaMethod.GetUseNewRulesRound.ToString());
        }

        /// <summary>
        /// Set a new points limit for team mode (value set depends on UseNewRulesTeam). Only available to Admin. Requires a challenge restart to be taken into account.
        /// </summary>
        public GenericResponse<bool> SetTeamPointsLimit(int pointLimit)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SetTeamPointsLimit.ToString(), pointLimit);
        }

        /// <summary>
        /// Get the current and next points limit for team mode (values returned depend on UseNewRulesTeam). The struct returned contains two fields CurrentValue and NextValue.
        /// </summary>
        public GenericResponse<CNPair<int>> GetTeamPointsLimit()
        {
            return (GenericResponse<CNPair<int>>)_client.SendMethod<GenericResponse<CNPair<int>>>(TrackManiaMethod.GetTeamPointsLimit.ToString());
        }

        /// <summary>
        /// Set a new number of maximum points per round for team mode. Only available to Admin. Requires a challenge restart to be taken into account.
        /// </summary>
        public GenericResponse<bool> SetMaxPointsTeam(int pointLimit)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SetMaxPointsTeam.ToString(), pointLimit);
        }

        /// <summary>
        /// Get the current and next number of maximum points per round for team mode. The struct returned contains two fields CurrentValue and NextValue.
        /// </summary>
        public GenericResponse<CNPair<int>> GetMaxPointsTeam()
        {
            return (GenericResponse<CNPair<int>>)_client.SendMethod<GenericResponse<CNPair<int>>>(TrackManiaMethod.GetMaxPointsTeam.ToString());
        }

        /// <summary>
        /// Set if new rules are used for team mode. Only available to Admin. Requires a challenge restart to be taken into account.
        /// </summary>
        public GenericResponse<bool> SetUseNewRulesTeam(bool setEnabled)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SetUseNewRulesTeam.ToString(), setEnabled);
        }

        /// <summary>
        /// Get if the new rules are used for team mode (Current and next values). The struct returned contains two fields CurrentValue and NextValue.
        /// </summary>
        public GenericResponse<CNPair<bool>> GetUseNewRulesTeam()
        {
            return (GenericResponse<CNPair<bool>>)_client.SendMethod<GenericResponse<CNPair<bool>>>(TrackManiaMethod.GetUseNewRulesTeam.ToString());
        }

        /// <summary>
        /// Set the points needed for victory in Cup mode. Only available to Admin. Requires a challenge restart to be taken into account.
        /// </summary>
        public GenericResponse<bool> SetCupPointsLimit(int pointLimit)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SetCupPointsLimit.ToString(), pointLimit);
        }

        /// <summary>
        /// Get the points needed for victory in Cup mode. The struct returned contains two fields CurrentValue and NextValue.
        /// </summary>
        public GenericResponse<CNPair<int>> GetCupPointsLimit()
        {
            return (GenericResponse<CNPair<int>>)_client.SendMethod<GenericResponse<CNPair<int>>>(TrackManiaMethod.GetCupPointsLimit.ToString());
        }

        /// <summary>
        /// Sets the number of rounds before going to next challenge in Cup mode. Only available to Admin. Requires a challenge restart to be taken into account.
        /// </summary>
        public GenericResponse<bool> SetCupRoundsPerChallenge(int numberOfRounds)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SetCupRoundsPerChallenge.ToString(), numberOfRounds);
        }

        /// <summary>
        /// Get the number of rounds before going to next challenge in Cup mode. The struct returned contains two fields CurrentValue and NextValue.
        /// </summary>
        public GenericResponse<CNPair<int>> GetCupRoundsPerChallenge()
        {
            return (GenericResponse<CNPair<int>>)_client.SendMethod<GenericResponse<CNPair<int>>>(TrackManiaMethod.GetCupRoundsPerChallenge.ToString());
        }

        /// <summary>
        /// Set whether to enable the automatic warm-up phase in Cup mode. 0 = no, otherwise it's the duration of the phase, expressed in number of rounds. Only available to Admin. Requires a challenge restart to be taken into account.
        /// </summary>
        public GenericResponse<bool> SetCupWarmUpDuration(int numberOfRounds)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SetCupWarmUpDuration.ToString(), numberOfRounds);
        }

        /// <summary>
        /// Get whether the automatic warm-up phase is enabled in Cup mode. The struct returned contains two fields CurrentValue and NextValue.
        /// </summary>
        public GenericResponse<CNPair<int>> GetCupWarmUpDuration()
        {
            return (GenericResponse<CNPair<int>>)_client.SendMethod<GenericResponse<CNPair<int>>>(TrackManiaMethod.GetCupWarmUpDuration.ToString());
        }

        /// <summary>
        /// Set the number of winners to determine before the match is considered over. Only available to Admin. Requires a challenge restart to be taken into account.
        /// </summary>
        public GenericResponse<bool> SetCupNbWinners(int numberOfWinners)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SetCupNbWinners.ToString(), numberOfWinners);
        }

        /// <summary>
        /// Get the number of winners to determine before the match is considered over. The struct returned contains two fields CurrentValue and NextValue.
        /// </summary>
        public GenericResponse<CNPair<int>> GetCupNbWinners()
        {
            return (GenericResponse<CNPair<int>>)_client.SendMethod<GenericResponse<CNPair<int>>>(TrackManiaMethod.GetCupNbWinners.ToString());
        }

        /// <summary>
        /// Returns the current challenge index in the selection, or -1 if the challenge is no longer in the selection.
        /// </summary>
        public GenericResponse<int> GetCurrentChallengeIndex()
        {
            return (GenericResponse<int>)_client.SendMethod<GenericResponse<int>>(TrackManiaMethod.GetCurrentChallengeIndex.ToString());
        }

        /// <summary>
        /// Sets the challenge index in the selection that will be played next (unless the current one is restarted...)
        /// </summary>
        public GenericResponse<bool> SetNextChallengeIndex()
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SetNextChallengeIndex.ToString());
        }

        /// <summary>
        /// Returns a boolean if the challenge with the specified filename matches the current server settings.
        /// </summary>
        public GenericResponse<bool> CheckChallengeForCurrentServerParams(string filename)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.CheckChallengeForCurrentServerParams.ToString(), filename);
        }

        /// <summary>
        /// Add the challenge with the specified filename at the end of the current selection.
        /// </summary>
        public GenericResponse<bool> AddChallenge(string filename)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.AddChallenge.ToString(), filename);
        }

        /// <summary>
        ///  Add the list of challenges with the specified filenames at the end of the current selection. The list of challenges to add is an array of strings.
        /// </summary>
        public GenericResponse<bool> AddChallengeList(string[] filenames)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.AddChallengeList.ToString(), filenames);
        }

        /// <summary>
        /// Remove the challenge with the specified filename from the current selection. Only available to Admin.
        /// </summary>
        public GenericResponse<bool> RemoveChallenge(string filename)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.RemoveChallenge.ToString(), filename);
        }

        /// <summary>
        /// Remove the list of challenges with the specified filenames from the current selection. The list of challenges to remove is an array of strings.
        /// </summary>
        public GenericResponse<bool> RemoveChallengeList(string[] filenames)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.RemoveChallengeList.ToString(), filenames);
        }

        /// <summary>
        /// Insert the challenge with the specified filename after the current challenge.
        /// </summary>
        public GenericResponse<bool> InsertChallenge(string filename)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.InsertChallenge.ToString(), filename);
        }

        /// <summary>
        /// Insert the list of challenges with the specified filenames after the current challenge. The list of challenges to insert is an array of strings.
        /// </summary>
        public GenericResponse<bool> InsertChallengeList(string[] filenames)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.InsertChallengeList.ToString(), filenames);
        }

        /// <summary>
        /// Set as next challenge the one with the specified filename, if it is present in the selection.
        /// </summary>
        public GenericResponse<bool> ChooseNextChallenge(string filename)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.ChooseNextChallenge.ToString(), filename);
        }

        /// <summary>
        ///  Set as next challenges the list of challenges with the specified filenames, if they are present in the selection. The list of challenges to choose is an array of strings.
        /// </summary>
        public GenericResponse<bool> ChooseNextChallengeList(string[] filenames)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.ChooseNextChallengeList.ToString(), filenames);
        }

        /// <summary>
        /// Set a list of challenges defined in the playlist with the specified filename as the current selection of the server, and load the gameinfos from the same file
        /// </summary>
        public GenericResponse<int> LoadMatchSettings(string filename)
        {
            return (GenericResponse<int>)_client.SendMethod<GenericResponse<int>>(TrackManiaMethod.LoadMatchSettings.ToString(), filename);
        }

        /// <summary>
        /// Add a list of challenges defined in the playlist with the specified filename at the end of the current selection.
        /// </summary>
        public GenericResponse<int> AppendPlaylistFromMatchSettings(string filename)
        {
            return (GenericResponse<int>)_client.SendMethod<GenericResponse<int>>(TrackManiaMethod.AppendPlaylistFromMatchSettings.ToString(), filename);
        }

        /// <summary>
        /// Save the current selection of challenge in the playlist with the specified filename, as well as the current gameinfos.
        /// </summary>
        public GenericResponse<int> SaveMatchSettings(string filename)
        {
            return (GenericResponse<int>)_client.SendMethod<GenericResponse<int>>(TrackManiaMethod.SaveMatchSettings.ToString(), filename);
        }

        /// <summary>
        /// Insert a list of challenges defined in the playlist with the specified filename after the current challenge.
        /// </summary>
        public GenericResponse<int> InsertPlaylistFromMatchSettings(string filename)
        {
            return (GenericResponse<int>)_client.SendMethod<GenericResponse<int>>(TrackManiaMethod.InsertPlaylistFromMatchSettings.ToString(), filename);
        }

        /// <summary>
        /// Force the team of the player. Only available in team mode. You have to pass the login and the team number (0 or 1).
        /// </summary>
        public GenericResponse<bool> ForcePlayerTeam(string login, int team)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.ForcePlayerTeam.ToString(), login, team);
        }

        /// <summary>
        /// Force the team of the player. Only available in team mode. You have to pass the playerid and the team number (0 or 1).
        /// </summary>
        public GenericResponse<bool> ForcePlayerTeamId(int userID, int team)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.ForcePlayerTeamId.ToString(), userID, team);
        }

        /// <summary>
        /// Force the spectating status of the player. You have to pass the login and the spectator mode (0: user selectable, 1: spectator, 2: player).
        /// </summary>
        public GenericResponse<bool> ForceSpectator(string login, int spectatorMode)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.ForceSpectator.ToString(), login, spectatorMode);
        }

        /// <summary>
        /// Force the spectating status of the player. You have to pass the playerid and the spectator mode (0: user selectable, 1: spectator, 2: player).
        /// </summary>
        public GenericResponse<bool> ForceSpectatorId(int userID, int spectatorMode)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.ForceSpectatorId.ToString(), userID, spectatorMode);
        }

        /// <summary>
        /// Force spectators to look at a specific player. You have to pass the login of the spectator (or '' for all) and the login of the target (or '' for automatic), and an integer for the camera type to use (-1 = leave unchanged, 0 = replay, 1 = follow, 2 = free).
        /// </summary>
        public GenericResponse<bool> ForceSpectatorTarget(string spectatorLogin, string targetLogin, int camType)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.ForceSpectatorTarget.ToString(), spectatorLogin, targetLogin, camType);
        }

        /// <summary>
        /// Force spectators to look at a specific player. You have to pass the id of the spectator (or -1 for all) and the id of the target (or -1 for automatic), and an integer for the camera type to use (-1 = leave unchanged, 0 = replay, 1 = follow, 2 = free).
        /// </summary>
        public GenericResponse<bool> ForceSpectatorTargetId(int spectatorUserID, string targetUserID, int camType)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.ForceSpectatorTargetId.ToString(), spectatorUserID, targetUserID, camType);
        }

        /// <summary>
        /// Enable control of the game flow: the game will wait for the caller to validate state transitions.
        /// </summary>
        public GenericResponse<bool> ManualFlowControlEnable(bool setEnabled)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.ManualFlowControlEnable.ToString(), setEnabled);
        }

        /// <summary>
        /// Allows the game to proceed.
        /// </summary>
        public GenericResponse<bool> ManualFlowControlProceed()
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.ManualFlowControlProceed.ToString());
        }

        /// <summary>
        /// Returns whether the manual control of the game flow is enabled. 0 = no, 1 = yes by the xml-rpc client making the call, 2 = yes, by some other xml-rpc client.
        /// </summary>
        public GenericResponse<int> ManualFlowControlIsEnabled()
        {
            return (GenericResponse<int>)_client.SendMethod<GenericResponse<int>>(TrackManiaMethod.ManualFlowControlIsEnabled.ToString());
        }

        /// <summary>
        /// Returns the transition that is currently blocked, or '' if none. (That's exactly the value last received by the callback.)
        /// </summary>
        public GenericResponse<string> ManualFlowControlGetCurTransition()
        {
            return (GenericResponse<string>)_client.SendMethod<GenericResponse<string>>(TrackManiaMethod.ManualFlowControlGetCurTransition.ToString());
        }

        /// <summary>
        /// Returns whether the current match ending condition. Return values are: 'Playing', 'ChangeMap' or 'Finished'.
        /// </summary>
        public GenericResponse<string> CheckEndMatchCondition()
        {
            return (GenericResponse<string>)_client.SendMethod<GenericResponse<string>>(TrackManiaMethod.CheckEndMatchCondition.ToString());
        }

        /// <summary>
        /// Start a server on lan, using the current configuration.
        /// </summary>
        public GenericResponse<bool> StartServerLan()
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.StartServerLan.ToString());
        }

        /// <summary>
        /// Start a server on internet using the 'Login' and 'Password' specified in the struct passed as parameters.
        /// </summary>
        public GenericResponse<bool> StartServerInternet()
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.StartServerInternet.ToString());
        }

        /// <summary>
        /// Quit the application.
        /// </summary>
        public GenericResponse<bool> QuitGame()
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.QuitGame.ToString());
        }

        /// <summary>
        /// Returns the path of the game datas directory.
        /// </summary>
        public GenericResponse<string> GameDataDirectory()
        {
            return (GenericResponse<string>)_client.SendMethod<GenericResponse<string>>(TrackManiaMethod.GameDataDirectory.ToString());
        }

        /// <summary>
        /// Returns the path of the tracks directory.
        /// </summary>
        public GenericResponse<string> GetTracksDirectory()
        {
            return (GenericResponse<string>)_client.SendMethod<GenericResponse<string>>(TrackManiaMethod.GetTracksDirectory.ToString());
        }

        /// <summary>
        /// Returns the path of the skins directory.
        /// </summary>
        public GenericResponse<string> GetSkinsDirectory()
        {
            return (GenericResponse<string>)_client.SendMethod<GenericResponse<string>>(TrackManiaMethod.GetSkinsDirectory.ToString());
        }

        /// <summary>
        /// Change the password for the specified login/user.
        /// </summary>
        public GenericResponse<bool> ChangeAuthPassword(string login, string password)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.ChangeAuthPassword.ToString(), login, password);
        }

        /// <summary>
        /// Call a vote for a cmd. The command is a XML string corresponding to an XmlRpc request.
        /// </summary>
        public GenericResponse<bool> CallVote(string commandXML)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.ChangeAuthPassword.ToString(), commandXML);
        }

        /// <summary>
        /// Extended call vote. Same as CallVote, but you can additionally supply specific parameters for this vote: a ratio, a time out and who is voting. Special timeout values: a timeout of '0' means default, '1' means indefinite; a ratio of '-1' means default; Voters values: '0' means only active players, '1' means any player, '2' is for everybody, pure spectators included.
        /// </summary>
        public GenericResponse<bool> CallVoteEx(string commandXML, double ratio, int timeoutInMilliseconds, int userID)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.CallVoteEx.ToString(), commandXML, ratio, timeoutInMilliseconds, userID);
        }

        /// <summary>
        /// Used internally by game.
        /// </summary>
        public GenericResponse<bool> InternalCallVote()
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.InternalCallVote.ToString());
        }

        /// <summary>
        /// Cancel the current vote.
        /// </summary>
        public GenericResponse<bool> CancelVote()
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.CancelVote.ToString());
        }

        /// <summary>
        /// Set a new timeout for waiting for votes. A zero value disables callvote. Only available to Admin. Requires a challenge restart to be taken into account.
        /// </summary>
        public GenericResponse<bool> SetCallVoteTimeOut(int timeoutInMilliseconds)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SetCallVoteTimeOut.ToString(), timeoutInMilliseconds);
        }

        /// <summary>
        /// Get the current and next timeout for waiting for votes. The struct returned contains two fields 'CurrentValue' and 'NextValue'.
        /// </summary>
        public GenericResponse<CNPair<int>> GetCallVoteTimeOut()
        {
            return (GenericResponse<CNPair<int>>)_client.SendMethod<GenericResponse<CNPair<int>>>(TrackManiaMethod.GetCallVoteTimeOut.ToString());
        }

        /// <summary>
        /// Set a new default ratio for passing a vote. Must lie between 0 and 1.
        /// </summary>
        public GenericResponse<bool> SetCallVoteRatio(double ratio)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SetCallVoteRatio.ToString(), ratio);
        }

        /// <summary>
        /// Get the current default ratio for passing a vote. This value lies between 0 and 1.
        /// </summary>
        public GenericResponse<double> SetCallVoteRatio()
        {
            return (GenericResponse<double>)_client.SendMethod<GenericResponse<double>>(TrackManiaMethod.SetCallVoteRatio.ToString());
        }

        /// <summary>
        /// The chat messages are no longer dispatched to the players, they only go to the rpc callback and the controller has to manually forward them.
        /// </summary>
        public GenericResponse<bool> ChatEnableManualRouting(bool setEnabled)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.ChatEnableManualRouting.ToString(), setEnabled);
        }

        /// <summary>
        /// Send a text message to the specified DestLogin (or everybody if empty) on behalf of SenderLogin. Only available if manual routing is enabled.
        /// </summary>
        public GenericResponse<bool> ChatForwardToLogin(string text, string senderLogin, string destinationLogin)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.ChatForwardToLogin.ToString(), text, senderLogin, destinationLogin);
        }

        /// <summary>
        /// Get the current and next maximum number of players allowed on server. The struct returned contains two fields CurrentValue and NextValue.
        /// </summary>
        public GenericResponse<CNPair<int>> GetMaxPlayers()
        {
            return (GenericResponse<CNPair<int>>)_client.SendMethod<GenericResponse<CNPair<int>>>(TrackManiaMethod.GetMaxPlayers.ToString());
        }

        /// <summary>
        ///  Get the current and next maximum number of Spectators allowed on server. The struct returned contains two fields CurrentValue and NextValue.
        /// </summary>
        public GenericResponse<CNPair<int>> GetMaxSpectators()
        {
            return (GenericResponse<CNPair<int>>)_client.SendMethod<GenericResponse<CNPair<int>>>(TrackManiaMethod.GetMaxSpectators.ToString());
        }

        /// <summary>
        /// Get the current and next ladder mode on server. The struct returned contains two fields CurrentValue and NextValue.
        /// </summary>
        public GenericResponse<CNPair<int>> GetLadderMode()
        {
            return (GenericResponse<CNPair<int>>)_client.SendMethod<GenericResponse<CNPair<int>>>(TrackManiaMethod.GetLadderMode.ToString());
        }

        /// <summary>
        /// Get the ladder points limit for the players allowed on this server. The struct returned contains two fields LadderServerLimitMin and LadderServerLimitMax.
        /// </summary>
        public GenericResponse<CNPair<int>> GetLadderServerLimits()
        {
            return (GenericResponse<CNPair<int>>)_client.SendMethod<GenericResponse<CNPair<int>>>(TrackManiaMethod.GetLadderServerLimits.ToString());
        }

        /// <summary>
        /// Get the current and next network vehicle quality on server. The struct returned contains two fields CurrentValue and NextValue.
        /// </summary>
        public GenericResponse<CNPair<int>> GetVehicleNetQuality()
        {
            return (GenericResponse<CNPair<int>>)_client.SendMethod<GenericResponse<CNPair<int>>>(TrackManiaMethod.GetVehicleNetQuality.ToString());
        }

        /// <summary>
        /// Send a text message to all clients. Only available to Admin.
        /// </summary>
        /// <param name="message"></param>
        public GenericResponse<bool> ChatSend(string message)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.ChatSend.ToString(), message);
        }

        /// <summary>
        /// Send a text message to the client with the specified login. Only available to Admin.
        /// </summary>
        /// <param name="login"></param>
        /// <param name="message"></param>
        public GenericResponse<bool> ChatSendToLogin(string message, string login)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.ChatSendToLogin.ToString(), message, login);
        }

        /// <summary>
        /// Send a text message to the client with the specified PlayerId. Only available to Admin.
        /// </summary>
        /// <param name="playerID"></param>
        /// <param name="message"></param>
        public GenericResponse<bool> ChatSendToID(string message, int playerID)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.ChatSendToId.ToString(), message, playerID);
        }

        /// <summary>
        /// Send a text message to all clients without the server login. Only available to Admin.
        /// </summary>
        /// <param name="message"></param>
        public GenericResponse<bool> ChatSendServerMessage(string message)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.ChatSendServerMessage.ToString(), message);
        }

        /// <summary>
        /// Send a text message without the server login to the client with the specified login. Only available to Admin.
        /// </summary>
        /// <param name="login"></param>
        /// <param name="message"></param>
        public GenericResponse<bool> ChatSendServerMessageToLogin(string message, string login)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.ChatSendServerMessageToLogin.ToString(), message, login);
        }

        /// <summary>
        /// Send a text message without the server login to the client with the specified PlayerId. Only available to Admin.
        /// </summary>
        /// <param name="playerID"></param>
        /// <param name="message"></param>
        public GenericResponse<bool> ChatSendServerMessageToId(string message, int playerID)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.ChatSendServerMessageToId.ToString(), message, playerID);
        }

        /// <summary>
        /// Returns the last chat lines. Maximum of 40 lines. Only available to Admin.
        /// </summary>
        public GenericListResponse<string> GetChatLines()
        {
            return (GenericListResponse<string>)_client.SendMethod<GenericListResponse<string>>(TrackManiaMethod.GetChatLines.ToString());
        }

        /// <summary>
        /// Display a notice on all clients. The parameters are the text message to display, and the login of the avatar to display next to it (or '' for no avatar), and an optional 'max duration' in seconds (default: 3). 
        /// </summary>
        public GenericResponse<bool> SendNotice(string message)
        {
            return SendNotice(message, string.Empty);
        }

        /// <summary>
        /// Display a notice on all clients. The parameters are the text message to display, and the login of the avatar to display next to it (or '' for no avatar), and an optional 'max duration' in seconds (default: 3). 
        /// </summary>
        public GenericResponse<bool> SendNotice(string message, string avatarLogin)
        {
            return SendNotice(message, avatarLogin, 3);
        }

        /// <summary>
        /// Display a notice on all clients. The parameters are the text message to display, and the login of the avatar to display next to it (or '' for no avatar), and an optional 'max duration' in seconds (default: 3). 
        /// </summary>
        public GenericResponse<bool> SendNotice(string message, string avatarLogin, int maxDurationInSeconds)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SendNotice.ToString(), message, avatarLogin, maxDurationInSeconds);
        }

        /// <summary>
        /// Display a notice on the client with the specified UId. The parameters are the Uid of the client to whom the notice is sent, the text message to display, and the UId of the avatar to display next to it (or '255' for no avatar).
        /// </summary>
        public GenericResponse<bool> SendNoticeToId(int userID, string message)
        {
            return SendNoticeToId(userID, message, 255);
        }

        /// <summary>
        /// Display a notice on the client with the specified UId. The parameters are the Uid of the client to whom the notice is sent, the text message to display, and the UId of the avatar to display next to it (or '255' for no avatar).
        /// </summary>
        public GenericResponse<bool> SendNoticeToId(int userID, string message, int avatarUserID)
        {
            return SendNoticeToId(userID, message, avatarUserID, 3);
        }

        /// <summary>
        /// Display a notice on the client with the specified UId. The parameters are the Uid of the client to whom the notice is sent, the text message to display, and the UId of the avatar to display next to it (or '255' for no avatar), and an optional 'max duration' in seconds (default: 3). Only available to Admin.
        /// </summary>
        public GenericResponse<bool> SendNoticeToId(int userID, string message, int avatarUserID, int maxDurationInSeconds)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SendNoticeToId.ToString(), userID, message, avatarUserID, maxDurationInSeconds);
        }

        /// <summary>
        /// Display a notice on the client with the specified login. The parameters are the login of the client to whom the notice is sent, the text message to display, and the login of the avatar to display next to it (or '' for no avatar), and an optional 'max duration' in seconds (default: 3). 
        /// </summary>
        public GenericResponse<bool> SendNoticeToLogin(string login, string message)
        {
            return SendNoticeToLogin(login, message, string.Empty);
        }

        /// <summary>
        /// Display a notice on the client with the specified login. The parameters are the login of the client to whom the notice is sent, the text message to display, and the login of the avatar to display next to it (or '' for no avatar), and an optional 'max duration' in seconds (default: 3). 
        /// </summary>
        public GenericResponse<bool> SendNoticeToLogin(string login, string message, string avatarLogin)
        {
            return SendNoticeToLogin(login, message, avatarLogin, 3);
        }

        /// <summary>
        /// Display a notice on the client with the specified login. The parameters are the login of the client to whom the notice is sent, the text message to display, and the login of the avatar to display next to it (or '' for no avatar), and an optional 'max duration' in seconds (default: 3). 
        /// </summary>
        public GenericResponse<bool> SendNoticeToLogin(string login, string message, string avatarLogin, int maxDurationInSeconds)
        {
            return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SendNoticeToLogin.ToString(), login, message, avatarLogin, maxDurationInSeconds);
        }

		/// <summary>
		/// Returns the current status of the server.
		/// </summary>
		public GenericResponse<ServerStatus> GetStatus()
		{
			return (GenericResponse<ServerStatus>)_client.SendMethod<GenericResponse<ServerStatus>>(TrackManiaMethod.GetStatus.ToString());
		}

		/// <summary>
		/// Returns a struct containing the networks stats of the server. The structure contains the following fields : Uptime, NbrConnection, MeanConnectionTime, MeanNbrPlayer, RecvNetRate, SendNetRate, TotalReceivingSize, TotalSendingSize and an array of structures named PlayerNetInfos. Each structure of the array PlayerNetInfos contains the following fields : Login, IPAddress, LastTransferTime, DeltaBetweenTwoLastNetState, PacketLossRate.
		/// </summary>
		public GenericResponse<NetworkStatus> GetNetworkStats()
		{
			return (GenericResponse<NetworkStatus>)_client.SendMethod<GenericResponse<NetworkStatus>>(TrackManiaMethod.GetNetworkStats.ToString());
		}

		/// <summary>
		/// Returns a struct containing the infos for the current challenge. The struct contains the following fields : Name, UId, FileName, Author, Environnement, Mood, BronzeTime, SilverTime, GoldTime, AuthorTime, CopperPrice and LapRace.
		/// </summary>
		public GenericResponse<ChallengeInfo> GetCurrentChallengeInfo()
		{
			return (GenericResponse<ChallengeInfo>)_client.SendMethod<GenericResponse<ChallengeInfo>>(TrackManiaMethod.GetCurrentChallengeInfo.ToString());
		}

		/// <summary>
		/// Returns a struct containing the infos for the next challenge. The struct contains the following fields : Name, UId, FileName, Author, Environnement, Mood, BronzeTime, SilverTime, GoldTime, AuthorTime, CopperPrice and LapRace.
		/// </summary>
		public GenericResponse<ChallengeInfo> GetNextChallengeInfo()
		{
			return (GenericResponse<ChallengeInfo>)_client.SendMethod<GenericResponse<ChallengeInfo>>(TrackManiaMethod.GetNextChallengeInfo.ToString());
		}

		/// <summary>
		/// Returns a struct containing the infos for the challenge with the specified filename. The struct contains the following fields : Name, UId, FileName, Author, Environnement, Mood, BronzeTime, SilverTime, GoldTime, AuthorTime, CopperPrice and LapRace.
		/// </summary>
		public GenericResponse<ChallengeInfo> GetChallengeInfo(string filename)
		{
			return (GenericResponse<ChallengeInfo>)_client.SendMethod<GenericResponse<ChallengeInfo>>(TrackManiaMethod.GetChallengeInfo.ToString(), filename);
		}

		/// <summary>
		/// Returns the current ranking for the race in progress. This method take two parameters. The first parameter specifies the maximum number of infos to be returned, and the second one the starting index in the ranking. The ranking returned is a list of structure. Each structure contains the following fields : Login, NickName, PlayerId, Rank, BestTime, Score, NbrLapsFinished and LadderScore. it also contains an array BestCheckpoints that contains the checkpoints times for the best race.
		/// </summary>
		public GenericListResponse<PlayerRank> GetCurrentRanking(int maxEntriestToReceive, int startingIndex)
		{
			return (GenericListResponse<PlayerRank>)_client.SendMethod<GenericListResponse<PlayerRank>>(TrackManiaMethod.GetCurrentRanking.ToString(), maxEntriestToReceive, startingIndex);
		}

		/// <summary>
		/// Returns the vote currently in progress. The returned structure is { CallerLogin, CmdName, CmdParam }.
		/// </summary>
		public GenericResponse<CallVote> GetCurrentCallVote()
		{
			return (GenericResponse<CallVote>)_client.SendMethod<GenericResponse<CallVote>>(TrackManiaMethod.GetCurrentCallVote.ToString());
		}

		/// <summary>
		/// Set new ratios for passing specific votes. The parameter is an array of structs {string Command, double Ratio}, ratio is in [0,1] or -1 for vote disabled.
		/// </summary>
		public GenericResponse<bool> SetCallVoteRatios(List<CallVoteRatio> ratios)
		{
			return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SetCallVoteRatios.ToString(), ratios);
		}

		/// <summary>
		/// Get the current ratios for passing votes.
		/// </summary>
		public GenericListResponse<CallVoteRatio> GetCallVoteRatios()
		{
			return (GenericListResponse<CallVoteRatio>)_client.SendMethod<GenericListResponse<CallVoteRatio>>(TrackManiaMethod.GetCallVoteRatios.ToString());
		}

		/// <summary>
		/// Get some system infos.
		/// </summary>
		public GenericResponse<SystemInfo> GetSystemInfo()
		{
			return (GenericResponse<SystemInfo>)_client.SendMethod<GenericResponse<SystemInfo>>(TrackManiaMethod.GetSystemInfo.ToString());
		}

		/// <summary>
		/// Set new server options using the struct passed as parameters. This struct must contain the following fields : Name, Comment, Password, PasswordForSpectator, RefereePassword, NextMaxPlayers, NextMaxSpectators, IsP2PUpload, IsP2PDownload, NextLadderMode, NextVehicleNetQuality, NextCallVoteTimeOut, CallVoteRatio, AllowChallengeDownload, AutoSaveReplays, and optionally RefereePassword, RefereeMode, AutoSaveValidationReplays, HideServer, UseChangingValidationSeed. Only available to Admin. A change of NextMaxPlayers, NextMaxSpectators, NextLadderMode, NextVehicleNetQuality, NextCallVoteTimeOut or UseChangingValidationSeed requires a challenge restart to be taken into account.
		/// </summary>
		public GenericResponse<bool> SetServerOptions(ServerOptions options)
		{
			return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SetServerOptions.ToString(), options);
		}

		/// <summary>
		/// Get the mods settings.
		/// </summary>
		public GenericResponse<ForcedMod> GetForcedMods()
		{
			return (GenericResponse<ForcedMod>)_client.SendMethod<GenericResponse<ForcedMod>>(TrackManiaMethod.GetForcedMods.ToString());
		}

		/// <summary>
		/// Set the mods to apply on the clients. Parameters: Override, if true even the challenges with a mod will be overridden by the server setting; and Mods, an array of structures [{EnvName, Url}, ...]. Requires a challenge restart to be taken into account. Only available to Admin.
		/// </summary>
		public GenericResponse<bool> SetForcedMods(bool setOverride, List<Mod> mods)
		{
			return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SetForcedMods.ToString(), setOverride, mods);
		}

		/// <summary>
		/// Get the music setting.
		/// </summary>
		public GenericResponse<ForcedMusic> GetForcedMusic()
		{
			return (GenericResponse<ForcedMusic>)_client.SendMethod<GenericResponse<ForcedMusic>>(TrackManiaMethod.GetForcedMusic.ToString());
		}

		/// <summary>
		///  Get the current forced skins.
		/// </summary>
		public GenericListResponse<ForcedSkin> GetForcedSkins()
		{
			return (GenericListResponse<ForcedSkin>)_client.SendMethod<GenericListResponse<ForcedSkin>>(TrackManiaMethod.GetForcedSkins.ToString());
		}

		/// <summary>
		/// efines a list of remappings for player skins. It expects a list of structs Orig, Name, Checksum, Url. Orig is the name of the skin to remap, or '*' for any other. Name, Checksum, Url define the skin to use. (they are optional, you may set value '' for any of those. All 3 null means same as Orig). Will only affects players connecting after the value is set. Only available to Admin.
		/// </summary>
		public GenericResponse<bool> SetForcedSkins(List<ForcedSkin> skins)
		{
			return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SetForcedSkins.ToString(), skins);
		}

		/// <summary>
		/// Get the current and next value of UseChangingValidationSeed. The struct returned contains two fields CurrentValue and NextValue.
		/// </summary>
		public GenericResponse<CNPair<bool>> GetUseChangingValidationSeed()
		{
			return (GenericResponse<CNPair<bool>>)_client.SendMethod<GenericResponse<CNPair<bool>>>(TrackManiaMethod.GetUseChangingValidationSeed.ToString());
		}

		/// <summary>
		/// Returns a struct containing the game settings for the next challenge, ie: GameMode, ChatTime, NbChallenge, RoundsPointsLimit, RoundsUseNewRules, RoundsForcedLaps, TimeAttackLimit, TimeAttackSynchStartPeriod, TeamPointsLimit, TeamMaxPoints, TeamUseNewRules, LapsNbLaps, LapsTimeLimit, FinishTimeout, and additionally for version 1: AllWarmUpDuration, DisableRespawn, ForceShowAllOpponents, RoundsPointsLimitNewRules, TeamPointsLimitNewRules, CupPointsLimit, CupRoundsPerChallenge, CupNbWinners, CupWarmUpDuration.
		/// </summary>
		public GenericResponse<GameInfo> GetNextGameInfo()
		{
			return (GenericResponse<GameInfo>)_client.SendMethod<GenericResponse<GameInfo>>(TrackManiaMethod.GetNextGameInfo.ToString());
		}

		/// <summary>
		/// Returns a struct containing two other structures, the first containing the current game settings and the second the game settings for next challenge. The first structure is named CurrentGameInfos and the second NextGameInfos.
		/// </summary>
		public GenericResponse<GameInfos> GetGameInfos()
		{
			return (GenericResponse<GameInfos>)_client.SendMethod<GenericResponse<GameInfos>>(TrackManiaMethod.GetGameInfos.ToString());
		}

		/// <summary>
		/// Set new game settings using the struct passed as parameters. This struct must contain the following fields : GameMode, ChatTime, RoundsPointsLimit, RoundsUseNewRules, RoundsForcedLaps, TimeAttackLimit, TimeAttackSynchStartPeriod, TeamPointsLimit, TeamMaxPoints, TeamUseNewRules, LapsNbLaps, LapsTimeLimit, FinishTimeout, and optionally: AllWarmUpDuration, DisableRespawn, ForceShowAllOpponents, RoundsPointsLimitNewRules, TeamPointsLimitNewRules, CupPointsLimit, CupRoundsPerChallenge, CupNbWinners, CupWarmUpDuration. Only available to Admin. Requires a challenge restart to be taken into account.
		/// </summary>
		public GenericResponse<bool> SetGameInfos(GameInfo gameInfo)
		{
			return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SetGameInfos.ToString(), gameInfo);
		}

		/// <summary>
		/// Gets the points used for the scores in round mode.
		/// </summary>
		public GenericListResponse<int> GetRoundCustomPoints()
		{
			return (GenericListResponse<int>)_client.SendMethod<GenericListResponse<int>>(TrackManiaMethod.GetRoundCustomPoints.ToString());
		}

		/// <summary>
		/// Set the points used for the scores in round mode. Points is an array of decreasing integers for the players from the first to last. And you can add an optional boolean to relax the constraint checking on the scores.
		/// </summary>
		public GenericResponse<bool> SetRoundCustomPoints(List<int> decreasingPoints, bool relaxConstraints)
		{
			return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.SetRoundCustomPoints.ToString(), decreasingPoints, relaxConstraints);
		}

		/// <summary>
		/// Force the scores of the current game. Only available in rounds and team mode. You have to pass an array of structs {int PlayerId, int Score}. And a boolean SilentMode - if true, the scores are silently updated (only available for SuperAdmin), allowing an external controller to do its custom counting...
		/// </summary>
		public GenericResponse<bool> ForceScores(List<ForcedScore> scores, bool silentMode)
		{
			return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.ForceScores.ToString(), scores, silentMode);
		}

		/// <summary>
		/// Returns a struct containing the player infos of the game server (ie: in case of a basic server, itself; in case of a relay server, the main server), with an optional parameter for compatibility: struct version (0 = united, 1 = forever). The structure is identical to the ones from GetPlayerList. Forever PlayerInfo struct is: Login, NickName, PlayerId, TeamId, SpectatorStatus, LadderRanking, and Flags.
		/// LadderRanking is 0 when not in official mode,
		/// Flags = ForceSpectator(0,1,2) + IsReferee * 10 + IsPodiumReady * 100 + IsUsingStereoscopy * 1000 + IsManagedByAnOtherServer * 10000 + IsServer * 100000
		/// SpectatorStatus = Spectator + TemporarySpectator * 10 + PureSpectator * 100 + AutoTarget * 1000 + CurrentTargetId * 10000
		/// </summary>
		public GenericResponse<PlayerInfo> GetMainServerPlayerInfo()
		{
			return (GenericResponse<PlayerInfo>)_client.SendMethod<GenericResponse<PlayerInfo>>(TrackManiaMethod.GetMainServerPlayerInfo.ToString());
		}

		/// <summary>
		/// Send a localised text message to all clients without the server login. The parameter is an array of structures {Lang='??', Text='...'}. If no matching language is found, the last text in the array is used. Only available to Admin.
		/// </summary>
		public GenericResponse<bool> ChatSendServerMessageToLanguage(List<LanguageDependentText> texts)
		{
			return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.ChatSendServerMessageToLanguage.ToString(), texts);
		}

		/// <summary>
		/// Send a localised text message to all clients. The parameter is an array of structures {Lang='??', Text='...'}. If no matching language is found, the last text in the array is used.
		/// </summary>
		public GenericResponse<bool> ChatSendToLanguage(List<LanguageDependentText> texts)
		{
			return (GenericResponse<bool>)_client.SendMethod<GenericResponse<bool>>(TrackManiaMethod.ChatSendToLanguage.ToString(), texts);
		}

        #endregion
    }
}