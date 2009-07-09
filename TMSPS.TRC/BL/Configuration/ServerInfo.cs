using System;
using System.ComponentModel;
using System.IO;
using TMSPS.Core.Common;

namespace TMSPS.TRC.BL.Configuration
{
    [Serializable]
    public class ServerInfo : NotifyPropertyChanged, IDataErrorInfo
    {
        #region Fields

        #pragma warning disable 649
        private string _name;
        private string _description;
        private ushort _xmlRpcPort;
        private string _address;
        private string _superAdminPassword;
        #pragma warning restore 649

        #endregion

        #region Properties

        public string Name
        {
            get { return _name; }
            set { SetProperty(() => Name, () => _name, value); }
        }

        public string Description
        {
            get { return _description; }
            set { SetProperty(() => Description, () => _description, value); }
        }

        public ushort XmlRpcPort
        {
            get { return _xmlRpcPort; }
            set { SetProperty(() => XmlRpcPort, () => _xmlRpcPort, value); }
        }

        public string Address
        {
            get { return _address; }
            set { SetProperty(() => Address, () => _address, value); }
        }

        public string SuperAdminPassword
        {
            get { return _superAdminPassword; }
            set { SetProperty(() => SuperAdminPassword, () => _superAdminPassword, value); }
        }

        #endregion

        #region Public Methods

        public void CopyFrom(ServerInfo serverInfo)
        {
            if (serverInfo == null)
                return;

            Name = serverInfo.Name;
            Description = serverInfo.Description;
            XmlRpcPort = serverInfo.XmlRpcPort;
            Address = serverInfo.Address;
            SuperAdminPassword = serverInfo.SuperAdminPassword;
        }

        public ServerInfo Clone()
        {
            return new ServerInfo
                       {
                           Name    = Name,
                           Description = Description,
                           XmlRpcPort = XmlRpcPort,
                           Address = Address,
                           SuperAdminPassword = SuperAdminPassword
                       };
        }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is ServerInfo))
                return false;

            ServerInfo other = (ServerInfo) obj;

            return (string.Compare(Name, other.Name, StringComparison.Ordinal) == 0) &&
                   (string.Compare(Description, other.Description, StringComparison.Ordinal) == 0) &&
                   (string.Compare(Address, other.Address, StringComparison.Ordinal) == 0) &&
                   (string.Compare(SuperAdminPassword, other.SuperAdminPassword, StringComparison.Ordinal) == 0) &&
                   XmlRpcPort == other.XmlRpcPort;
        }

        public void WriteToBinaryWriter(BinaryWriter writer)
        {
            writer.Write(Name);
            writer.Write(Description);
            writer.Write(XmlRpcPort);
            writer.Write(Address);
            writer.Write(SuperAdminPassword);
        }

        public static ServerInfo ReadFromBinaryReader(BinaryReader reader)
        {
            return new ServerInfo
            {
                Name = reader.ReadString(),
                Description = reader.ReadString(),
                XmlRpcPort = reader.ReadUInt16(),
                Address = reader.ReadString(),
                SuperAdminPassword = reader.ReadString()
            };
        }

        #endregion

        #region IDataErrorInfo Members

        string IDataErrorInfo.Error
        {
            get { return null; }
        }

        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                if (columnName == "Name" && Name.IsNullOrTimmedEmpty())
                    return  "Please provide a non empty name.";

                if (columnName == "XmlRpcPort" && XmlRpcPort == 0)
                    return "Please provide a port that is between 1 and 65535.";

                if (columnName == "Address" && Address.IsNullOrTimmedEmpty())
                    return "Please provide a non empty address.";

                if (columnName == "SuperAdminPassword" && SuperAdminPassword.IsNullOrTimmedEmpty())
                    return "Please provide a non empty super admin password.";

                return null;
            }
        }

        #endregion
    }
}