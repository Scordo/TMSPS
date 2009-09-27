using Microsoft.VisualStudio.TestTools.UnitTesting;
using TMSPS.Core.ManiaLinking;

namespace TMSPS.UnitTests.Core.ManiaLinking
{
    [TestClass]
    public class TMActionUTC
    {
        [TestMethod]
        public void TestAreaAction()
        {
            TMAction action = new TMAction(2, 3, 4);
            uint actionID = action.ToCalculatedActionID();
            TMAction parsedAction = TMAction.Parse(actionID);

            Assert.AreEqual(action.AreaActionID, parsedAction.AreaActionID);
            Assert.AreEqual(action.AreaID, parsedAction.AreaID);
            Assert.AreEqual(action.PluginID, parsedAction.PluginID);
            Assert.AreEqual(action.RowActionID, parsedAction.RowActionID);
            Assert.AreEqual(action.RowIndex, parsedAction.RowIndex);
        }

        [TestMethod]
        public void TestRowAction()
        {
            TMAction action = new TMAction(2, 3, 4, 5);
            uint actionID = action.ToCalculatedActionID();
            TMAction parsedAction = TMAction.Parse(actionID);

            Assert.AreEqual(action.AreaActionID, parsedAction.AreaActionID);
            Assert.AreEqual(action.AreaID, parsedAction.AreaID);
            Assert.AreEqual(action.PluginID, parsedAction.PluginID);
            Assert.AreEqual(action.RowActionID, parsedAction.RowActionID);
            Assert.AreEqual(action.RowIndex, parsedAction.RowIndex);
        }
    }
}
