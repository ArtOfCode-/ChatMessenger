using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using EPQMessenger.Windows;

namespace EPQMessenger.States
{
    class NewMessageState
    {
        public string Message { get; private set; }

        public string Name { get; private set; }

        public Color NameColor { get; private set; }

        public ClientWindow Window { get; private set; }

        public NewMessageState(string message, string name, Color nameColor, ClientWindow window)
        {
            Message = message;
            Name = name;
            NameColor = nameColor;
            Window = window;
        }
    }
}
