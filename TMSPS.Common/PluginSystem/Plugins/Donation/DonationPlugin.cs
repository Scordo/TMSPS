using System;
using System.Collections.Generic;
using System.Globalization;
using TMSPS.Core.Common;
using TMSPS.Core.Communication.ProxyTypes;
using TMSPS.Core.Communication.ResponseHandling;
using TMSPS.Core.PluginSystem.Configuration;
using BillState=TMSPS.Core.Communication.EventArguments.Callbacks.BillState;
using Version=System.Version;

namespace TMSPS.Core.PluginSystem.Plugins.Donation
{
    public class DonationPlugin : TMSPSPlugin
    {
        #region Properties

        public override Version Version
        {
            get { return new Version("1.0.0.0"); }
        }

        public override string Author
        {
            get { return "Jens Hofmann"; }
        }

        public override string Name
        {
            get { return "DonationPlugin"; }
        }

        public override string Description
        {
            get { return "Kicks players and/or spectators idling too long."; }
        }

        public override string ShortName
        {
            get { return "Donation"; }
        }

        private DonationPluginSettings Settings { get; set; }
        private bool InitializationAborted { get; set; }
        private Dictionary<int, DonationInfo> BillDictionary { get; set; }

        #endregion

        protected override void Init()
        {
            Settings = DonationPluginSettings.ReadFromFile(PluginSettingsFilePath);
            InitializationAborted = true;

            if (Settings.IsServerTargetLogin && Context.ServerInfo.Version.Name != "TmForever")
            {
                Logger.WarnToUI("Plugin not started, because the server is not a forever-server");
                return;
            }

            if (!Context.ServerInfo.PlayerInfo.IsUnitedAccount)
            {
                Logger.WarnToUI("Plugin not started, because the server is not running with an united account.");
                return;
            }

            InitializationAborted = false;
            BillDictionary = new Dictionary<int, DonationInfo>();
            Context.RPCClient.Callbacks.PlayerChat += Callbacks_PlayerChat;
            Context.RPCClient.Callbacks.BillUpdated += Callbacks_BillUpdated;
        }

        protected override void Dispose(bool connectionLost)
        {
            if (InitializationAborted)
                return;

            Context.RPCClient.Callbacks.PlayerChat -= Callbacks_PlayerChat;
            Context.RPCClient.Callbacks.BillUpdated -= Callbacks_BillUpdated;
        }

        private void Callbacks_PlayerChat(object sender, Communication.EventArguments.Callbacks.PlayerChatEventArgs e)
        {
            if (e.Text == null || !e.Text.StartsWith(Command.DONATE, StringComparison.OrdinalIgnoreCase))
                return;

            int coppers;

            if (!int.TryParse(e.Text.Substring(Command.DONATE.Length).Trim(), NumberStyles.None, CultureInfo.InvariantCulture,  out coppers) || coppers <= 0)
                return;

            if (coppers < Settings.MinDonationValue)
            {
                SendFormattedMessageToLogin(e.Login, Settings.DonationToSmallMessage, "Coppers", Settings.MinDonationValue.ToString(CultureInfo.InvariantCulture));
                return;
            }

            PlayerSettings playerSettings = GetPlayerSettings(e.Login);

            bool isUnitedAccount = playerSettings.IsUnitedAccount;

            if (!playerSettings.DetailMode.HasDetailedPlayerInfo())
            {
                DetailedPlayerInfo playerInfo = GetDetailedPlayerInfo(e.Login);

                if (playerInfo != null)
                    isUnitedAccount = playerInfo.IsUnitedAccount;
            }

            if (!isUnitedAccount)
            {
                SendFormattedMessageToLogin(e.Login, Settings.PlayerHasNoUnitedAccountMessage);
                return;
            }

            GenericResponse<int> billResponse = Context.RPCClient.Methods.SendBill(e.Login, coppers, Settings.DonationHint, Settings.DonationTargetLogin);

            if (billResponse.Erroneous)
            {
                Logger.Warn(string.Format("Error while calling method SendBill: {0}({1})", billResponse.Fault.FaultMessage, billResponse.Fault.FaultCode));
                SendFormattedMessageToLogin(e.Login, Settings.DonationErrorMessage, "ErrorMessage", billResponse.Fault.FaultMessage);
                return;
            }
            
            BillDictionary[billResponse.Value] = new DonationInfo{Login = e.Login, Coppers = coppers};
        }

        private void Callbacks_BillUpdated(object sender, Communication.EventArguments.Callbacks.BillUpdatedEventArgs e)
        {
            DonationInfo donationInfo = BillDictionary.ContainsKey(e.BillID) ? BillDictionary[e.BillID] : null;

            switch (e.State)
            {
                case BillState.Payed:
                    Logger.Error(string.Format("Donation successfull: {0} (TransactionID: {1}, BillId: {2})", e.StateName, e.TransactionID, e.BillID));
                    if (!CheckDonationInfo(donationInfo, e.BillID))
                        break;

                    BillDictionary.Remove(e.BillID);
                    SendFormattedMessageToLogin(donationInfo.Login, Settings.DonationThanksMessage, "Coppers", donationInfo.Coppers.ToString(CultureInfo.InvariantCulture));
                    break;
                case BillState.Refused:
                    Logger.Error(string.Format("Donation Refused: {0} (TransactionID: {1}, BillId: {2})", e.StateName, e.TransactionID, e.BillID));

                    if (!CheckDonationInfo(donationInfo, e.BillID))
                        break;

                    BillDictionary.Remove(e.BillID);
                    SendFormattedMessageToLogin(donationInfo.Login, Settings.RefuseMessage);
                    break;
                case BillState.Erroneous:
                    Logger.Error(string.Format("Donation erroneous: {0} (TransactionID: {1}, BillId: {2})", e.StateName, e.TransactionID, e.BillID));
                    if (!CheckDonationInfo(donationInfo, e.BillID))
                        break;

                    BillDictionary.Remove(e.BillID);
                    SendFormattedMessageToLogin(donationInfo.Login, Settings.DonationErrorMessage, "ErrorMessage", e.StateName);
                    break;
                default:
                    // skip Issued/CreatingTransaction/ValidatingPayement
                    break;
            }
        }

        private bool CheckDonationInfo(DonationInfo donationInfo, int billID)
        {
            if (donationInfo == null)
                Logger.WarnToUI("Could not find DonationInfo for BillID: " + billID);

            return donationInfo != null;
        }

        private class DonationInfo
        {
            public string Login { get; set;}
            public int Coppers { get; set;}
        }
    }
}