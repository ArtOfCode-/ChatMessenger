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
    /// A StateObject used in callbacks from NetworkStream.BeginWrite.
    /// </summary>
    class WriteState
    {
        /// <summary>
        /// The NetworkStream on which the callback acts.
        /// </summary>
        public NetworkStream Stream { get; private set; }

        /// <summary>
        /// A user-specified second callback to call when the operation is complete.
        /// Note: calling of this method is not handled by this class.
        /// </summary>
        public AsyncCallback Callback { get; private set; }

        /// <summary>
        /// Initialises a new instance of the WriteState class, using the specified parameters.
        /// </summary>
        /// <param name="stream">The NetworkStream on which the callback acts.</param>
        /// <param name="callback">The AsyncCallback to be called when the operation completes.</param>
        public WriteState(NetworkStream stream, AsyncCallback callback)
        {
            Stream = stream;
            Callback = callback;
        }
    }
}
