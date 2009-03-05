using System;
using System.Collections.Generic;
using System.IO;
using TMSPS.Core.Communication.EventArguments.Callbacks;
using TMSPS.Core.Communication.ProxyTypes;
using Version=System.Version;

namespace TMSPS.Core.PluginSystem.Plugins.TMX
{
    public class TMXPlugin : TMSPSPlugin
    {
        #region Constants

        public const string TMX_INFO_COMMAND = "tmxinfo";
        public const string TMX_ADD_TRACK_COMMAND = "addtrack";

        #endregion

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
            get { return "TMXPlugin"; }
        }

        public override string Description
        {
            get { return "Allows TMX challenge download and info retrieval."; }
        }

        public override string ShortName
        {
            get { return "TMX"; }
        }

        #endregion

        #region Methods

        protected override void Init()
        {

            Context.RPCClient.Callbacks.PlayerChat += Callbacks_PlayerChat;
        }

        private void Callbacks_PlayerChat(object sender, PlayerChatEventArgs e)
        {
            if (e.Erroneous)
            {
                Logger.Error(string.Format("[Callbacks_PlayerChat] Invalid Response: {0}[{1}]", e.Fault.FaultMessage, e.Fault.FaultCode));
                return;
            }

            RunCatchLog(() =>
            {
                if (e.IsServerMessage || e.Text.IsNullOrTimmedEmpty())
                    return;

                if (CheckForTMXInfoCommand(e))
                    return;

                if (CheckForTMXAddTrackCommand(e))
                    return;

            }, "Error in Callbacks_PlayerChat Method.", true);
        }

        private bool CheckForTMXInfoCommand(PlayerChatEventArgs e)
        {
            ServerCommand command = ServerCommand.Parse(e.Text);

            if (command.IsMainCommandAnyOf(TMX_INFO_COMMAND))
            {
                if (command.PartsWithoutMainCommand.Count > 0)
                {
                    string trackID = command.PartsWithoutMainCommand[0];
                    TMXInfo tmxInfo = TMXInfo.Retrieve(trackID);

                    if (tmxInfo != null && !tmxInfo.Erroneous)
                        Context.RPCClient.Methods.ChatSendToLogin(string.Format("[TMX-Info] Name: {0}, Author: {1}, Environment: {2}", tmxInfo.Name, tmxInfo.Author, tmxInfo.Environment), e.Login);
                    else
                        Context.RPCClient.Methods.ChatSendToLogin("Could not retrieve trackinfo for trackid " + trackID, e.Login);
                }

                return true;
            }

            return false;
        }

        private void WriteTMXTrack(byte[] content, string trackID)
        {
            string targetFilePath = TMXInfo.GetTMXFilePath(Context.ServerInfo.TrackDirectory, trackID);
            string targetDirectory = Path.GetDirectoryName(targetFilePath);

            if (!Directory.Exists(targetDirectory))
                Directory.CreateDirectory(targetDirectory);

            File.WriteAllBytes(targetFilePath, content);
        }

        private bool CheckForTMXAddTrackCommand(PlayerChatEventArgs e)
        {
            ServerCommand command = ServerCommand.Parse(e.Text);

            if (!command.IsMainCommandAnyOf(TMX_ADD_TRACK_COMMAND))
                return false;

            if (!Context.Credentials.UserHasRight(e.Login, TMX_ADD_TRACK_COMMAND))
            {
                Context.RPCClient.Methods.ChatSendToLogin("You do not have permissions to execute this command!", e.Login);
                return true;
            }

            if (command.PartsWithoutMainCommand.Count == 0)
                return true;

            string trackID = command.PartsWithoutMainCommand[0];
            TMXInfo tmxInfo = TMXInfo.Retrieve(trackID);
            
            if (tmxInfo == null || tmxInfo.Erroneous)
            {
                Context.RPCClient.Methods.ChatSendToLogin("Could not retrieve trackinfo for trackid " + trackID, e.Login);
                return true;
            }

            List<ChallengeListSingleInfo> challenges = GetChallengeList();
            
            if (challenges == null)
            {
                Context.RPCClient.Methods.ChatSendToLogin("Could not retrieve current challenge list.", e.Login);
                return true;
            }

            if (challenges.Exists(c => c.FileName.Equals(tmxInfo.GetRelativeFilePath(), StringComparison.InvariantCultureIgnoreCase)))
            {
                Context.RPCClient.Methods.ChatSendToLogin("Track is already in tracklist.", e.Login);
                return true;
            }

            byte[] trackData = TMXInfo.DownloadTrack(trackID);

            if (trackData != null)
            {
                string targetTrackFilePath = tmxInfo.GetTMXFilePath(Context.ServerInfo.TrackDirectory);


                WriteTMXTrack(trackData, trackID);
                Context.RPCClient.Methods.AddChallenge(targetTrackFilePath);
                Context.RPCClient.Methods.ChatSendToLogin(string.Format("Track '{0}' with trackid {1} added to tracklist.", tmxInfo.Name, trackID), e.Login);


                challenges = GetChallengeList();
                if (challenges != null)
                {
                    int challengeIndex = challenges.FindIndex(c => c.FileName.Equals(tmxInfo.GetRelativeFilePath(), StringComparison.InvariantCultureIgnoreCase));

                    if (challengeIndex != -1)
                    {
                        Context.RPCClient.Methods.SetNextChallengeIndex(challengeIndex);
                        Context.RPCClient.Methods.ChatSendToLogin(string.Format("Track '{0}' with trackid {1} will be the next track.", tmxInfo.Name, trackID), e.Login);
                    }
                }
            }
            else
            {
                Context.RPCClient.Methods.ChatSendToLogin(string.Format("Could not retrieve track '{0}' for trackid {1}.", tmxInfo.Name, trackID), e.Login);
            }

            return true;
        }

        protected override void Dispose(bool connectionLost)
        {
            Context.RPCClient.Callbacks.PlayerChat -= Callbacks_PlayerChat;
        }

      

        #endregion
    }
}
