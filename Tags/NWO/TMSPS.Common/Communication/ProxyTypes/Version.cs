using TMSPS.Core.Common;
using TMSPS.Core.Communication.ResponseHandling;

namespace TMSPS.Core.Communication.ProxyTypes
{
    public class Version : IDump
    {
        [RPCParam("Name")]
        public string Name { get; set; }

        [RPCParam("Version")]
        public string VersionString { get; set; }

        [RPCParam("Build")]
        public string Build { get; set; }

        public string GetShortName()
        {
            string name = Name == null ? null : Name.ToUpper();

            switch (name)
            {
                case "TMFOREVER": return "TMF";
                case "TMNATIONSESWC": return "TMN";
                case "TMSUNRISE": return "TMS";
                case "TMORIGINAL": return "TMO";
                default: return "---";
            }
        }

        public Version Clone()
        {
            return new Version{Build = Build, Name =  Name, VersionString = VersionString};
        }
    }
}