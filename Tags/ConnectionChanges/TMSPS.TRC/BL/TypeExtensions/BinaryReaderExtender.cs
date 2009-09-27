using System;
using System.IO;

namespace TMSPS.TRC.BL.TypeExtensions
{
    public static class BinaryReaderExtender
    {
        public static string ReadSringSafe(this BinaryReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            try
            {
                return reader.ReadString();
            }
            catch (EndOfStreamException)
            {
                return null;
            }
        }
    }
}