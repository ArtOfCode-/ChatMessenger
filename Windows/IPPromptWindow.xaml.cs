using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EPQMessenger.Windows
{
    /// <summary>
    /// Interaction logic for IPPromptWindow.xaml
    /// </summary>
    public partial class IPPromptWindow : Window
    {
        public string IPAddress;

        public int Port;

        private bool _systemClosed = false;

        public IPPromptWindow(bool failed)
        {
            InitializeComponent();
            if (failed)
            {
                FailLabel.Content = "Could not connect";
            }
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            IPAddress = IPInput.Text;
            int.TryParse(PortInput.Text, out Port);
            _systemClosed = true;
            this.Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (!_systemClosed)
            {
                Environment.Exit(0);
            }
        }
    }
}
