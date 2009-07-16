using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TMSPS.Core.Common;

namespace TMSPS.UnitTests.Core.Common
{
    /// <summary>
    /// Summary description for TimedVolatileExecutionQueueUTC
    /// </summary>
    [TestClass]
    public class TimedVolatileExecutionQueueUTC
    {
        [TestMethod]
        public void Test1()
        {
            TimedVolatileExecutionQueue<object[]> queue = new TimedVolatileExecutionQueue<object[]>(TimeSpan.FromSeconds(1));

            const int millisecondsToRun = 5500;

            List<int> addedValues = new List<int>();

            Console.WriteLine(string.Format("[{0}] Start", DateTime.Now));
            for (int i = 0; i < millisecondsToRun / 50; i++)
            {
                queue.Enqueue(Execute, new object[] {addedValues, i});
                Thread.Sleep(50);
            }

            Assert.AreEqual(5, addedValues.Count);
        }

        private static void Execute(object[] parameters)
        {
            List<int> addedValues = (List<int>) parameters[0];
            int value = (int) parameters[1];

            addedValues.Add(value);

            Console.WriteLine(string.Format("[{0}] Value: {1}", DateTime.Now, value));
        }
    }
}
