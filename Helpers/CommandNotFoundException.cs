using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPQMessenger.Helpers
{
    class CommandNotFoundException : Exception
    {
        public CommandNotFoundException() : base() { }

        public CommandNotFoundException(string message) : base(message) { }

        public CommandNotFoundException(string message, params string[] args) : base(string.Format(message, args)) { }

        public CommandNotFoundException(string message, Exception inner) : base(message, inner) { }
    }
}
