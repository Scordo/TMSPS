using System.Collections.Generic;
using System.Globalization;
using TMSPS.Core.ManiaLinking;

namespace TMSPS.Core.PluginSystem
{
    public class PagedDialogActions
    {
        #region Properties

        public uint CloseActionID { get; private set; }
        public uint FirstPageActionID { get; private set; }
        public uint PrevPageActionID { get; private set; }
        public uint NextPageActionID { get; private set; }
        public uint LastPageActionID { get; private set; }
        public ushort PluginID { get; private set; }
        public byte AreaID { get; private set; }

        #endregion

        #region Constructors

        public PagedDialogActions(ushort pluginID, byte areaID) : this (pluginID, areaID, (byte) DefaultDialogAction.CloseDialog, (byte) DefaultDialogAction.FirstPage,(byte) DefaultDialogAction.PrevPage,(byte) DefaultDialogAction.NextPage,(byte) DefaultDialogAction.LastPage)
        {
            
        }

        public PagedDialogActions(ushort pluginID, byte areaID, byte closeAction, byte firstPageAction, byte prevPageAction, byte nextPageAction, byte lastPageAction)
        {
            PluginID = pluginID;
            AreaID = areaID;

            CloseActionID = TMAction.CalculateActionID(pluginID, areaID, closeAction);
            FirstPageActionID = TMAction.CalculateActionID(pluginID, areaID, firstPageAction);
            PrevPageActionID = TMAction.CalculateActionID(pluginID, areaID, prevPageAction);
            NextPageActionID = TMAction.CalculateActionID(pluginID, areaID, nextPageAction);
            LastPageActionID = TMAction.CalculateActionID(pluginID, areaID, lastPageAction);
        }

        #endregion

        #region Methods

        public string[] GetReplaceParameters(params KeyValuePair<string, byte>[] actionNameValuePairs)
        {
            List<string> commands = new List<string>();

            commands.Add("CloseActionID");
            commands.Add(CloseActionID.ToString(CultureInfo.InvariantCulture));

            commands.Add("FirstPageActionID");
            commands.Add(FirstPageActionID.ToString(CultureInfo.InvariantCulture));

            commands.Add("PrevPageActionID");
            commands.Add(PrevPageActionID.ToString(CultureInfo.InvariantCulture));

            commands.Add("NextPageActionID");
            commands.Add(NextPageActionID.ToString(CultureInfo.InvariantCulture));

            commands.Add("LastPageActionID");
            commands.Add(LastPageActionID.ToString(CultureInfo.InvariantCulture));

            if (actionNameValuePairs != null)
            {
                foreach (KeyValuePair<string, byte> nameValuePair in actionNameValuePairs)
                {
                    commands.Add(nameValuePair.Key);
                    commands.Add(TMAction.CalculateActionID(PluginID, AreaID, nameValuePair.Value).ToString(CultureInfo.InvariantCulture));
                }
            }

            return commands.ToArray();
        }

        #endregion


        public enum DefaultDialogAction : byte
        {
            CloseDialog = 1,
            FirstPage = 2,
            PrevPage = 3,
            NextPage = 4,
            LastPage = 5
        }
    }
}