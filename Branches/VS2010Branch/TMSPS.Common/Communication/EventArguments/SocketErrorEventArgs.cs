using System;
using System.Net.Sockets;

namespace TMSPS.Core.Communication.EventArguments
{
    public class SocketErrorEventArgs : EventArgs
    {
        public SocketError SocketError
        {
            get;
            private set;
        }

        public SocketErrorEventArgs(SocketError socketError)
        {
            SocketError = socketError;
        }
    }
}