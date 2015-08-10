using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using EPQMessenger.Workers;

namespace EPQMessenger.Windows
{
    /// <summary>
    /// Interaction logic for ClientWindow.xaml
    /// </summary>
    public partial class ClientWindow : Window
    {
        private Client _client;

        /// <summary>
        /// Initialises a new instance of the ClientWindow class.
        /// </summary>
        public ClientWindow()
        {
            InitializeComponent();
            _client = new Client(this);
            this.Connect(false);
        }

        /// <summary>
        /// Adds a message to the displayed conversation. Handles status changes itself.
        /// </summary>
        /// <param name="message">The message text to add.</param>
        /// <param name="name">The user the message is from.</param>
        /// <param name="nameColor">The color in which to display the username.</param>
        public void AddMessage(string message, string name, Color nameColor)
        {
            Console.WriteLine("[ClientWindow.AddMessage] Call note");
            this.ChangeStatus("Receiving", Color.FromRgb(204, 81, 0));
            Grid fullMessage = new Grid();
            fullMessage.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70.00) });
            fullMessage.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(600.00) });

            Label nameLabel = new Label();
            nameLabel.Content = string.Format("[{0}]", name);
            nameLabel.Foreground = new SolidColorBrush(nameColor);
            nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
            if (name == "Server")
            {
                nameLabel.FontStyle = FontStyles.Italic;
            }

            Grid.SetColumn(nameLabel, 0);

            TextBlock textLabel = new TextBlock();
            textLabel.Text = message;
            textLabel.TextWrapping = TextWrapping.Wrap;
            textLabel.Margin = new Thickness(0, 5, 0, 5);
            if (name == "Server")
            {
                textLabel.FontStyle = FontStyles.Italic;
            }

            Grid.SetColumn(textLabel, 1);

            fullMessage.Children.Add(nameLabel);
            fullMessage.Children.Add(textLabel);

            Messages.Children.Add(fullMessage);

            this.ResetStatus();

            Console.WriteLine("[ClientWindow.AddMessage] Method end");
        }

        private void Connect(bool failed)
        {
            IPPromptWindow ipPrompt = new IPPromptWindow(failed);
            ipPrompt.ShowDialog();
            string address = ipPrompt.IPAddress;
            int port = ipPrompt.Port;
            if (!_client.Connect(address, port))
            {
                this.Connect(true);
            }
        }

        /// <summary>
        /// Changes the status message displayed in the status bar, and the color of the bar.
        /// </summary>
        /// <param name="statusMessage">The new message to show.</param>
        /// <param name="barColor">The color to change the background to.</param>
        public void ChangeStatus(string statusMessage, Color barColor)
        {
            this.Dispatcher.BeginInvoke(new Action(delegate()
            {
                ReadyState.Content = statusMessage;
                Status.Background = new SolidColorBrush(barColor);
            }));
        }

        /// <summary>
        /// Resets the displayed status bar to its original values.
        /// </summary>
        public void ResetStatus()
        {
            this.Dispatcher.BeginInvoke(new Action(delegate()
            {
                ReadyState.Content = "Ready";
                Status.Background = new SolidColorBrush(Colors.DodgerBlue);
            }));
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string message = Protocol.GetResponseFromCode(302) + "\n" + MessageInput.Text;
            _client.Send(message);
            MessageInput.Text = "";
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _client.Close();
            App.StopAllThreads = true;
        }
    }
}
