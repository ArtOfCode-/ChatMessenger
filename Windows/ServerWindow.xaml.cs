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
        private Server _server;

        private Logger _logger;

        /// <summary>
        /// Initialises a new instance of the ServerWindow class and sets up the required values and
        /// objects for use.
        /// </summary>
        public ServerWindow()
        {
            InitializeComponent();
            _logger = new Logger();
            _logger.IsEnabled = true;
            Overlay.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#44FFFFFF"));
            this.Log("Loading Messenger::Server modules");
            this.Log("Creating Server instance...");
            _server = new Server(this, 25958);
            this.Log("Server instance {0} on port {1}", _server, _server.Port);
            this.Log("Log location: {0}", _logger.LogDirectory + "\\" + _logger.LogFile);
            new Thread(FinishLoadingThread).Start();
            _server.Start();
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
            e.Cancel = true;
            this.Log("Closing event canceled. Start sending shutdown signals...");
            _server.SendShutdown();
            this.Log("Shutdown signal sending complete. Shutting down.");
            Environment.Exit(0);
        }
    }
}
