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
    class ServerReadState : ReadState
    {
        public ServerWindow Window { get; private set; }

        public ServerReadState(TcpClient client, byte[] buffer, ServerWindow window)
            : base(client, buffer)
        {
            Window = window;
        }
    }
}
