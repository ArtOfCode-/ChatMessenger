using EPQMessenger.Workers;
using EPQMessenger.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;

namespace EPQMessenger.Windows
{
    /// <summary>
    /// Interaction logic for ServerWindow.xaml
    /// </summary>
    public partial class ServerWindow : Window
    {
        /// <summary>
        /// The Server instance that this applciation is running.
        /// </summary>
        public Server RunningServer { get; set; }

        private Logger _logger;

        private CommandRegistry<ServerCommand> _commands;

        /// <summary>
        /// Initialises a new instance of the ServerWindow class and sets up the required values and
        /// objects for use.
        /// </summary>
        public ServerWindow(bool crashRecovery)
        {
            InitializeComponent();
            _logger = new Logger();
            _logger.IsEnabled = true;
            Overlay.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#44FFFFFF"));
            this.Log("Loading Messenger::Server modules");
            this.Log("Creating Server instance...");
            RunningServer = new Server(this, this.GetPort());
            this.Log("Server instance {0} on port {1}", RunningServer, RunningServer.Port);
            this.Log("Log location: {0}", _logger.LogDirectory + "\\" + _logger.LogFile);
            new Thread(FinishLoadingThread).Start();
            RunningServer.Start();
            _commands = new CommandRegistry<ServerCommand>();
            new ServerCommands(RunningServer, _commands);
        }

        /// <summary>
        /// Gets a port number for the server to operate on.
        /// </summary>
        /// <returns>An integer port number.</returns>
        public int GetPort()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Contains("--port") || args.Contains("-p"))
            {
                int portIndex = (Array.IndexOf(args, "--port") == -1 ? Array.IndexOf(args, "-p") 
                    : Array.IndexOf(args, "--port")) + 1;
                if (portIndex > args.Length || portIndex == 0)
                {
                    return 25958;
                }
                else
                {
                    try
                    {
                        return int.Parse(args[portIndex]);
                    }
                    catch (Exception)
                    {
                        return 25958;
                    }
                }
            }
            return 25958;
        }

        /// <summary>
        /// Logs a message to the server window's output console.
        /// </summary>
        /// <param name="message">The message to log, optionally with formatting tags.</param>
        /// <param name="args">The arguments to insert into any formatting tags.</param>
        public void Log(string message, params object[] args)
        {
            this.Dispatcher.BeginInvoke(new Action(delegate()
            {
                StackFrame callerFrame = new StackFrame(1);
                MethodBase callerMethod = callerFrame.GetMethod();
                string[] splitFullTypeName = callerMethod.DeclaringType.Name.Split('.');
                string simpleTypeName = splitFullTypeName[splitFullTypeName.Count() - 1];
                string callerName = simpleTypeName + "." + callerMethod.Name;
                string messageText = string.Format("[{0}] {1}", DateTime.Now.ToLongTimeString(), string.Format(message, args));
                ConsoleBox.Text += messageText + "\n";
                _logger.LogWithoutCallerInfo("[{0}] {1}", callerName, string.Format(message, args));
            }));
        }

        private void FinishLoadingThread()
        {
            foreach (Type type in typeof(ServerWindow).GetTypesInSameNamespace())
            {
                this.Dispatcher.BeginInvoke(new Action(delegate()
                {
                    CurrentLoad.Content = "load " + type.FullName;
                }));
                Thread.Sleep(150);
            }
            this.Dispatcher.BeginInvoke(new Action(delegate()
            {
                CurrentLoad.Content = "load complete";
                Thread.Sleep(500);

                Storyboard storyboard = new Storyboard();

                DoubleAnimation animation = new DoubleAnimation();
                animation.From = 1.0;
                animation.To = 0.0;
                animation.Duration = new Duration(TimeSpan.FromMilliseconds(300));

                storyboard.Children.Add(animation);

                Storyboard.SetTarget(animation, Overlay);
                Storyboard.SetTargetProperty(animation, new PropertyPath(Canvas.OpacityProperty));

                storyboard.Begin();

                new Thread(new ThreadStart(() =>
                {
                    Thread.Sleep(300);
                    this.Dispatcher.BeginInvoke(new Action(delegate()
                    {
                        Master.Children.Remove(Overlay);
                    }));
                })).Start();
            }));
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            this.Log("Start sending shutdown signals...");
            RunningServer.SendShutdown();
            this.Log("Shutdown signal sending complete. Shutting down.");
            Thread.Sleep(100);
        }

        private void CommandInput_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Key key = e.Key;
            if (key == Key.Enter)
            {
                string command = CommandInput.Text;
                List<string> argsWithCommand = command.Split(' ').ToList<string>();
                if (argsWithCommand.Count() > 0)
                {
                    string commandName = argsWithCommand[0];
                    argsWithCommand.RemoveAt(0);
                    string[] args = argsWithCommand.ToArray();
                    this.Log("[" + commandName + "] " + _commands.Execute(commandName, args));
                }
                CommandInput.Text = "";
            }
        }
    }
}
