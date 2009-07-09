using System;
using System.IO;
using System.Text;
using TMSPS.TRC.BL.Common;

namespace TMSPS.TRC.BL.Configuration
{
    public class Profile : ProfileInfo
    {
        public ServerInfoList Servers { get; protected set; }
        public string Password { get; set; }

        public Profile()
        {
            Servers = new ServerInfoList();
        }

        public override void Save()
        {
            Save(writer => SaveServerList(writer, Password));
        }

        public static Profile ReadFromFile(string filePath, string password)
        {
            if (filePath.IsNullOrTimmedEmpty())
                throw new ArgumentException("filePath is null or empty.");

            using (FileStream fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (BinaryReader reader = new BinaryReader(fileStream, Encoding.UTF8))
                {
                    Profile result = new Profile { FilePath = filePath };
                    result.ReadFromReader(reader);

                    if (result.PasswordHash != password.ToHash())
                        return null;

                    if (reader.PeekChar() == -1)
                        return result;

                    int serverListBytesCount = reader.ReadInt32();

                    if (serverListBytesCount > 0)
                    {
                        byte[] serverListData = reader.ReadBytes(serverListBytesCount);
                        result.LoadServerListData(serverListData, password);
                    }
                    else
                    {
                        result.Servers = new ServerInfoList();
                    }

                    return result;
                }
            }
        }

        protected void LoadServerListData(byte[] encryptedServerListData, string password)
        {
            using (MemoryStream stream = new MemoryStream(CryptoHelper.Decrypt(encryptedServerListData, password)))
            {
                using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8))
                {
                    Servers = ServerInfoList.ReadFromBinaryReader(reader);    
                }
            }
        }

        protected void SaveServerList(BinaryWriter outerWriter,  string password)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter innerWriter = new BinaryWriter(stream, Encoding.UTF8))
                {
                    Servers.WriteToBinaryWriter(innerWriter);
                    stream.Seek(0, SeekOrigin.Begin);

                    byte[] bytes = CryptoHelper.Encrypt(stream.GetBuffer(), password);
                    outerWriter.Write(bytes.Length);
                    outerWriter.Write(bytes);
                }
            }
        }

        public Profile CloneProfile()
        {
            return new Profile{FilePath = FilePath, Name = Name, Password = Password, PasswordHash = PasswordHash, Servers = Servers.Clone()};
        }
    }
}