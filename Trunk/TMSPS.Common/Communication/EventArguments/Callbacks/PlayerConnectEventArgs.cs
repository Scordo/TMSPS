using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.Communication.EventArguments.Callbacks
{
    public class PlayerConnectEventArgs : EventArgsBase<PlayerConnectEventArgs>
    {
        #region Properties

        [RPCParam(0)]
        public string Login { get; private set; }

        [RPCParam(1)]
        public bool IsSpectator { get; private set; }

        public bool Handled { get; set; }

        #endregion

        #region Consturctors

        public PlayerConnectEventArgs()
        {
            Handled = false;
        }

        #endregion
    }
}