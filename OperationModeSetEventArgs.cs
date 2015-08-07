using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPQMessenger
{
    /// <summary>
    /// Event arguments to be used in the OperationModeSet event.
    /// </summary>
    public class OperationModeSetEventArgs : EventArgs
    {
        /// <summary>
        /// The OperationMode to which the program has been set.
        /// </summary>
        public OperationMode Mode { get; private set; }

        /// <summary>
        /// Initialises a new instance of the OperationModeSetEventArgs class.
        /// </summary>
        /// <param name="mode">The OperationMode which has been set.</param>
        public OperationModeSetEventArgs(OperationMode mode)
        {
            Mode = mode;
        }
    }
}
