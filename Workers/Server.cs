using EPQMessenger.Windows;
using EPQMessenger.States;
using EPQMessenger.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Media;
using System.IO;

namespace EPQMessenger.Workers
{
    /// <summary>
    /// The behind-the-scenes operation class that actually does all the server work.
    /// </summary>
    public class Server
    {
        private static ServerWindow _console;

        private static Dictionary<string, TcpClient> _clients;

        private static ConversationLogger _logger = new ConversationLogger();

        private TcpListener _listener;

        /// <summary>
        /// Gets (or privately, sets) the port on which the server will operate.
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// A list of usernames whose threads require termination.
        /// </summary>
        public static List<string> StopThreads { get; set; }

        /// <summary>
        /// Initialises a new instance of Server, using the specified console window and port number.
        /// </summary>
        /// <param name="console">A ServerWindow representing the currently open server console Window.</param>
        /// <param name="port">The port number to start the server on.</param>
        public Server(ServerWindow console, int port)
        {
            _console = console;
            Port = port;
            _clients = new Dictionary<string, TcpClient>();
            _logger.IsEnabled = true;
            StopThreads = new List<string>();
        }

        /// <summary>
        /// Starts the Server instance: initialises a TcpListener and starts it listening on the server port.
        /// </summary>
        public void Start()
        {
            _listener = new TcpListener(IPAddress.Any, Port);
            _listener.Start();
            _listener.BeginAcceptTcpClient(AcceptClientCallback, _listener);
        }

        /// <summary>
        /// The callback from TcpListener.BeginAcceptTcpClient - handles a client connection.
        /// </summary>
        /// <param name="result">The IAsyncResult passed from BeginAcceptTcpClient.</param>
        private void AcceptClientCallback(IAsyncResult result)
        {
            TcpListener listener = (TcpListener)result.AsyncState;
            TcpClient connectedClient;
            try
            {
                connectedClient = listener.EndAcceptTcpClient(result);
            }
            catch (ObjectDisposedException)
            {
                _console.Log("Could not end listen call on the listener - object has been disposed.");
                return;
            }

            _console.Log("Client connected from {0}: send 202 Continue and await identification.", 
                connectedClient.Client.RemoteEndPoint.ToString());
            connectedClient.Send(Protocol.GetResponseFromCode(202));

            NetworkStream clientStream = connectedClient.GetStream();
            while (!clientStream.DataAvailable)
            {
                Thread.Sleep(100);
            }
            byte[] readBuffer = new byte[connectedClient.Available];
            clientStream.BeginRead(readBuffer, 0, readBuffer.Length, ReadCallback, new ReadState(connectedClient, readBuffer));

            try
            {
                listener.BeginAcceptTcpClient(AcceptClientCallback, listener);
            }
            catch (ObjectDisposedException)
            {
                _console.Log("Could not reload listener for next client - object has been disposed.");
            }
        }

        /// <summary>
        /// The callback from NetworkStream.BeginRead. Handles reception of identification for a client, and
        /// starts a client-specific communication thread when the client is identified.
        /// </summary>
        /// <param name="result">The IAsyncResult passed from BeginRead.</param>
        private void ReadCallback(IAsyncResult result)
        {
            ReadState state = (ReadState)result.AsyncState;

            TcpClient client = state.Client;
            NetworkStream stream = state.Stream;
            byte[] buffer = state.Buffer;

            stream.EndRead(result);

            string received = Encoding.ASCII.GetString(buffer);

            string response;
            try
            {
                response = Protocol.GetClientResponse(received);
                if (App.BannedUsers.Contains(response))
                {
                    _console.Log("User identified as a banned user, connection denied.");
                    client.SendSync(Protocol.GetResponseFromCode(106));
                    client.Close();
                    return;
                }
                _clients.Add(response, client);      // response contains the username
                new ServerCommunicationThread(client, response, _console, this);
                _console.Log("Client '{0}' finished connecting, communication thread spawned.", response);
            }
            catch (Exception e)
            {
                if (e is InvalidStatusException || e is FormatException)
                {
                    client.Send(Protocol.GetResponseFromCode(401));
                }
            }
        }

        /// <summary>
        /// Removes a client from the list of connected clients.
        /// </summary>
        /// <param name="username">The username of the client to remove.</param>
        /// <returns>The result of the call to Dictionary.Remove.</returns>
        public static bool RemoveClient(string username)
        {
            return _clients.Remove(username);
        }

        /// <summary>
        /// Closes a client's connection to the server and drops the connection thread.
        /// </summary>
        /// <param name="username">The username of the client to kick.</param>
        /// <param name="reasonCode">The Protocol code to send to the client immediately before the kick.</param>
        /// <returns>A bool, indicating whether or not the kick was successful.</returns>
        public static bool DisconnectClient(string username, int reasonCode)
        {
            if (_clients.ContainsKey(username))
            {
                TcpClient client = _clients[username];
                client.Send(Protocol.GetResponseFromCode(reasonCode));
                client.Close();
                StopThreads.Add(username);
                return _clients.Remove(username);
            }
            else return false;
        }

        /// <summary>
        /// Sends a message from a specified user to all connected clients.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="username">The user to attribute the message to.</param>
        public static void SendMessage(string message, string username)
        {
            _logger.LogWithoutCallerInfo(string.Format("[{0}] {1}", username, message));
            foreach (KeyValuePair<string, TcpClient> pair in _clients)
            {
                try
                {
                    pair.Value.Send(string.Format("{0}\n<{1}>{2}", Protocol.GetResponseFromCode(302), username, message));
                }
                catch (IOException e)
                {
                    _console.Log("Failed to send to {0}: {1}", pair.Key, e.Message);
                }
            }
            _console.Log("Sent message from {0} to all clients.", username);
        }

        /// <summary>
        /// Sends notification that the server is shutting down to all connected clients.
        /// </summary>
        public void SendShutdown()
        {
            foreach (KeyValuePair<string, TcpClient> pair in _clients)
            {
                try
                {
                    pair.Value.SendSync(Protocol.GetResponseFromCode(104));
                    _console.Log("Sent shutdown signal to {0}", pair.Key);
                }
                catch (IOException e)
                {
                    _console.Log("Failed to send shutdown to '{0}': {1}", pair.Key, e.Message);
                }
                catch (InvalidOperationException e)
                {
                    _console.Log("Failed to send shutdown to '{0}': {1}", pair.Key, e.Message);
                }
            }
        }

        /// <summary>
        /// Returns the client list to the caller.
        /// </summary>
        /// <returns>The client list, as a Dictionary&lt;string, TcpClient&gt;</returns>
        public Dictionary<string, TcpClient> GetClients()
        {
            return _clients;
        }

        /// <summary>
        /// Gracefully stops the server running after an error and notifies all clients.
        /// </summary>
        public void StopServer(string reason)
        {
            SendMessage("The server is restarting because " + reason + ". Your connection will be dropped. The server will be available again shortly.", "Server");
            Thread.Sleep(50);
            this.SendShutdown();
            _listener.Stop();
        }
    }
}
