using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using EPQMessenger.States;
using EPQMessenger.Windows;
using EPQMessenger.Helpers;

namespace EPQMessenger.Workers
{
    /// <summary>
    /// The basic thread used to handle communications with a single client.
    /// </summary>
    class ServerCommunicationThread
    {
        private readonly string _username;

        /// <summary>
        /// Initialises a new instance of the ServerCommunicationThread class.
        /// </summary>
        /// <param name="client">The TcpClient whose communications this thread will handle.</param>
        /// <param name="username">The username of the TcpClient.</param>
        /// <param name="console">The ServerWindow object containing the output console.</param>
        public ServerCommunicationThread(TcpClient client, string username, ServerWindow console)
        {
            _username = username;
            new Thread(new ParameterizedThreadStart(ThreadMethod)).Start(new ServerCommunicationState(client, console));
        }

        private void ThreadMethod(object stateObject)
        {
            ServerCommunicationState state = (ServerCommunicationState)stateObject;

            TcpClient client = state.Client;
            ServerWindow console = state.Window;

            NetworkStream stream = client.GetStream();

            client.Send(Protocol.GetResponseFromCode(101));

            while (!App.StopAllThreads)
            {
                bool dataAvailable = false;
                while (!dataAvailable && !App.StopAllThreads)
                {
                    Thread.Sleep(50);
                    try
                    {
                        dataAvailable = stream.DataAvailable;
                    }
                    catch (ObjectDisposedException)
                    {
                        Server.RemoveClient(_username);
                        return;
                    }
                }
                byte[] readBuffer = new byte[client.Available];
                stream.BeginRead(readBuffer, 0, readBuffer.Length, ReadCallback, new ServerReadState(client, readBuffer, console));
            }
        }

        private void ReadCallback(IAsyncResult result)
        {
            ServerReadState state = (ServerReadState)result.AsyncState;

            TcpClient client = state.Client;
            NetworkStream stream = state.Stream;
            byte[] buffer = state.Buffer;
            ServerWindow console = state.Window;

            string response = Encoding.ASCII.GetString(buffer);

            console.Log("Message from client {0}:\n{1}", _username, response);

            stream.EndRead(result);

            int responseCode = 0;
            try
            {
                responseCode = Protocol.GetCodeFromResponse(response);
            }
            catch (FormatException e)
            {
                client.Send(Protocol.GetResponseFromCode(400));
                console.Log("Could not parse response code: {0}. 400 Bad Request sent.", e.Message);
                return;
            }

            if (responseCode < 300 || responseCode > 399)
            {
                client.Send(Protocol.GetResponseFromCode(400));
                console.Log("Client sent status code < 300 || > 399. 400 Bad Request sent.");
                return;
            }

            this.HandleMessage(response, responseCode, client, console);
        }

        private void HandleMessage(string fullMessage, int code, TcpClient client, ServerWindow console)
        {
            string[] response = null;
            if (code == 302)
            {
                response = Protocol.GetClientResponse(fullMessage);
            }

            switch (code)
            {
                case 300:
                    client.Send(Protocol.GetResponseFromCode(404));
                    console.Log("Client sent 300 Response, but no question was asked. 404 No Question sent.");
                    break;
                case 301:
                    Server.RemoveClient(_username);
                    console.Log("Client sent 301 Disconnecting. Client removed from list, closing resources.");
                    client.Close();
                    Thread.CurrentThread.Abort();
                    break;
                case 302:
                    Server.SendMessage(string.Join("\n", response), _username);
                    client.Send(Protocol.GetResponseFromCode(201));
                    console.Log("201 Received sent.");
                    break;
            }
        }
    }
}
