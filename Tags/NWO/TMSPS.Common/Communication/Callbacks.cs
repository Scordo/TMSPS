using System;
using System.Windows.Forms;
using System.Xml.Linq;
using TMSPS.Core.Communication.EventArguments;
using TMSPS.Core.Communication.EventArguments.Callbacks;

namespace TMSPS.Core.Communication
{
    public class Callbacks
    {
        private delegate void RaiseEventHandler(XElement messageElement);
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
                    new RaiseEventHandler(OnPlayerConnect).BeginInvoke(messageElement, null, null);
                    break;
                case TrackManiaCallback.PlayerDisconnect:
                    new RaiseEventHandler(OnPlayerDisconnect).BeginInvoke(messageElement, null, null);
                    break;
                case TrackManiaCallback.PlayerChat:
                    new RaiseEventHandler(OnPlayerChat).BeginInvoke(messageElement, null, null);
                    break;
                case TrackManiaCallback.BeginRace:
                    new RaiseEventHandler(OnBeginRace).BeginInvoke(messageElement, null, null);
                    break;
                case TrackManiaCallback.EndRace:
                    new RaiseEventHandler(OnEndRace).BeginInvoke(messageElement, null, null);
                    break;
                case TrackManiaCallback.PlayerManialinkPageAnswer:
                    new RaiseEventHandler(OnPlayerManialinkPageAnswer).BeginInvoke(messageElement, null, null);
                    break;
                case TrackManiaCallback.Echo:
                    new RaiseEventHandler(OnEcho).BeginInvoke(messageElement, null, null);
                    break;
                case TrackManiaCallback.ServerStart:
                    new MethodInvoker(OnServerStart).BeginInvoke(null, null);
                    break;
                case TrackManiaCallback.ServerStop:
                    new MethodInvoker(OnServerStart).BeginInvoke(null, null);
                    break;
                case TrackManiaCallback.BeginRound:
                    new MethodInvoker(OnServerStart).BeginInvoke(null, null);
                    break;
                case TrackManiaCallback.EndRound:
                    new MethodInvoker(OnServerStart).BeginInvoke(null, null);
                    break;
                case TrackManiaCallback.StatusChanged:
                    new RaiseEventHandler(OnStatusChanged).BeginInvoke(messageElement, null, null);
                    break;
                case TrackManiaCallback.PlayerCheckpoint:
                    new RaiseEventHandler(OnPlayerCheckpoint).BeginInvoke(messageElement, null, null);
                    break;
                case TrackManiaCallback.PlayerFinish:
                    new RaiseEventHandler(OnPlayerFinish).BeginInvoke(messageElement, null, null);
                    break;
                case TrackManiaCallback.PlayerIncoherence:
                    new RaiseEventHandler(OnPlayerIncoherence).BeginInvoke(messageElement, null, null);
                    break;
                case TrackManiaCallback.BillUpdated:
                    new RaiseEventHandler(OnBillUpdated).BeginInvoke(messageElement, null, null);
                    break;
                case TrackManiaCallback.TunnelDataReceived:
                    new RaiseEventHandler(OnTunnelDataReceived).BeginInvoke(messageElement, null, null);
                    break;
                case TrackManiaCallback.ChallengeListModified:
                    new RaiseEventHandler(OnChallengeListModified).BeginInvoke(messageElement, null, null);
                    break;
                case TrackManiaCallback.PlayerInfoChanged:
                    new RaiseEventHandler(OnPlayerInfoChanged).BeginInvoke(messageElement, null, null);
                    break;
                case TrackManiaCallback.ManualFlowControlTransition:
                    new RaiseEventHandler(OnManualFlowControlTransition).BeginInvoke(messageElement, null, null);
                    break;
                default:
                    result = false;
                    break;
            }

            return result;
        }

        #endregion

        #region Non Public Methods

        protected void OnPlayerConnect(XElement messageElement)
        {
            if (PlayerConnect != null)
                PlayerConnect(this, EventArgsBase<PlayerConnectEventArgs>.Parse(messageElement));
        }

        protected void OnPlayerDisconnect(XElement messageElement)
        {
            if (PlayerDisconnect != null)
                PlayerDisconnect(this, EventArgsBase<PlayerDisconnectEventArgs>.Parse(messageElement));
        }

        protected void OnPlayerChat(XElement messageElement)
        {
            if (PlayerChat != null)
                PlayerChat(this, EventArgsBase<PlayerChatEventArgs>.Parse(messageElement));
        }

        protected void OnBeginRace(XElement messageElement)
        {
            if (BeginRace != null)
                BeginRace(this, EventArgsBase<BeginRaceEventArgs>.Parse(messageElement));
        }

        protected void OnEndRace(XElement messageElement)
        {
            if (EndRace != null)
                EndRace(this, EventArgsBase<EndRaceEventArgs>.Parse(messageElement));
        }

        protected void OnPlayerManialinkPageAnswer(XElement messageElement)
        {
            if (PlayerManialinkPageAnswer != null)
                PlayerManialinkPageAnswer(this, EventArgsBase<PlayerManialinkPageAnswerEventArgs>.Parse(messageElement));
        }

        protected void OnEcho(XElement messageElement)
        {
            if (Echo != null)
                Echo(this, EventArgsBase<EchoEventArgs>.Parse(messageElement));
        }

        protected void OnServerStart()
        {
            if (ServerStart != null)
                ServerStart(this, EventArgs.Empty);
        }

        protected void OnServerStop()
        {
            if (ServerStop != null)
                ServerStop(this, EventArgs.Empty);
        }

        protected void OnBeginRound()
        {
            if (BeginRound != null)
                BeginRound(this, EventArgs.Empty);
        }

        protected void OnEndRound()
        {
            if (EndRound != null)
                EndRound(this, EventArgs.Empty);
        }

        protected void OnStatusChanged(XElement messageElement)
        {
            if (StatusChanged != null)
                StatusChanged(this, EventArgsBase<StatusChangedEventArgs>.Parse(messageElement));
        }

        protected void OnPlayerCheckpoint(XElement messageElement)
        {
            if (PlayerCheckpoint != null)
                PlayerCheckpoint(this, EventArgsBase<PlayerCheckpointEventArgs>.Parse(messageElement));
        }

        protected void OnPlayerFinish(XElement messageElement)
        {
            if (PlayerFinish != null)
                PlayerFinish(this, EventArgsBase<PlayerFinishEventArgs>.Parse(messageElement));
        }

        protected void OnPlayerIncoherence(XElement messageElement)
        {
            if (PlayerIncoherence != null)
                PlayerIncoherence(this, EventArgsBase<PlayerIncoherenceEventArgs>.Parse(messageElement));
        }

        protected void OnBillUpdated(XElement messageElement)
        {
            if (BillUpdated != null)
                BillUpdated(this, EventArgsBase<BillUpdatedEventArgs>.Parse(messageElement));
        }

        protected void OnTunnelDataReceived(XElement messageElement)
        {
            if (TunnelDataReceived != null)
                TunnelDataReceived(this, EventArgsBase<TunnelDataReceivedEventArgs>.Parse(messageElement));
        }

        protected void OnChallengeListModified(XElement messageElement)
        {
            if (ChallengeListModified != null)
                ChallengeListModified(this, EventArgsBase<ChallengeListModifiedeventArgs>.Parse(messageElement));
        }

        protected void OnPlayerInfoChanged(XElement messageElement)
        {
            if (PlayerInfoChanged != null)
                PlayerInfoChanged(this, EventArgsBase<PlayerInfoChangedEventArgs>.Parse(messageElement));
        }

        protected void OnManualFlowControlTransition(XElement messageElement)
        {
            if (ManualFlowControlTransition != null)
                ManualFlowControlTransition(this, EventArgsBase<ManualFlowControlTransitionEventArgs>.Parse(messageElement));
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