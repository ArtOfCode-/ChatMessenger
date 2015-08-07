using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using EPQMessenger.Helpers;

namespace EPQMessenger.States
{
    /// <summary>
    /// An extended version of the ReadState class, incorporating a callback.
    /// </summary>
    class ExtendedReadState : ReadState
    {
        /// <summary>
        /// The ReadCallback that should be called by the calling code when the relevant operation is done.
        /// </summary>
        public ReadCallback Callback { get; private set; }

        /// <summary>
        /// Initialises a new instance of the ExtendedReadState class, using the specified parameters.
        /// </summary>
        /// <param name="client">The TcpClient from which the data has been read.</param>
        /// <param name="buffer">The buffer containing the read data.</param>
        /// <param name="callback">The ReadCallback which should be called by the calling code when the 
        /// relevant operation is done.</param>
        public ExtendedReadState(TcpClient client, byte[] buffer, ReadCallback callback)
            : base(client, buffer)
        {
            this.Callback = callback;
        }
    }
}
