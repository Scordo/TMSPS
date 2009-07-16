using System;
using System.Runtime.Serialization;

namespace TMSPS.TRC.BL.Exceptions
{
    [Serializable]
    public class InvalidFileFormatException : Exception
    {
        #region Properties

        public string Filename { get; set; }

        #endregion

        #region Constructors

        public InvalidFileFormatException()
        {

        }

        public InvalidFileFormatException(string message) : this(message, (string) null)
        {

        }

        public InvalidFileFormatException(string message, string filename) : base(message)
        {
            Filename = filename;
        }

        public InvalidFileFormatException(string message, Exception inner) : this(message, null, inner)
        {

        }

        public InvalidFileFormatException(string message, string filename, Exception inner) : base(message, inner)
        {
            Filename = filename;
        }

        protected InvalidFileFormatException(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }

        #endregion
    }
}