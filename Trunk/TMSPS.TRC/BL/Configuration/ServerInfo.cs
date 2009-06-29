namespace TMSPS.TRC.BL.Configuration
{
    public class ServerInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public ushort XmlRpcPort { get; set; }
        public string Address { get; set; }
        public string SuperAdminPassword { get; set; }
    }
}