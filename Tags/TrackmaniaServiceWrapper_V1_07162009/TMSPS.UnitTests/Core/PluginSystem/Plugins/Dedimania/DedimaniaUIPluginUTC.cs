using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TMSPS.Core.PluginSystem.Plugins.Dedimania;

namespace TMSPS.UnitTests.Core.PluginSystem.Plugins.Dedimania
{
    /// <summary>
    /// Summary description for DedimaniaUIPluginUTC
    /// </summary>
    [TestClass]
    public class DedimaniaUIPluginUTC
    {
        [TestMethod]
        public void Show3WithMax30Pos1()
        {
            const uint maxRecordsToReport = 30;
            DedimaniaRanking[] rankings = GetRankings(maxRecordsToReport, "scordo", 1);
            SortedList<uint, DedimaniaRanking> rankingsToShow = DedimaniaUIPlugin.GetRankingsToShow(rankings, "scordo", 3, 30);

            Assert.AreEqual(3, rankingsToShow.Count);
            Assert.AreEqual("scordo", rankingsToShow[1].Login);
            Assert.AreEqual("scordo2", rankingsToShow[2].Login);
            Assert.AreEqual("scordo3", rankingsToShow[3].Login);
        }

        [TestMethod]
        public void Show3WithMax30Pos2()
        {
            const uint maxRecordsToReport = 30;
            DedimaniaRanking[] rankings = GetRankings(maxRecordsToReport, "scordo", 2);
            SortedList<uint, DedimaniaRanking> rankingsToShow = DedimaniaUIPlugin.GetRankingsToShow(rankings, "scordo", 3, 30);

            Assert.AreEqual(3, rankingsToShow.Count);
            Assert.AreEqual("scordo1", rankingsToShow[1].Login);
            Assert.AreEqual("scordo", rankingsToShow[2].Login);
            Assert.AreEqual("scordo3", rankingsToShow[3].Login);
        }

        [TestMethod]
        public void Show3WithMax30Pos3()
        {
            const uint maxRecordsToReport = 30;
            DedimaniaRanking[] rankings = GetRankings(maxRecordsToReport, "scordo", 3);
            SortedList<uint, DedimaniaRanking> rankingsToShow = DedimaniaUIPlugin.GetRankingsToShow(rankings, "scordo", 3, 30);

            Assert.AreEqual(3, rankingsToShow.Count);
            Assert.AreEqual("scordo1", rankingsToShow[1].Login);
            Assert.AreEqual("scordo2", rankingsToShow[2].Login);
            Assert.AreEqual("scordo", rankingsToShow[3].Login);
        }

        [TestMethod]
        public void Show3WithMax0()
        {
            DedimaniaRanking[] rankings = GetRankings(0, "scordo", null);
            SortedList<uint, DedimaniaRanking> rankingsToShow = DedimaniaUIPlugin.GetRankingsToShow(rankings, "scordo", 3, 30);

            Assert.AreEqual(3, rankingsToShow.Count);
        }

        [TestMethod]
        public void Show25WithMax30Pos3()
        {
            int? position = 3;
            const uint recordsToShow = 25;
            const uint maxRecordsToReport = 30;

            DedimaniaRanking[] rankings = GetRankings(maxRecordsToReport, "scordo", position);
            SortedList<uint, DedimaniaRanking> rankingsToShow = DedimaniaUIPlugin.GetRankingsToShow(rankings, "scordo", recordsToShow, maxRecordsToReport);

            Assert.AreEqual(recordsToShow, (uint)rankingsToShow.Count);
            Assert.AreEqual("scordo", rankingsToShow[(uint)position.Value].Login);

            for (uint i = 1; i < maxRecordsToReport; i++)
            {
                if (i < 15)
                    Assert.IsTrue(rankingsToShow.ContainsKey(i));
                else if (i < 20)
                    Assert.IsFalse(rankingsToShow.ContainsKey(i));
                else
                    Assert.IsTrue(rankingsToShow.ContainsKey(i));
            }
        }

        [TestMethod]
        public void Show25WithMax30Pos4()
        {
            int? position = 4;
            const uint recordsToShow = 25;
            const uint maxRecordsToReport = 30;

            DedimaniaRanking[] rankings = GetRankings(maxRecordsToReport, "scordo", position);
            SortedList<uint, DedimaniaRanking> rankingsToShow = DedimaniaUIPlugin.GetRankingsToShow(rankings, "scordo", recordsToShow, maxRecordsToReport);

            Assert.AreEqual(recordsToShow, (uint)rankingsToShow.Count);
            Assert.AreEqual("scordo", rankingsToShow[(uint)position.Value].Login);

            for (uint i = 1; i < maxRecordsToReport; i++)
            {
                if (i < 15)
                    Assert.IsTrue(rankingsToShow.ContainsKey(i));
                else if (i < 20)
                    Assert.IsFalse(rankingsToShow.ContainsKey(i));
                else
                    Assert.IsTrue(rankingsToShow.ContainsKey(i));
            }
        }

        [TestMethod]
        public void Show25WithMax30Pos6()
        {
            int? position = 6;
            const uint recordsToShow = 25;
            const uint maxRecordsToReport = 30;

            DedimaniaRanking[] rankings = GetRankings(maxRecordsToReport, "scordo", position);
            SortedList<uint, DedimaniaRanking> rankingsToShow = DedimaniaUIPlugin.GetRankingsToShow(rankings, "scordo", recordsToShow, maxRecordsToReport);

            Assert.AreEqual(recordsToShow, (uint)rankingsToShow.Count);
            Assert.AreEqual("scordo", rankingsToShow[(uint)position.Value].Login);

            for (uint i = 1; i < maxRecordsToReport; i++)
            {
                if (i < 16)
                    Assert.IsTrue(rankingsToShow.ContainsKey(i));
                else if (i < 21)
                    Assert.IsFalse(rankingsToShow.ContainsKey(i));
                else
                    Assert.IsTrue(rankingsToShow.ContainsKey(i));
            }
        }

        [TestMethod]
        public void Show25WithMax30Pos9()
        {
            int? position = 9;
            const uint recordsToShow = 25;
            const uint maxRecordsToReport = 30;

            DedimaniaRanking[] rankings = GetRankings(maxRecordsToReport, "scordo", position);
            SortedList<uint, DedimaniaRanking> rankingsToShow = DedimaniaUIPlugin.GetRankingsToShow(rankings, "scordo", recordsToShow, maxRecordsToReport);

            Assert.AreEqual(recordsToShow, (uint)rankingsToShow.Count);
            Assert.AreEqual("scordo", rankingsToShow[(uint)position.Value].Login);

            for (uint i = 1; i < maxRecordsToReport; i++)
            {
                if (i < 18)
                    Assert.IsTrue(rankingsToShow.ContainsKey(i));
                else if (i < 23)
                    Assert.IsFalse(rankingsToShow.ContainsKey(i));
                else
                    Assert.IsTrue(rankingsToShow.ContainsKey(i));
            }
        }

        [TestMethod]
        public void Show25WithMax30Pos12()
        {
            int? position = 12;
            const uint recordsToShow = 25;
            const uint maxRecordsToReport = 30;

            DedimaniaRanking[] rankings = GetRankings(maxRecordsToReport, "scordo", position);
            SortedList<uint, DedimaniaRanking> rankingsToShow = DedimaniaUIPlugin.GetRankingsToShow(rankings, "scordo", recordsToShow, maxRecordsToReport);

            Assert.AreEqual(recordsToShow, (uint)rankingsToShow.Count);
            Assert.AreEqual("scordo", rankingsToShow[(uint)position.Value].Login);

            for (uint i = 1; i < maxRecordsToReport; i++)
            {
                if (i < 19)
                    Assert.IsTrue(rankingsToShow.ContainsKey(i));
                else if (i < 24)
                    Assert.IsFalse(rankingsToShow.ContainsKey(i));
                else
                    Assert.IsTrue(rankingsToShow.ContainsKey(i));
            }
        }

        [TestMethod]
        public void Show25WithMax30Pos15()
        {
            int? position = 15;
            const uint recordsToShow = 25;
            const uint maxRecordsToReport = 30;

            DedimaniaRanking[] rankings = GetRankings(maxRecordsToReport, "scordo", position);
            SortedList<uint, DedimaniaRanking> rankingsToShow = DedimaniaUIPlugin.GetRankingsToShow(rankings, "scordo", recordsToShow, maxRecordsToReport);

            Assert.AreEqual(recordsToShow, (uint)rankingsToShow.Count);
            Assert.AreEqual("scordo", rankingsToShow[(uint)position.Value].Login);

            for (uint i = 1; i < maxRecordsToReport; i++)
            {
                if (i > 20 && i < 26)
                    Assert.IsFalse(rankingsToShow.ContainsKey(i));
                else
                    Assert.IsTrue(rankingsToShow.ContainsKey(i));
            }
        }

        [TestMethod]
        public void Show25WithMax30Pos18()
        {
            int? position = 18;
            const uint recordsToShow = 25;
            const uint maxRecordsToReport = 30;

            DedimaniaRanking[] rankings = GetRankings(maxRecordsToReport, "scordo", position);
            SortedList<uint, DedimaniaRanking> rankingsToShow = DedimaniaUIPlugin.GetRankingsToShow(rankings, "scordo", recordsToShow, maxRecordsToReport);

            Assert.AreEqual(recordsToShow, (uint)rankingsToShow.Count);
            Assert.AreEqual("scordo", rankingsToShow[(uint)position.Value].Login);

            for (uint i = 1; i < maxRecordsToReport; i++)
            {
                if (i < 9)
                    Assert.IsTrue(rankingsToShow.ContainsKey(i));
                else if (i < 12)
                    Assert.IsFalse(rankingsToShow.ContainsKey(i));
                else if (i < 24)
                    Assert.IsTrue(rankingsToShow.ContainsKey(i));
                else if (i < 26)
                    Assert.IsFalse(rankingsToShow.ContainsKey(i));
                else
                    Assert.IsTrue(rankingsToShow.ContainsKey(i));
            }
        }

        [TestMethod]
        public void Show25WithMax30Pos21()
        {
            int? position = 21;
            const uint recordsToShow = 25;
            const uint maxRecordsToReport = 30;

            DedimaniaRanking[] rankings = GetRankings(maxRecordsToReport, "scordo", position);
            SortedList<uint, DedimaniaRanking> rankingsToShow = DedimaniaUIPlugin.GetRankingsToShow(rankings, "scordo", recordsToShow, maxRecordsToReport);

            Assert.AreEqual(recordsToShow, (uint)rankingsToShow.Count);
            Assert.AreEqual("scordo", rankingsToShow[(uint)position.Value].Login);

            for (uint i = 1; i < maxRecordsToReport; i++)
            {
                if (i < 10)
                    Assert.IsTrue(rankingsToShow.ContainsKey(i));
                else if (i < 15)
                    Assert.IsFalse(rankingsToShow.ContainsKey(i));
                else
                    Assert.IsTrue(rankingsToShow.ContainsKey(i));
            }
        }

        [TestMethod]
        public void Show25WithMax30Pos24()
        {
            int? position = 24;
            const uint recordsToShow = 25;
            const uint maxRecordsToReport = 30;

            DedimaniaRanking[] rankings = GetRankings(maxRecordsToReport, "scordo", position);
            SortedList<uint, DedimaniaRanking> rankingsToShow = DedimaniaUIPlugin.GetRankingsToShow(rankings, "scordo", recordsToShow, maxRecordsToReport);

            Assert.AreEqual(recordsToShow, (uint)rankingsToShow.Count);
            Assert.AreEqual("scordo", rankingsToShow[(uint)position.Value].Login);

            for (uint i = 1; i < maxRecordsToReport; i++)
            {
                if (i < 11)
                    Assert.IsTrue(rankingsToShow.ContainsKey(i));
                else if (i < 16)
                    Assert.IsFalse(rankingsToShow.ContainsKey(i));
                else
                    Assert.IsTrue(rankingsToShow.ContainsKey(i));
            }
        }

        [TestMethod]
        public void Show25WithMax30Pos27()
        {
            int? position = 27;
            const uint recordsToShow = 25;
            const uint maxRecordsToReport = 30;

            DedimaniaRanking[] rankings = GetRankings(maxRecordsToReport, "scordo", position);
            SortedList<uint, DedimaniaRanking> rankingsToShow = DedimaniaUIPlugin.GetRankingsToShow(rankings, "scordo", recordsToShow, maxRecordsToReport);

            Assert.AreEqual(recordsToShow, (uint)rankingsToShow.Count);
            Assert.AreEqual("scordo", rankingsToShow[(uint)position.Value].Login);

            for (uint i = 1; i < maxRecordsToReport; i++)
            {
                if (i < 13)
                    Assert.IsTrue(rankingsToShow.ContainsKey(i));
                else if (i < 18)
                    Assert.IsFalse(rankingsToShow.ContainsKey(i));
                else
                    Assert.IsTrue(rankingsToShow.ContainsKey(i));
            }
        }

        [TestMethod]
        public void Show25WithMax30Pos30()
        {
            int? position = 30;
            const uint recordsToShow = 25;
            const uint maxRecordsToReport = 30;

            DedimaniaRanking[] rankings = GetRankings(maxRecordsToReport, "scordo", position);
            SortedList<uint, DedimaniaRanking> rankingsToShow = DedimaniaUIPlugin.GetRankingsToShow(rankings, "scordo", recordsToShow, maxRecordsToReport);

            Assert.AreEqual(recordsToShow, (uint)rankingsToShow.Count);
            Assert.AreEqual("scordo", rankingsToShow[(uint)position.Value].Login);

            for (uint i = 1; i < maxRecordsToReport; i++)
            {
                if (i < 14)
                    Assert.IsTrue(rankingsToShow.ContainsKey(i));
                else if (i < 19)
                    Assert.IsFalse(rankingsToShow.ContainsKey(i));
                else
                    Assert.IsTrue(rankingsToShow.ContainsKey(i));
            }
        }

        [TestMethod]
        public void Show25WithMax30()
        {
            const uint recordsToShow = 25;
            const uint maxRecordsToReport = 30;

            DedimaniaRanking[] rankings = GetRankings(maxRecordsToReport, "scordo", null);
            SortedList<uint, DedimaniaRanking> rankingsToShow = DedimaniaUIPlugin.GetRankingsToShow(rankings, "scordo", recordsToShow, maxRecordsToReport);

            Assert.AreEqual(recordsToShow, (uint)rankingsToShow.Count);

            for (uint i = 1; i < maxRecordsToReport; i++)
            {
                if (i < 15 || i > 19)
                    Assert.IsTrue(rankingsToShow.ContainsKey(i));
                else
                    Assert.IsFalse(rankingsToShow.ContainsKey(i));
            }
        }

        [TestMethod]
        public void Show26WithMax30()
        {
            const uint recordsToShow = 26;
            const uint maxRecordsToReport = 30;

            DedimaniaRanking[] rankings = GetRankings(maxRecordsToReport, "scordo", null);
            SortedList<uint, DedimaniaRanking> rankingsToShow = DedimaniaUIPlugin.GetRankingsToShow(rankings, "scordo", recordsToShow, maxRecordsToReport);

            Assert.AreEqual(recordsToShow, (uint)rankingsToShow.Count);

            for (uint i = 1; i < maxRecordsToReport; i++)
            {
                if (i < 16 || i > 19)
                    Assert.IsTrue(rankingsToShow.ContainsKey(i));
                else
                    Assert.IsFalse(rankingsToShow.ContainsKey(i));
            }
        }

        [TestMethod]
        public void Show5WithMax30()
        {
            const uint recordsToShow = 5;
            const uint maxRecordsToReport = 30;

            DedimaniaRanking[] rankings = GetRankings(maxRecordsToReport, "scordo", null);
            SortedList<uint, DedimaniaRanking> rankingsToShow = DedimaniaUIPlugin.GetRankingsToShow(rankings, "scordo", recordsToShow, maxRecordsToReport);

            Assert.AreEqual(recordsToShow, (uint)rankingsToShow.Count);

            for (uint i = 1; i < maxRecordsToReport; i++)
            {
                if (i < 5 || i > 29)
                    Assert.IsTrue(rankingsToShow.ContainsKey(i));
                else
                    Assert.IsFalse(rankingsToShow.ContainsKey(i));
            }
        }

        [TestMethod]
        public void Show8WithMax30()
        {
            const uint recordsToShow = 8;
            const uint maxRecordsToReport = 30;

            DedimaniaRanking[] rankings = GetRankings(maxRecordsToReport, "scordo", null);
            SortedList<uint, DedimaniaRanking> rankingsToShow = DedimaniaUIPlugin.GetRankingsToShow(rankings, "scordo", recordsToShow, maxRecordsToReport);

            Assert.AreEqual(recordsToShow, (uint)rankingsToShow.Count);

            for (uint i = 1; i < maxRecordsToReport; i++)
            {
                if (i < 7 || i > 28)
                    Assert.IsTrue(rankingsToShow.ContainsKey(i));
                else
                    Assert.IsFalse(rankingsToShow.ContainsKey(i));
            }
        }

        private static DedimaniaRanking[] GetRankings(uint maxRankings, string login, int? position)
        {
            List<DedimaniaRanking> rankings = new List<DedimaniaRanking>();
            for (uint i = 0; i < maxRankings; i++)
                rankings.Add(new DedimaniaRanking(login + (i + 1), login + (i + 1), (20 + i + 1) * 1000, DateTime.Now));

            if (position.HasValue && position > 0 && position <= maxRankings)
            {
                rankings[position.Value - 1].Login = login;
                rankings[position.Value - 1].Nickname = login;
            }

            return rankings.ToArray();
        }
    }
}
