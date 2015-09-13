using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Diagnostics;
using System.Reflection;
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
        /// <summary>
        /// The mode the software is operating in. Server or Client.
        /// </summary>
        public OperationMode OperatingMode { get; private set; }

        /// <summary>
        /// Indicates that active threads should stop running to prepare for shutdown.
        /// Should be polled as part of a thread's while loop.
        /// </summary>
        public static bool StopAllThreads { get; set; }

        /// <summary>
        /// The Logger used to write system logs.
        /// </summary>
        public static Logger LogDevice { get; private set; }

        /// <summary>
        /// Provides a list of banned users.
        /// </summary>
        public static List<string> BannedUsers
        {
            get
            {
                return _bannedUsersInternal;
            }
            set
            {
                _bannedUsersInternal = value;
                SaveData(_bannedUsersInternal, "banned_users.dta");
            }
        }
        private static List<string> _bannedUsersInternal = new List<string>();

        public static List<string> PrivilegedUsers
        {
            get
            {
                return _privilegedUsersInternal;
            }
            set
            {
                _privilegedUsersInternal = value;
                SaveData(_privilegedUsersInternal, "privileged_users.dta");
            }
        }
        private static List<string> _privilegedUsersInternal = new List<string>();

        /// <summary>
        /// Represents the application's main window.
        /// </summary>
        public static Window ActiveWindow { get; set; }

        [STAThread]
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            StopAllThreads = false;

            LogDevice = new Logger();

            this.ParseCommandLineArgs();
            BannedUsers = this.LoadData("banned_users.dta");
            PrivilegedUsers = this.LoadData("privileged_users.dta");

            if (OperatingMode == OperationMode.Server)
            {
                string[] args = Environment.GetCommandLineArgs();
                ServerWindow window = new ServerWindow(args.Contains("-c") || args.Contains("--crash-recovery"));
                ActiveWindow = window;
                window.Show();
                window.Closed += (sender, ea) =>
                {
                    Environment.Exit(0);
                };
            }
            else if (OperatingMode == OperationMode.Client)
            {
                ClientWindow window = new ClientWindow();
                ActiveWindow = window;
                window.Show();
                window.Closed += (sender, ea) =>
                {
                    Environment.Exit(0);
                };
            }
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogDevice.Log("[APPDOMAIN] Unhandled exception occured during execution. Details follow.");
            LogDevice.Log("[APPDOMAIN] Message: {0}", ((Exception)e.ExceptionObject).Message);
            LogDevice.Log("[APPDOMAIN] Stack trace: {0}", ((Exception)e.ExceptionObject).StackTrace);
            LogDevice.Log("[APPDOMAIN] Application is being restarted automatically under crash recovery.");
            if (OperatingMode == OperationMode.Server)
            {
                ServerWindow window = (ServerWindow)ActiveWindow;
                window.RunningServer.StopServer("it encountered an error");
            }
            Process.Start(Assembly.GetExecutingAssembly().CodeBase, string.Join(" ", Environment.GetCommandLineArgs()));
        }

        private void ParseCommandLineArgs()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Contains("-c") || args.Contains("--crash-recovery"))
            {
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            }
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

        private List<string> LoadData(string filename)
        {
            string dataDirectory = Path.Combine(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath),
                "data");
            if (!Directory.Exists(dataDirectory))
            {
                Directory.CreateDirectory(dataDirectory);
            }
            if (!File.Exists(Path.Combine(dataDirectory, filename)))
            {
                File.Create(Path.Combine(dataDirectory, filename));
            }
            return File.ReadAllLines(Path.Combine(dataDirectory, filename)).ToList<string>();
        }

        public static void SaveData(List<string> data, string filename)
        {
            File.WriteAllLines(Path.Combine(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath),
                "data", filename), data.ToArray<string>());
        }
    }
}
