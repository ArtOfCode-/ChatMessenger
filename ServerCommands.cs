using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Reflection;
using EPQMessenger.Helpers;
using EPQMessenger.Workers;

namespace EPQMessenger
{
    class ServerCommands
    {
        private Server _server;

        private CommandRegistry<ServerCommand> _registry;

        /// <summary>
        /// Initialises a new instance of the ServerCommands class.
        /// </summary>
        /// <param name="server">The instance of the Server running.</param>
        /// <param name="registry">A CommandRegistry to register commands to.</param>
        public ServerCommands(Server server, CommandRegistry<ServerCommand> registry)
        {
            _server = server;
            _registry = registry;
            _registry.Register(new ServerCommand("broadcast", false, Broadcast, "Broadcasts a message to every connected client."));
            _registry.Register(new ServerCommand("help", false, Help, "Provides help for the command utility."));
            _registry.Register(new ServerCommand("ban", true, BanUser, "Bans a user from the server, never to return!"));
            _registry.Register(new ServerCommand("kick", true, KickUser, "Kicks a user from their session. They may reconnect, but their sessions will be read-only until the server is restarted."));
            _registry.Register(new ServerCommand("restart", true, Restart, "Restarts the server, notifying clients that it's happening."));
        }

        public string Broadcast(string[] args)
        {
            if (args.Length > 0)
            {
                if (args.Length == 1 && args[0] == "")
                {
                    return "You must include a message to send!";
                }
                string message = string.Join(" ", args);
                foreach (KeyValuePair<string, TcpClient> pair in _server.GetClients())
                {
                    pair.Value.Send(Protocol.GetResponseFromCode(302) + "\n" + "<Server>" + message);
                }
                return "Successfully broadcast.";
            }
            else
            {
                return "You must include a message to send!";
            }
        }

        public string Help(string[] args)
        {
            if (args.Length == 0)
            {
                string helpText = "COMMAND HELP\n";
                foreach (string commandName in _registry.ListCommandNames())
                {
                    helpText += commandName + ": " + _registry.GetCommandHelp(commandName) + "\n";
                }
                return helpText;
            }
            else
            {
                string commandHelp = _registry.GetCommandHelp(args[0]);
                if (commandHelp == null) return "No such command.";
                else return args[0] + ": " + commandHelp;
            }
        }

        public string BanUser(string[] args)
        {
            if (args.Length == 0)
            {
                return "You must specify a user to ban!";
            }
            else
            {
                bool result = Server.DisconnectClient(args[0], 106);
                App.BannedUsers.Add(args[0]);
                App.SaveBannedUsers();
                return "User banned" + (result ? " and disconnected." : ", but could not be disconnected.");
            }
        }

        public string KickUser(string[] args)
        {
            if (args.Length == 0)
            {
                return "You must specify a user to kick!";
            }
            else
            {
                return Server.DisconnectClient(args[0], 105) ? "User kicked." : "User could not be kicked.";
            }
        }

        public string Restart(string[] args)
        {
            App.LogDevice.Log("[restart] Server restarted by an administrator.");
            _server.StopServer("an administrator has asked for a reboot");
            Process.Start(Assembly.GetExecutingAssembly().CodeBase, string.Join(" ", Environment.GetCommandLineArgs()));
            Thread.Sleep(50);
            Environment.Exit(0);
            return "Restarting server.";
        }
    }
}
