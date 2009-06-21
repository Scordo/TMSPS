using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TMSPS.Core.Common;
using TMSPS.Core.PluginSystem.Configuration;

namespace TMSPS.UnitTests.PluginSystem.Configuration
{
    [TestClass]
    public class CredentialsReaderUTC
    {
        [TestMethod]
        public void Test1()
        {
            Dictionary<string, HashSet<string>> credentials = CredentialsReader.ReadCredentials(CredentialsReaderUTCSampleData.Sample1, false);

            Assert.IsTrue(credentials.ContainsKey("user1"));
            Assert.IsTrue(credentials.ContainsKey("user2"));
            Assert.IsTrue(credentials.ContainsKey("user3"));
            Assert.IsTrue(credentials.ContainsKey("user4"));
            Assert.IsTrue(credentials.ContainsKey("user5"));

            Assert.IsTrue(credentials["user1"].Contains("right1"));
            Assert.IsTrue(credentials["user1"].Contains("right2"));
            Assert.IsTrue(credentials["user1"].Contains("right3"));
            Assert.IsTrue(credentials["user1"].Contains("right4"));

            Assert.IsTrue(credentials["user2"].Contains("right1"));
            Assert.IsTrue(credentials["user2"].Contains("right2"));
            Assert.IsTrue(credentials["user2"].Contains("right3"));
            Assert.IsTrue(credentials["user2"].Contains("right4"));
            Assert.IsTrue(credentials["user2"].Contains("right22"));

            Assert.IsTrue(credentials["user3"].Contains("right1"));
            Assert.IsTrue(credentials["user3"].Contains("right2"));
            Assert.IsTrue(credentials["user3"].Contains("right3"));
            Assert.IsTrue(credentials["user3"].Contains("right4"));

            Assert.IsTrue(credentials["user4"].Contains("right2"));
            Assert.IsTrue(credentials["user4"].Contains("right3"));
            Assert.IsTrue(credentials["user4"].Contains("right5"));
            Assert.IsTrue(credentials["user4"].Contains("right7"));

            Assert.IsTrue(credentials["user5"].Contains("right17"));
            Assert.IsTrue(credentials["user5"].Contains("right19"));
            Assert.IsTrue(credentials["user5"].Contains("right20"));
        }
    }
}
