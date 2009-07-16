using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.Communication.EventArguments.Callbacks
{
    public class PlayerDisconnectEventArgs : EventArgsBase<PlayerDisconnectEventArgs>
    {
        #region Properties

        [RPCParam(0)]
        public string Login
        {
            get; set;
        }

        #endregion
    }
}