using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using EPQMessenger.Windows;
using EPQMessenger.Workers;

namespace EPQMessenger.States
{
    class ServerCommunicationState
    {
        public TcpClient Client { get; private set; }

        public ServerWindow Window { get; private set; }

        public Server Server { get; private set; }

        public ServerCommunicationState(TcpClient client, ServerWindow window, Server server)
        {
            Client = client;
            Window = window;
            Server = server;
        }
    }
}
