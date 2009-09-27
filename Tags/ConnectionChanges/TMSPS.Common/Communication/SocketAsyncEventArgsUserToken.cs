using System.Net.Sockets;

namespace TMSPS.Core.Communication
{
    public class SocketAsyncEventArgsUserToken
    {
        public Socket Socket { get; set; }
        public int SizeToReceive { get; set;}
        public string CurrentRawMessage { get; set; }
        public int CurrentRawMessageLength { get; set; }
        public uint? MessageID { get; set; }

        public int RemainingBytesToReceive
        {
            get { return SizeToReceive - CurrentRawMessageLength; }
        }

        public bool MoreBytesNeedToBeRead
        {
            get { return RemainingBytesToReceive > 0; }
        }
    }
}