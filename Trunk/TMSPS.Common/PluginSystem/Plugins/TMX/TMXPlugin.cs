using System;
using System.Collections.Generic;
using TMSPS.Core.Common;
using TMSPS.Core.Communication.EventArguments.Callbacks;
using TMSPS.Core.Communication.ProxyTypes;
using TMSPS.Core.Communication.ResponseHandling;
using Version=System.Version;

namespace TMSPS.Core.PluginSystem.Plugins.TMX
{
    public class TMXPlugin : TMSPSPlugin
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

        #region Constructor

        protected TMXPlugin(string pluginDirectory) : base(pluginDirectory)
        {
            
        }

	    #endregion

        #region Methods

        protected override void Init()
        {
            Context.RPCClient.Callbacks.PlayerChat += Callbacks_PlayerChat;
        }

        protected override void Dispose(bool connectionLost)
        {
            Context.RPCClient.Callbacks.PlayerChat -= Callbacks_PlayerChat;
        }

        private void Callbacks_PlayerChat(object sender, PlayerChatEventArgs e)
        {
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

            if (command.Is(Command.TMXInfo))
            {
                if (command.PartsWithoutMainCommand.Count > 0)
                {
                    string trackID = command.PartsWithoutMainCommand[0];
                    TMXInfo tmxInfo = TMXInfo.Retrieve(trackID);

                    if (tmxInfo != null && !tmxInfo.Erroneous)
                        SendFormattedMessageToLogin(e.Login, "{[#ServerStyle]}> {[#MessageStyle]}[TMX-Info] Name: {[#HighlightStyle]}{[Name]}{[#MessageStyle]}, Author: {[#HighlightStyle]}{[Author]}{[#MessageStyle]}, Environment: {[#HighlightStyle]}{[Env]}", "Name", tmxInfo.Name, "Author", tmxInfo.Author, "Env", tmxInfo.Environment);
                    else
                        SendFormattedMessageToLogin(e.Login, "{[#ServerStyle]}> {[#ErrorStyle]}Could not retrieve trackinfo for trackid " + trackID);
                }

                return true;
            }

            return false;
        }

        private bool CheckForTMXAddTrackCommand(PlayerChatEventArgs e)
        {
            ServerCommand command = ServerCommand.Parse(e.Text);

            if (!command.IsAny(Command.AddTrack, Command.InsertTrack))
                return false;

            if (!LoginHasRight(e.Login, true, Command.AddTrack))
                return true;

            if (command.PartsWithoutMainCommand.Count == 0)
                return true;

            string trackID = command.PartsWithoutMainCommand[0];
            TMXInfo tmxInfo = TMXInfo.Retrieve(trackID);
            
            if (tmxInfo == null || tmxInfo.Erroneous)
            {
                SendFormattedMessageToLogin(e.Login, "{[#ServerStyle]}> {[#ErrorStyle]}Could not retrieve trackinfo for trackid " + trackID);
                return true;
            }

            List<ChallengeListSingleInfo> challenges = GetChallengeList();
            
            if (challenges == null)
            {
                SendFormattedMessageToLogin(e.Login, "{[#ServerStyle]}> {[#ErrorStyle]}Could not retrieve current challenge list.");
                return true;
            }

            if (challenges.Exists(c => c.FileName.Equals(tmxInfo.GetRelativeFilePath(), StringComparison.InvariantCultureIgnoreCase)))
            {
                SendFormattedMessageToLogin(e.Login, "{[#ServerStyle]}> {[#MessageStyle]}Track is already in tracklist.");
                return true;
            }

            byte[] trackData = TMXInfo.DownloadTrack(trackID);

            if (trackData == null)
            {
                SendFormattedMessageToLogin(e.Login, "{[#ServerStyle]}> {[#ErrorStyle]}Could not retrieve track {[#HighlightStyle]}{[Trackname]}{[#ErrorStyle]} with trackid {[#HighlightStyle]}{[TrackID]}.", "Trackname", tmxInfo.Name, "TrackID", trackID);
                return true;
            }

            string targetTrackFilePath = tmxInfo.GetRelativeFilePath();

            GenericResponse<bool> writeFileResponse = Context.RPCClient.Methods.WriteFile(targetTrackFilePath, trackData);

            if (writeFileResponse.Erroneous || !writeFileResponse.Value)
            {
                SendFormattedMessageToLogin(e.Login, "{[#ServerStyle]}>{[#ErrorStyle] Could write track {[#HighlightStyle]}{[Trackname]}{[#ErrorStyle]} with trackid {[#HighlightStyle]}{[TrackID]}.", "Trackname", tmxInfo.Name, "TrackID", trackID);
                return true;
            }

            GenericResponse<bool> addtrackResponse = command.Is(Command.AddTrack) ? Context.RPCClient.Methods.AddChallenge(targetTrackFilePath) 
                                                                                  : Context.RPCClient.Methods.InsertChallenge(targetTrackFilePath);

            if (addtrackResponse.Erroneous || !addtrackResponse.Value)
            {
                SendFormattedMessageToLogin(e.Login, "{[#ServerStyle]}>{[#ErrorStyle] Could add track {[#HighlightStyle]}{[Trackname]}{[#ErrorStyle]} with trackid {[#HighlightStyle]}{[TrackID]}.", "Trackname", tmxInfo.Name, "TrackID", trackID);
                return true;
            }

            SendFormattedMessageToLogin(e.Login, "{[#ServerStyle]}> {[#MessageStyle]}Track {[#HighlightStyle]}{[Trackname]}{[#MessageStyle]} with trackid {[#HighlightStyle]}{[TrackID]}{[#MessageStyle]} added to tracklist.", "Trackname", tmxInfo.Name, "TrackID", trackID);

            if (command.Is(Command.InsertTrack))
                SendFormattedMessageToLogin(e.Login, "{[#ServerStyle]}> {[#MessageStyle]}Track {[#HighlightStyle]}{[Trackname]}{[#MessageStyle]} with trackid {[#HighlightStyle]}{[TrackID]}{[#MessageStyle]} will be the next track.", "Trackname", tmxInfo.Name, "TrackID", trackID);

            return true;
        }

        #endregion
    }
}
