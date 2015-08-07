using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace EPQMessenger.Helpers
{
    /// <summary>
    /// The exception thrown when a given Protocol status code
    /// is invalid for the current situation.
    /// </summary>
    [Serializable]
    public class InvalidStatusException : Exception
    {
        /// <summary>
        /// The Protocol status code that was given.
        /// </summary>
        public int AttemptedCode { get; private set; }

        /// <summary>
        /// Initialises a new instance of the InvalidStausException class.
        /// </summary>
        /// <param name="code">The Protocol status code that was given.</param>
        public InvalidStatusException(int code)
            : base()
        {
            AttemptedCode = code;
        }

        /// <summary>
        /// Initialises a new instance of the InvalidStausException class.
        /// </summary>
        /// <param name="code">The Protocol status code that was given.</param>
        /// <param name="message">The exception message.</param>
        public InvalidStatusException(int code, string message)
            : base(message)
        {
            AttemptedCode = code;
        }

        /// <summary>
        /// Initialises a new instance of the InvalidStausException class.
        /// </summary>
        /// <param name="code">The Protocol status code that was given.</param>
        /// <param name="message">The exception message.</param>
        /// <param name="inner">The inner exception that caused this one.</param>
        public InvalidStatusException(int code, string message, Exception inner)
            : base(message, inner)
        {
            AttemptedCode = code;
        }

        protected InvalidStatusException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }
    }
}
