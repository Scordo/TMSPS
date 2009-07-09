using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace TMSPS.TRC.BL.Configuration
{
    [Serializable]
    public class ServerInfoList : ObservableCollection<ServerInfo>
    {
        #region Constructors

        public ServerInfoList()
        {
            
        }

        public ServerInfoList(IEnumerable<ServerInfo> serverInfoList)
        {
            AddRange(serverInfoList);
        }

        #endregion

        #region Public Methods

        public ServerInfoList Clone()
        {
            ServerInfoList result = new ServerInfoList();

            foreach (ServerInfo serverInfo in this)
            {
                result.Add(serverInfo.Clone());
            }

            return result;
        }

        public void AddRange(IEnumerable<ServerInfo> serverInfoList)
        {
            if (serverInfoList == null)
                return;

            foreach (ServerInfo serverInfo in serverInfoList)
            {
                Add(serverInfo);
            }
        }

        public void WriteToBinaryWriter(BinaryWriter writer)
        {
            writer.Write(Count);

            foreach (ServerInfo serverInfo in this)
            {
                serverInfo.WriteToBinaryWriter(writer);
            }
        }

        public static ServerInfoList ReadFromBinaryReader(BinaryReader reader)
        {
            ServerInfoList result = new ServerInfoList();
            int count = reader.ReadInt32();

            for (int i = 0; i < count; i++)
            {
                result.Add(ServerInfo.ReadFromBinaryReader(reader));
            }

            return result;
        }

        #endregion
    }
}