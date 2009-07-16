namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords
{
    public class TopRankingEntry
    {
        public uint Position { get; set; }
        public string Login { get; set; }
        public string Nickname { get; set; }
        public uint FirstRecords { get; set; }
        public uint SecondRecords { get; set; }
        public uint ThirdRecords { get; set; }
    }
}