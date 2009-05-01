using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;
using TMSPS.Core.Communication.EventArguments;
using TMSPS.Core.Communication.ResponseHandling;
using TMSPS.Core.Logging;
using ProtocolType=System.Net.Sockets.ProtocolType;
using Socket=System.Net.Sockets.Socket;
using SocketAsyncEventArgs=System.Net.Sockets.SocketAsyncEventArgs;
using SocketType=System.Net.Sockets.SocketType;

namespace TMSPS.Core.Communication
{
    public delegate Boolean SocketAsyncMethod(SocketAsyncEventArgs args);

    public class TrackManiaRPCClient
    {
        #region Constants

        private const int DEFAULT_PORT = 5000;

        #endregion

        #region Non Public Members

		private readonly object _readLock = new object();
        private readonly object _sendLock = new object();
        private readonly Callbacks _callBacks;
        private readonly Methods _methods;
        private readonly Queue<string> _methodResponseQueue = new Queue<string>();

        #endregion

        #region Properties

        public string Host
        {
            get; protected set;
        }

        public int? Port
        {
            get; protected set;
        }

        public SocketAsyncEventArgs SocketAsyncEventArgs
        {
            get; protected set;
        }

        public Socket Socket
        {
            get; set;
        }

        public Callbacks Callbacks
        {
            get { return _callBacks; }
        }

        public Methods Methods
        {
            get { return _methods; }
        }

        private Queue<string> MethodResponseQueue
        {
            get { return _methodResponseQueue; }
        }

        #endregion

        #region Events

        public event EventHandler Connected;
        public event EventHandler ServerClosedConnection;
        public event EventHandler<SocketErrorEventArgs> SocketError;
        public event EventHandler ReadyForSendingCommands;

        #endregion

        #region Constructors

        public TrackManiaRPCClient(): this(null)
        {

        }

        public TrackManiaRPCClient(string host) : this(host, null)
        {

        }

        public TrackManiaRPCClient(string host, int? port)
        {
            Host = host;
            Port = port;

            SocketError += TMNRPCClient_SocketError;
            _callBacks = new Callbacks();
            _methods = new Methods(this);
        }

        #endregion

        #region Public Methods

        public void Connect()
        {
            Connect(Host, Port);
        }

        public void Connect(string host)
        {
            Connect(host, Port);
        }

        public void Connect(string host, int? port)
        {
            if (SocketAsyncEventArgs != null)
                return;

            MethodResponseQueue.Clear();

            if (port == null)
                port = DEFAULT_PORT;

            if (host == null || host.Trim().Length == 0)
                host = "localhost";

            SocketAsyncEventArgs = new SocketAsyncEventArgs();

            IPAddress remoteIP;
            if (!IPAddress.TryParse(host, out remoteIP))
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(host);

                if (hostEntry.AddressList.Length == 0)
                    throw new InvalidOperationException(string.Format("Could not resolve host: {0}", host));

                remoteIP = hostEntry.AddressList[0];
            }

            IPEndPoint remoteEndPoint = new IPEndPoint(remoteIP, port.Value);
            Socket = new Socket(remoteEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            SocketAsyncEventArgs.RemoteEndPoint = remoteEndPoint;
            SocketAsyncEventArgs.UserToken = new SocketAsyncEventArgsUserToken {Socket = Socket};
            SocketAsyncEventArgs.Completed += Client_Connected;

            Socket.InvokeAsyncMethod(Socket.ConnectAsync, Client_Connected, SocketAsyncEventArgs);
        }

        public void Disconnect()
        {
            if (SocketAsyncEventArgs == null)
                return;

            if (SocketAsyncEventArgs.SocketError != System.Net.Sockets.SocketError.Success)
            {
                SocketAsyncEventArgs = null;
                return;
            }

            ((SocketAsyncEventArgsUserToken)SocketAsyncEventArgs.UserToken).Socket.Shutdown(SocketShutdown.Both);
            SocketAsyncEventArgs = null;
        }

        public ResponseBase<T> SendMethod<T>(string method, params object[] parameters) where T : ResponseBase<T>
        {
            string xml = RPCCommand.GetMethodCallElementWithXmlDeclaration(method, parameters).ToString(SaveOptions.DisableFormatting);
            string response;
            
            lock (_sendLock)
            {
                Send(xml);
                response = GetNextMethodResponseFromQueue();
            }

            XElement messageElement = TryParseXElement(response);
            return ResponseBase<T>.Parse(messageElement);
        }

        #endregion

        #region Non Public Methods

        private string GetNextMethodResponseFromQueue()
        {
            while (true)
            {
				string message = MethodResponseQueue.Dequeue(msg => true, _readLock);

				if (message != null)
					return message;

                Thread.Sleep(10);
            }
        }

        private int Send(string message)
        {
            try
            {
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                byte[] sizeBytes = BitConverter.GetBytes(messageBytes.Length);
                byte[] handleBytes = BitConverter.GetBytes(-1);

                byte[] sendMessageBuffer = new byte[messageBytes.Length + sizeBytes.Length + handleBytes.Length];
                sizeBytes.CopyTo(sendMessageBuffer, 0);
                handleBytes.CopyTo(sendMessageBuffer, 4);
                messageBytes.CopyTo(sendMessageBuffer, 8);

                return Socket.Send(sendMessageBuffer, 0, sendMessageBuffer.Length, SocketFlags.None);
            }
            catch (SocketException ex)
            {
                OnSocketError(ex.SocketErrorCode);
                throw;
            }
        }

        private void Client_MessageSent(object sender, SocketAsyncEventArgs e)
        {
            //ReadMessageWithPrefix(e, Client_MessageSent, true);
        }

        private void Client_Connected(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == System.Net.Sockets.SocketError.Success)
            {
                if (Connected != null)
                    Connected(this, EventArgs.Empty);

                ReadMessageWithPrefix(e, Client_Connected, false);
            }
            else
                OnSocketError(e.SocketError);
        }

        private void ReadMessageWithPrefix(SocketAsyncEventArgs e, EventHandler<SocketAsyncEventArgs> previousMessageHandler, bool includeHandle)
        {
            SocketAsyncEventArgsUserToken userToken = (SocketAsyncEventArgsUserToken)e.UserToken;

            byte[] sizeBuffer = new byte[includeHandle ? 8 : 4];
            e.SetBuffer(sizeBuffer, 0, sizeBuffer.Length);

            if (previousMessageHandler != null)
                e.Completed -= previousMessageHandler;

            e.Completed += Client_SizeReceived;

            userToken.Socket.InvokeAsyncMethod(userToken.Socket.ReceiveAsync, Client_SizeReceived, e);
        }

        private void Client_SizeReceived(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == System.Net.Sockets.SocketError.Success)
            {
                SocketAsyncEventArgsUserToken userToken = (SocketAsyncEventArgsUserToken)e.UserToken;

                byte[] sizeBuffer = new[] { e.Buffer[0], e.Buffer[1], e.Buffer[2], e.Buffer[3] };
                userToken.SizeToReceive = BitConverter.ToInt32(sizeBuffer, 0);
                userToken.CurrentRawMessageLength = 0;
                userToken.CurrentRawMessage = string.Empty;

                byte[] messageBuffer = new byte[userToken.SizeToReceive];
                e.SetBuffer(messageBuffer, 0, messageBuffer.Length);
                e.Completed -= Client_SizeReceived;
                e.Completed += Client_MessageReceived;

                userToken.Socket.InvokeAsyncMethod(userToken.Socket.ReceiveAsync, Client_MessageReceived, e);
            }
            else
            {
                OnSocketError(e.SocketError);
            }
        }

        private void Client_MessageReceived(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == System.Net.Sockets.SocketError.Success)
            {
                SocketAsyncEventArgsUserToken userToken = (SocketAsyncEventArgsUserToken)e.UserToken;
                userToken.CurrentRawMessageLength += e.BytesTransferred;
                userToken.CurrentRawMessage += Encoding.UTF8.GetString(e.Buffer, e.Offset, e.BytesTransferred);

                e.Completed -= Client_SizeReceived;
                e.Completed -= Client_MessageReceived;

                if (userToken.MoreBytesNeedToBeRead)
                {   
                    byte[] messageBuffer = new byte[userToken.RemainingBytesToReceive];
                    e.SetBuffer(messageBuffer, 0, messageBuffer.Length);
                    
                    e.Completed += Client_MessageReceived;
                    userToken.Socket.InvokeAsyncMethod(userToken.Socket.ReceiveAsync, Client_MessageReceived, e);
                }
                else
                {
                    if (string.Compare(userToken.CurrentRawMessage, "GBXRemote 2", StringComparison.OrdinalIgnoreCase) == 0)
                        (new MethodInvoker(OnReadyForSendingCommands)).BeginInvoke(null, null);
                    else
                    {
                        if (userToken.CurrentRawMessage != null)
                        {
                            if (userToken.CurrentRawMessage.IndexOf("<methodCall>", StringComparison.OrdinalIgnoreCase) != -1)
                            {
                                // message is a callback
                                XElement messageElement = TryParseXElement(userToken.CurrentRawMessage);

                                if (messageElement != null && !Callbacks.CheckForKnownMethodCallback(messageElement))
                                    CoreLogger.UniqueInstance.Error("Found unknown callback: " + userToken.CurrentRawMessage);
                            }
                            else
                            {
                                // message is a method reply
                                MethodResponseQueue.Enqueue(userToken.CurrentRawMessage);
                            }
                        }
                    }

                    ReadMessageWithPrefix(e, Client_MessageSent, true);
                }
            }
            else
            {
                OnSocketError(e.SocketError);
            }
        }

        private void OnReadyForSendingCommands()
        {
            if (ReadyForSendingCommands != null)
                ReadyForSendingCommands(this, EventArgs.Empty);
        }

        protected void OnSocketError(SocketError socketError)
        {
            if (SocketError != null)
                SocketError(this, new SocketErrorEventArgs(socketError));
        }

        private void TMNRPCClient_SocketError(object sender, SocketErrorEventArgs e)
        {
            SocketAsyncEventArgs = null;

            if (e.SocketError == System.Net.Sockets.SocketError.ConnectionReset && ServerClosedConnection != null)
                ServerClosedConnection(this, EventArgs.Empty);
        }

        private static XElement TryParseXElement(string xmlFragment)
        {
            try
            {
                return XElement.Parse(xmlFragment);
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion
    }
}