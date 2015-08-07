using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using EPQMessenger.Windows;

namespace EPQMessenger.States
{
    class ServerCommunicationState
    {
        public TcpClient Client { get; private set; }

        public ServerWindow Window { get; private set; }

        public ServerCommunicationState(TcpClient client, ServerWindow window)
        {
            Client = client;
            Window = window;
        }
    }
}
