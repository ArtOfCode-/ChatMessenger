using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Windows.Media;
using System.IO;
using EPQMessenger.Windows;
using EPQMessenger.States;
using EPQMessenger.Helpers;

namespace EPQMessenger.Workers
{
    class Client
    {
        private ClientWindow _window;

        private TcpClient _client;

        /// <summary>
        /// Initialises a new instance of the Client class, using the specified parameters.
        /// </summary>
        /// <param name="window">The open ClientWindow with which to interact.</param>
        public Client(ClientWindow window)
        {
            _window = window;
            _client = new TcpClient();
        }

        /// <summary>
        /// Tries to connect the client to a remote server.
        /// </summary>
        /// <param name="ipAddress">The (string) IP address of the remote server.</param>
        /// <param name="port">The server port.</param>
        /// <returns>A bool representing whether or not the operation succeeded.</returns>
        public bool Connect(string ipAddress, int port)
        {
            IPAddress holder = IPAddress.None;
            bool valid = IPAddress.TryParse(ipAddress, out holder);
            if (valid)
            {
                return this.Connect(holder, port);
            }
            return false;
        }

        /// <summary>
        /// Tries to connect the client to a remote server.
        /// </summary>
        /// <param name="ipAddress">The IP address of the remote server.</param>
        /// <param name="port">The server port.</param>
        /// <returns>A bool representing whether or not the operation succeeded.</returns>
        public bool Connect(IPAddress ipAddress, int port)
        {
            try
            {
                _client.Connect(new IPEndPoint(ipAddress, port));
            }
            catch (SocketException)
            {
                return false;
            }
            this.Receive(ConnectedCallback);
            return true;
        }

        /// <summary>
        /// Sends a string to the remote server.
        /// </summary>
        /// <param name="message">The message to send.</param>
        public void Send(string message)
        {
            Console.WriteLine("[Client.Send] Sending: {0}", message);
            if (_client.Connected)
            {
                _window.ChangeStatus("Sending", Color.FromRgb(204, 81, 0));
                NetworkStream stream = _client.GetStream();
                byte[] bytes = Encoding.ASCII.GetBytes(message);
                try
                {
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush();
                }
                catch (IOException)
                {
                    _window.AddMessage("could not send - try again", "Server", Colors.Crimson);
                }
                this.Receive(ReceivedCallback);
            }
        }

        /// <summary>
        /// Waits and receives data from the remote server, and passes it on to a callback.
        /// </summary>
        /// <param name="callback">The ReadCallback to pass data to when the operation completes.</param>
        public void Receive(ReadCallback callback)
        {
            NetworkStream stream;
            try
            {
                stream = _client.GetStream();
            }
            catch (InvalidOperationException)
            {
                _window.AddMessage("fatal: failed to receive new messages", "Server", Colors.Crimson);
                _window.Dispatcher.BeginInvoke(new Action(delegate()
                {
                    _window.SendButton.IsEnabled = false;
                    _window.MessageInput.IsEnabled = false;
                }));
                return;
            }
            while (!stream.DataAvailable)
            {
                Thread.Sleep(50);
            }
            byte[] readBuffer = new byte[_client.Available];
            stream.BeginRead(readBuffer, 0, readBuffer.Length, ReceiveCallbackInternal, 
                new ExtendedReadState(_client, readBuffer, callback));
        }

        /// <summary>
        /// INTERNAL: The method used by Receive's stream.BeginRead call to handle the callback from the read.
        /// </summary>
        /// <param name="result">The IAsyncResult of the read operation.</param>
        private void ReceiveCallbackInternal(IAsyncResult result)
        {
            ExtendedReadState state = (ExtendedReadState)result.AsyncState;
            NetworkStream stream = state.Stream;
            byte[] buffer = state.Buffer;
            ReadCallback callback = state.Callback;

            stream.EndRead(result);

            Console.WriteLine("[Client.ReceiveCallbackInternal] Received: {0}", Encoding.ASCII.GetString(buffer));

            callback.Invoke(Encoding.ASCII.GetString(buffer));
        }

        /// <summary>
        /// INTERNAL: The ReadCallback used when the client has made a connection to the server, and needs
        /// to authenticate with a username.
        /// </summary>
        /// <param name="received">The string response received from the server.</param>
        private void ConnectedCallback(string received)
        {
            int code = 0;
            try
            {
                code = Protocol.GetCodeFromResponse(received);
            }
            catch (FormatException)
            {
                _window.ChangeStatus("Connection failed", Colors.Crimson);
            }
            if (code == 202)
            {
                string message = Protocol.GetResponseFromCode(300) + "\n" + Environment.UserName;
                this.Send(message);
                Thread listenThread = new Thread(new ThreadStart(ListenForMessages));
                listenThread.SetApartmentState(ApartmentState.STA);
                listenThread.Start();
            }
            else
            {
                _window.ChangeStatus("Connection failed", Colors.Crimson);
            }
        }

        /// <summary>
        /// Used by Send as the callback from sending a message to the server. Handles the server's confirmation
        /// of receipt.
        /// </summary>
        /// <param name="received"></param>
        private void ReceivedCallback(string received)
        {
            int code = Protocol.GetCodeFromResponse(received);
            if (code == 201 || code == 101)
            {
                _window.ResetStatus();
            }
            else
            {
                this.Receive(ReceivedCallback);
            }
        }

        /// <summary>
        /// INTERNAL, NEW-THREAD: Waits for a message to arrive then reads it in asynchronously.
        /// </summary>
        private void ListenForMessages()
        {
            NetworkStream stream = _client.GetStream();
            while (!App.StopAllThreads)
            {
                while (!stream.DataAvailable)
                {
                    Thread.Sleep(50);
                }
                byte[] buffer = new byte[_client.Available];
                stream.BeginRead(buffer, 0, buffer.Length, MessageReceivedCallback, new ReadState(_client, buffer));
            }
        }

        /// <summary>
        /// INTERNAL: Used from ListenForMessages as the AsyncCallback from the read operation.
        /// Adds the received message to the ClientWindow.
        /// </summary>
        /// <param name="result"></param>
        [STAThread]
        private void MessageReceivedCallback(IAsyncResult result)
        {
            ReadState state = (ReadState)result.AsyncState;
            NetworkStream stream = state.Stream;
            byte[] buffer = state.Buffer;

            stream.EndRead(result);

            string message = Encoding.ASCII.GetString(buffer);

            Console.WriteLine("Message received: {0}", message);

            this.HandleMessage(message);
        }

        private void HandleMessage(string message)
        {
            int code = 0;
            try
            {
                code = Protocol.GetCodeFromResponse(message);
            }
            catch (FormatException) { return; }

            switch (code)
            {
                case 302:
                    string name = message.FindContainedText("<", ">");
                    string text = message.Substring(message.IndexOf(">") + 1);
                    if (name == Environment.UserName)
                    {
                        Thread addThread = new Thread(new ParameterizedThreadStart((window) =>
                        {
                            ((ClientWindow)window).AddMessage(text, name, Colors.DarkBlue);
                        }));
                        addThread.SetApartmentState(ApartmentState.STA);
                        addThread.Start(_window);
                    }
                    else
                    {
                        Thread addThread = new Thread(new ParameterizedThreadStart((window) => 
                        {
                            Console.WriteLine("[Client.HandleMessage:Thread] Thread started.");
                            ((ClientWindow)window).AddMessage(text, name, Colors.DarkGreen);
                        }));
                        addThread.SetApartmentState(ApartmentState.STA);
                        addThread.Start(_window);
                    }
                    break;
                case 201:
                    _window.ResetStatus();
                    break;
                default:
                    string errorText = string.Format("encountered {0} {1} error - try again", code, Protocol.StatusCodes[code]);
                    _window.AddMessage(errorText, "Server", Colors.Crimson);
                    break;
            }
        }

        /// <summary>
        /// Closes all disposable resources and notifies the server of a disconnect.
        /// </summary>
        public void Close()
        {
            if (_client.Connected)
            {
                this.Send(Protocol.GetResponseFromCode(301));
            }
        }
    }
}
