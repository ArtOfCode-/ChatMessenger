using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPQMessenger.Helpers
{
    /// <summary>
    /// Represents a command intended for execution server-side.
    /// </summary>
    class ServerCommand : ICommand
    {
        /// <summary>
        /// The ICommand-derived name of the command.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The help text for this command. Derived from ICommand.
        /// </summary>
        public string HelpText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the command requires permissions to run.
        /// </summary>
        public bool IsPrivileged { get; set; }

        private Func<string[], string> _commandMethod;

        /// <summary>
        /// Initialises a new instance of the ServerCommand class.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        /// <param name="action">The method called when the command is executed.</param>
        public ServerCommand(string name, bool isPrivileged, Func<string[], string> action, string help)
        {
            Name = name;
            _commandMethod = action;
            HelpText = help;
        }

        /// <summary>
        /// Executes the command represented by this instance, by calling its method.
        /// </summary>
        /// <param name="args">The arguments to pass to the command.</param>
        /// <returns>The string that the command returns.</returns>
        public string Execute(string[] args)
        {
            return _commandMethod.Invoke(args);
        }
    }
}
