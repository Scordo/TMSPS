using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.Communication.EventArguments.Callbacks
{
    public class PlayerChatEventArgs : EventArgsBase<PlayerChatEventArgs>
    {
        #region Properties

        [RPCParam(0)]
        public int PlayerID { get; set; }
        [RPCParam(1)]
        public string Login { get; set; }
        [RPCParam(2)]
        public string Text { get; set; }
        [RPCParam(3)]
        public bool IsRegisteredCommand { get; set; }

        public bool IsServerMessage
        {
            get { return PlayerID == 0; }
        }

        #endregion
    }
}