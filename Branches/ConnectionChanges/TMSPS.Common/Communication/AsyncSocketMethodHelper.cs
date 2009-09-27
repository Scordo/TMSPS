using System;
using System.Net.Sockets;

namespace TMSPS.Core.Communication
{
    internal static class AsyncSocketMethodHelper
    {
        /// <summary>
        /// Represents one of the new Socket xxxAsync methods in .NET 3.5.
        /// </summary>
        /// <param name="socket">The socket.</param>
        /// <param name="method">The method.</param>
        /// <param name="callback">The callback.</param>
        /// <param name="args">The SocketAsyncEventArgs for use with the method.</param>
        public static void InvokeAsyncMethod(this Socket socket, SocketAsyncMethod method, EventHandler<SocketAsyncEventArgs> callback, SocketAsyncEventArgs args)
        {
            if (!method(args))
                callback(socket, args);
        }
    }
}