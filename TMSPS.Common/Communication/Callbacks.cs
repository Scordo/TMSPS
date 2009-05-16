using System;
using System.Threading;
using System.Xml.Linq;
using TMSPS.Core.Communication.EventArguments;
using TMSPS.Core.Communication.EventArguments.Callbacks;

namespace TMSPS.Core.Communication
{
    public class Callbacks
    {
        private const string TRACKMANIA_METHOD_PREFIX = "TrackMania.";

        #region Events

        public event EventHandler<PlayerConnectEventArgs> PlayerConnect;
        public event EventHandler<PlayerDisconnectEventArgs> PlayerDisconnect;
        public event EventHandler<PlayerChatEventArgs> PlayerChat;
        public event EventHandler<BeginRaceEventArgs> BeginRace;
        public event EventHandler<EndRaceEventArgs> EndRace;
        public event EventHandler<PlayerManialinkPageAnswerEventArgs> PlayerManialinkPageAnswer;
        public event EventHandler<EchoEventArgs> Echo;
        public event EventHandler ServerStart;
        public event EventHandler ServerStop;
        public event EventHandler BeginRound;
        public event EventHandler EndRound;
        public event EventHandler<StatusChangedEventArgs> StatusChanged;
        public event EventHandler<PlayerCheckpointEventArgs> PlayerCheckpoint;
        public event EventHandler<PlayerFinishEventArgs> PlayerFinish;
        public event EventHandler<PlayerIncoherenceEventArgs> PlayerIncoherence;
        public event EventHandler<BillUpdatedEventArgs> BillUpdated;
        public event EventHandler<TunnelDataReceivedEventArgs> TunnelDataReceived;
        public event EventHandler<ChallengeListModifiedeventArgs> ChallengeListModified;
        public event EventHandler<PlayerInfoChangedEventArgs> PlayerInfoChanged;
        public event EventHandler<ManualFlowControlTransitionEventArgs> ManualFlowControlTransition;
        public event EventHandler<BeginChallengeEventArgs> BeginChallenge;
        public event EventHandler<EndChallengeEventArgs> EndChallenge;

        #endregion

        #region Public Methods

        public bool CheckForKnownMethodCallback(XElement messageElement)
        {
            TrackManiaCallback? callback = TryParseCallback(messageElement);

            if (!callback.HasValue)
                return false;

            bool result = true;
            switch (callback)
            {
                case TrackManiaCallback.PlayerConnect:
            		ThreadPool.QueueUserWorkItem(OnPlayerConnect, messageElement);
                    break;
                case TrackManiaCallback.PlayerDisconnect:
            		ThreadPool.QueueUserWorkItem(OnPlayerDisconnect, messageElement);
                    break;
                case TrackManiaCallback.PlayerChat:
					ThreadPool.QueueUserWorkItem(OnPlayerChat, messageElement);
                    break;
                case TrackManiaCallback.BeginRace:
					ThreadPool.QueueUserWorkItem(OnBeginRace, messageElement);
                    break;
                case TrackManiaCallback.EndRace:
					ThreadPool.QueueUserWorkItem(OnEndRace, messageElement);
                    break;
                case TrackManiaCallback.PlayerManialinkPageAnswer:
					ThreadPool.QueueUserWorkItem(OnPlayerManialinkPageAnswer, messageElement);
                    break;
                case TrackManiaCallback.Echo:
					ThreadPool.QueueUserWorkItem(OnEcho, messageElement);
                    break;
                case TrackManiaCallback.ServerStart:
					ThreadPool.QueueUserWorkItem(OnServerStart, messageElement);
                    break;
                case TrackManiaCallback.ServerStop:
					ThreadPool.QueueUserWorkItem(OnServerStop, messageElement);
                    break;
                case TrackManiaCallback.BeginRound:
					ThreadPool.QueueUserWorkItem(OnBeginRound, messageElement);
                    break;
                case TrackManiaCallback.EndRound:
					ThreadPool.QueueUserWorkItem(OnEndRound, messageElement);
                    break;
                case TrackManiaCallback.StatusChanged:
					ThreadPool.QueueUserWorkItem(OnStatusChanged, messageElement);
                    break;
                case TrackManiaCallback.PlayerCheckpoint:
					ThreadPool.QueueUserWorkItem(OnPlayerCheckpoint, messageElement);
                    break;
                case TrackManiaCallback.PlayerFinish:
					ThreadPool.QueueUserWorkItem(OnPlayerFinish, messageElement);
                    break;
                case TrackManiaCallback.PlayerIncoherence:
					ThreadPool.QueueUserWorkItem(OnPlayerIncoherence, messageElement);
                    break;
                case TrackManiaCallback.BillUpdated:
					ThreadPool.QueueUserWorkItem(OnBillUpdated, messageElement);
                    break;
                case TrackManiaCallback.TunnelDataReceived:
					ThreadPool.QueueUserWorkItem(OnTunnelDataReceived, messageElement);
                    break;
                case TrackManiaCallback.ChallengeListModified:
					ThreadPool.QueueUserWorkItem(OnChallengeListModified, messageElement);
                    break;
                case TrackManiaCallback.PlayerInfoChanged:
					ThreadPool.QueueUserWorkItem(OnPlayerInfoChanged, messageElement);
                    break;
                case TrackManiaCallback.ManualFlowControlTransition:
					ThreadPool.QueueUserWorkItem(OnManualFlowControlTransition, messageElement);
                    break;
                case TrackManiaCallback.BeginChallenge:
					ThreadPool.QueueUserWorkItem(OnBeginChallenge, messageElement);
                    break;
                case TrackManiaCallback.EndChallenge:
					ThreadPool.QueueUserWorkItem(OnEndChallenge, messageElement);
                    break;
                default:
                    result = false;
                    break;
            }

            return result;
        }

        #endregion

        #region Non Public Methods

        protected void OnPlayerConnect(object messageElement)
        {
            if (PlayerConnect != null)
                PlayerConnect(this, EventArgsBase<PlayerConnectEventArgs>.Parse((XElement) messageElement));
        }

        protected void OnPlayerDisconnect(object messageElement)
        {
            if (PlayerDisconnect != null)
                PlayerDisconnect(this, EventArgsBase<PlayerDisconnectEventArgs>.Parse((XElement) messageElement));
        }

        protected void OnPlayerChat(object messageElement)
        {
            if (PlayerChat != null)
                PlayerChat(this, EventArgsBase<PlayerChatEventArgs>.Parse((XElement) messageElement));
        }

        protected void OnBeginRace(object messageElement)
        {
            if (BeginRace != null)
                BeginRace(this, EventArgsBase<BeginRaceEventArgs>.Parse((XElement) messageElement));
        }

        protected void OnEndRace(object messageElement)
        {
            if (EndRace != null)
                EndRace(this, EventArgsBase<EndRaceEventArgs>.Parse((XElement) messageElement));
        }

        protected void OnPlayerManialinkPageAnswer(object messageElement)
        {
            if (PlayerManialinkPageAnswer != null)
                PlayerManialinkPageAnswer(this, EventArgsBase<PlayerManialinkPageAnswerEventArgs>.Parse((XElement) messageElement));
        }

        protected void OnEcho(object messageElement)
        {
            if (Echo != null)
                Echo(this, EventArgsBase<EchoEventArgs>.Parse((XElement) messageElement));
        }

		protected void OnServerStart(object messageElement)
        {
            if (ServerStart != null)
                ServerStart(this, EventArgs.Empty);
        }

		protected void OnServerStop(object messageElement)
        {
            if (ServerStop != null)
                ServerStop(this, EventArgs.Empty);
        }

		protected void OnBeginRound(object messageElement)
        {
            if (BeginRound != null)
                BeginRound(this, EventArgs.Empty);
        }

		protected void OnEndRound(object messageElement)
        {
            if (EndRound != null)
                EndRound(this, EventArgs.Empty);
        }

        protected void OnStatusChanged(object messageElement)
        {
            if (StatusChanged != null)
                StatusChanged(this, EventArgsBase<StatusChangedEventArgs>.Parse((XElement) messageElement));
        }

        protected void OnPlayerCheckpoint(object messageElement)
        {
            if (PlayerCheckpoint != null)
                PlayerCheckpoint(this, EventArgsBase<PlayerCheckpointEventArgs>.Parse((XElement) messageElement));
        }

        protected void OnPlayerFinish(object messageElement)
        {
            if (PlayerFinish != null)
                PlayerFinish(this, EventArgsBase<PlayerFinishEventArgs>.Parse((XElement) messageElement));
        }

        protected void OnPlayerIncoherence(object messageElement)
        {
            if (PlayerIncoherence != null)
                PlayerIncoherence(this, EventArgsBase<PlayerIncoherenceEventArgs>.Parse((XElement) messageElement));
        }

        protected void OnBillUpdated(object messageElement)
        {
            if (BillUpdated != null)
                BillUpdated(this, EventArgsBase<BillUpdatedEventArgs>.Parse((XElement) messageElement));
        }

        protected void OnTunnelDataReceived(object messageElement)
        {
            if (TunnelDataReceived != null)
                TunnelDataReceived(this, EventArgsBase<TunnelDataReceivedEventArgs>.Parse((XElement) messageElement));
        }

        protected void OnChallengeListModified(object messageElement)
        {
            if (ChallengeListModified != null)
                ChallengeListModified(this, EventArgsBase<ChallengeListModifiedeventArgs>.Parse((XElement) messageElement));
        }

        protected void OnPlayerInfoChanged(object messageElement)
        {
            if (PlayerInfoChanged != null)
                PlayerInfoChanged(this, EventArgsBase<PlayerInfoChangedEventArgs>.Parse((XElement) messageElement));
        }

        protected void OnManualFlowControlTransition(object messageElement)
        {
            if (ManualFlowControlTransition != null)
                ManualFlowControlTransition(this, EventArgsBase<ManualFlowControlTransitionEventArgs>.Parse((XElement) messageElement));
        }

        protected void OnBeginChallenge(object messageElement)
        {
            if (BeginChallenge != null)
                BeginChallenge(this, EventArgsBase<BeginChallengeEventArgs>.Parse((XElement) messageElement));
        }

        protected void OnEndChallenge(object messageElement)
        {
            if (EndChallenge != null)
                EndChallenge(this, EventArgsBase<EndChallengeEventArgs>.Parse((XElement) messageElement));
        }

        private static TrackManiaCallback? TryParseCallback(XContainer messageElement)
        {
            XElement methodNameElement = messageElement.Element("methodName");

            if (methodNameElement == null)
                return null;

            return TryParseCallback(methodNameElement.Value.Trim());
        }

        private static TrackManiaCallback? TryParseCallback(string methodName)
        {
            if (methodName == null)
                return null;

            methodName = methodName.Trim();

            if (!methodName.StartsWith(TRACKMANIA_METHOD_PREFIX, StringComparison.OrdinalIgnoreCase))
                return null;

            methodName = methodName.Substring(TRACKMANIA_METHOD_PREFIX.Length);

            TrackManiaCallback? result = null;

            try
            {
                result = (TrackManiaCallback)Enum.Parse(typeof(TrackManiaCallback), methodName, true);
            }
            catch (ArgumentException)
            {

            }

            return result;
        }

        #endregion
    }
}