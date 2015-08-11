using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using EPQMessenger.States;

namespace EPQMessenger.Helpers
{
    /// <summary>
    /// Contains static extension methods for the TcpClient class.
    /// </summary>
    static class TcpClientExtensions
    {
        /// <summary>
        /// Sends a message to the specified TcpClient.
        /// </summary>
        /// <param name="client">The TcpClient to send the message to.</param>
        /// <param name="message">The message to send.</param>
        /// <param name="callback">An AsyncCallback which will be called when the message has been written to the
        /// network stream.</param>
        public static void Send(this TcpClient client, string message, AsyncCallback callback)
        {
            NetworkStream stream = client.GetStream();
            byte[] messageBuffer = Encoding.ASCII.GetBytes(message);
            stream.BeginWrite(messageBuffer, 0, messageBuffer.Length, BeginWriteCallback, (object)new WriteState(stream, callback));
        }

        /// <summary>
        /// Sends a message to the specified TcpClient.
        /// </summary>
        /// <param name="client">The TcpClient to send the message to.</param>
        /// <param name="message">The message to send.</param>
        public static void Send(this TcpClient client, string message)
        {
            NetworkStream stream = client.GetStream();
            byte[] messageBuffer = Encoding.ASCII.GetBytes(message);
            stream.BeginWrite(messageBuffer, 0, messageBuffer.Length, BeginWriteCallback, (object)new WriteState(stream, null));
        }

        /// <summary>
        /// Sends a message synchronously to the specified TcpClient.
        /// </summary>
        /// <param name="client">The TcpClient to send the message to.</param>
        /// <param name="message">The message to send.</param>
        public static void SendSync(this TcpClient client, string message)
        {
            NetworkStream stream = client.GetStream();
            byte[] messageBuffer = Encoding.ASCII.GetBytes(message);
            stream.Write(messageBuffer, 0, messageBuffer.Length);
        }

        private static void BeginWriteCallback(IAsyncResult result)
        {
            WriteState state = (WriteState)result.AsyncState;
            NetworkStream stream = state.Stream;
            AsyncCallback callback = state.Callback;
            stream.EndWrite(result);
            if (callback != null)
            {
                callback.Invoke(result);
            }
        }
    }
}
