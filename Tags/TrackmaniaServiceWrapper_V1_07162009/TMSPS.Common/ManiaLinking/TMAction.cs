using System;

namespace TMSPS.Core.ManiaLinking
{
    public class TMAction
    {
        #region Properties

        public ushort PluginID { get; private set; }
        public byte AreaID { get; private set; }
        public byte AreaActionID { get; private set; }
        public byte RowIndex { get; private set; }
        public byte RowActionID { get; private set; }

        public bool IsAreaAction { get { return AreaActionID != 0;} }
        public bool IsRowAction { get { return RowActionID != 0; } }

        #endregion

        #region Constructors

        private TMAction()
        {
            
        }

        public TMAction(ushort pluginID, byte areaID, byte areaActionID)
        {
            if (pluginID < 1 || pluginID > 1023)
                throw new ArgumentOutOfRangeException("pluginID", pluginID, "pluginID must be between 1 and 1023");

            if (areaID < 1 || areaID > 15)
                throw new ArgumentOutOfRangeException("areaID", areaID, "areaID must be between 1 and 15");

            if (areaActionID < 1)
                throw new ArgumentOutOfRangeException("areaActionID", areaActionID, "areaActionID must be between 1 and 255");

            PluginID = pluginID;
            AreaID = areaID;
            AreaActionID = areaActionID;

            RowActionID = 0;
            RowIndex = 0;
        }

        public TMAction(ushort pluginID, byte areaID, byte rowIndex, byte rowActionID)
        {
            if (pluginID < 1 || pluginID > 1023)
                throw new ArgumentOutOfRangeException("pluginID", pluginID, "pluginID must be between 1 and 1023");

            if (areaID  < 1 || areaID > 15)
                throw new ArgumentOutOfRangeException("areaID", areaID, "areaID must be between 1 and 15");
            
            if (rowIndex > 31)
                throw new ArgumentOutOfRangeException("rowIndex", rowIndex, "rowIndex must be between 0 and 31");

            if (rowActionID < 1 || rowActionID > 15)
                throw new ArgumentOutOfRangeException("rowActionID", rowActionID, "rowActionID must be between 1 and 15");

            PluginID = pluginID;
            AreaID = areaID;
            RowIndex = rowIndex;
            RowActionID = rowActionID;
            AreaActionID = 0;
        }

        #endregion

        #region Public Methods

        public uint ToCalculatedActionID()
        {
            return  Convert.ToUInt32(PluginID << 21) +
                    Convert.ToUInt32(AreaID << 17) +
                    Convert.ToUInt32(AreaActionID << 9) +
                    Convert.ToUInt32(RowIndex << 4) +
                    Convert.ToUInt32(RowActionID);
        }

        public static TMAction Parse(uint actionID)
        {
            if (actionID > int.MaxValue)
                return null;

            TMAction result = new TMAction();
            result.PluginID = Convert.ToUInt16(actionID >> 21);
            actionID -= Convert.ToUInt32(result.PluginID << 21);
            if (result.PluginID == 0)
                return null;

            
            result.AreaID = Convert.ToByte(actionID >> 17);
            actionID -= Convert.ToUInt32(result.AreaID << 17);
            if (result.AreaID == 0)
                return null;

            
            result.AreaActionID = Convert.ToByte(actionID >> 9);
            actionID -= Convert.ToUInt32(result.AreaActionID << 9);
            
            result.RowIndex = Convert.ToByte(actionID >> 4);
            actionID -= Convert.ToUInt32(result.RowIndex << 4);
            
            result.RowActionID = Convert.ToByte(actionID);

            return result;
        }

        public static uint CalculateActionID(ushort pluginID, byte areaID, byte areaActionID)
        {
            return new TMAction(pluginID, areaID, areaActionID).ToCalculatedActionID();
        }

        public static uint CalculateActionID(ushort pluginID, byte areaID, byte rowIndex, byte rowActionID)
        {
            return new TMAction(pluginID, areaID, rowIndex, rowActionID).ToCalculatedActionID();
        }

        #endregion
    }
}