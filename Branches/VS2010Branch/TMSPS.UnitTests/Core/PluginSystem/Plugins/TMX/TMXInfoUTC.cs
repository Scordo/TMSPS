using Microsoft.VisualStudio.TestTools.UnitTesting;
using TMSPS.Core.PluginSystem.Plugins.TMX;

namespace TMSPS.UnitTests.Core.PluginSystem.Plugins.TMX
{
    /// <summary>
    /// Summary description for TMXInfoUTC
    /// </summary>
    [TestClass]
    public class TMXInfoUTC
    {
        [TestMethod]
        public void ParseUTM()
        {
            const string tmxinfostring = "972145	^^drumon^^	917699	Yukima	2009-03-04 18:59:49	2009-03-04 18:59:49	True	Race	Stadium	Sunset	Stunt	Multi	30s	Intermediate	1000	TM Nations Forever	Have fun^^	";

            TMXInfo info = TMXInfo.Parse(tmxinfostring);

            Assert.IsNotNull(info);
            Assert.IsFalse(info.Erroneous);
        }

        [TestMethod]
        public void RetrieveUTM()
        {
            TMXInfo info = TMXInfo.Retrieve("972145");

            Assert.IsNotNull(info);
            Assert.IsFalse(info.Erroneous);

            info = TMXInfo.Retrieve("False ID");
            Assert.IsNotNull(info);
            Assert.IsTrue(info.Erroneous);
        }

        [TestMethod]
        public void DownloadTrackUTM()
        {
            byte[] data = TMXInfo.DownloadTrack("972145");

            Assert.IsNotNull(data);
            Assert.AreEqual(7834, data.Length);
        }
    }
}
