using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace EPQMessenger.States
{
    /// <summary>
    /// A StateObject used for callbacks from NetworkStream.BeginRead.
    /// </summary>
    class ReadState
    {
        /// <summary>
        /// The TcpClient that the NetworkStream belongs to.
        /// </summary>
        public TcpClient Client { get; private set; }

        /// <summary>
        /// The NetworkStream on which the callback is acting.
        /// </summary>
        public NetworkStream Stream { get; private set; }

        /// <summary>
        /// The read buffer, containing any data read from the network.
        /// </summary>
        public byte[] Buffer { get; private set; }

        /// <summary>
        /// Initialises a new instance of the ReadState class, with the specified parameters.
        /// </summary>
        /// <param name="client">The TcpClient to which the NetworkStream belongs.</param>
        /// <param name="buffer">The byte array that represents the read buffer.</param>
        public ReadState(TcpClient client, byte[] buffer)
        {
            Client = client;
            Stream = client.GetStream();
            Buffer = buffer;
        }
    }
}
