using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using EPQMessenger.Windows;
using EPQMessenger.Workers;

[assembly:CLSCompliant(true)]
namespace EPQMessenger
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public OperationMode OperatingMode { get; private set; }

        public static bool StopAllThreads { get; set; }

        public static Logger LogDevice { get; private set; }

        [STAThread]
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            StopAllThreads = false;

            LogDevice = new Logger();

            this.ParseCommandLineArgs();

            if (OperatingMode == OperationMode.Server)
            {
                ServerWindow window = new ServerWindow();
                window.Show();
                window.Closed += (sender, ea) =>
                {
                    Environment.Exit(0);
                };
            }
            else if (OperatingMode == OperationMode.Client)
            {
                ClientWindow window = new ClientWindow();
                window.Show();
                window.Closed += (sender, ea) =>
                {
                    Environment.Exit(0);
                };
            }
        }

        private void ParseCommandLineArgs()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Contains("-s") || args.Contains("--server"))
            {
                this.SetOperationMode(OperationMode.Server);
            }
            else
            {
                this.SetOperationMode(OperationMode.Client);
            }
            if (args.Contains("-d") || args.Contains("--disable-logging"))
            {
                LogDevice.IsEnabled = false;
            }
            else
            {
                LogDevice.IsEnabled = true;
            }
        }

        private void SetOperationMode(OperationMode mode)
        {
            OperatingMode = mode;
        }
    }
}
