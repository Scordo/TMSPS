using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using TMSPS.TRC.BL.Common;

namespace TMSPS.TRC.BL.Configuration
{
    public class Profile : ProfileInfo
    {
        public ServerInfoList Servers { get; protected set; }
        public string Password { get; set; }

        public override void Save()
        {
            base.Save();
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

                    uint serverListBytesCount = reader.ReadUInt32();

                    if (serverListBytesCount > 0)
                    {
                        byte[] serverListData = reader.ReadBytes((int)serverListBytesCount);
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
                BinaryFormatter formatter = new BinaryFormatter();
                Servers = (ServerInfoList) formatter.Deserialize(stream);
            }
        }
    }
}
