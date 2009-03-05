using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Text;
using System.Xml.Linq;

namespace TMSPS.Core.PluginSystem.Plugins.PodiumPlugins
{
    public class PodiumPluginUI
    {
        public static string GetRecordListManiaLinkPage(IList<PodiumPluginUIEntry> entries, string maniaLinkPageID, PodiumPluginSettings settings)
        {
            double totalHeight = Math.Abs(settings.EntryToContainerMarginY * 2) + Math.Abs(settings.EntryHeight * entries.Count) + Math.Abs(settings.EntryEndMargin);

            XElement mainTemplate = XElement.Parse(settings.MainTemplate.Replace("{[ManiaLinkID]}", maniaLinkPageID).Replace("{[ContainerHeight}}", totalHeight.ToString(CultureInfo.InvariantCulture)).Replace("{[Title]}", settings.Title).Replace("{[X]}", settings.X.ToString(CultureInfo.InvariantCulture)));
            XElement rankingPlaceHolder = mainTemplate.Descendants("EntryPlaceHolder").First();
            double currentY = settings.EntryStartMargin;

            XElement lastInsertedNode = rankingPlaceHolder;

            for (uint i = 1; i <= entries.Count; i++)
            {
                PodiumPluginUIEntry entry = entries[(int) i - 1];
                XElement currentElement = GetEntryElement(entry, settings.EntryTemplate, currentY, i, settings.StripNickFormatting);
                lastInsertedNode.AddAfterSelf(currentElement);
                lastInsertedNode = currentElement;
                currentY -= settings.EntryHeight;
            }

            rankingPlaceHolder.Remove();

            return mainTemplate.ToString();
        }

        private static XElement GetEntryElement(PodiumPluginUIEntry entryInfo, string templateXML, double currentY, uint currentPosition, bool stripFormatting)
        {
            StringBuilder playerRecordXml = new StringBuilder(templateXML);
            playerRecordXml.Replace("{[Y]}", currentY.ToString(CultureInfo.InvariantCulture));
            playerRecordXml.Replace("{[Rank]}", currentPosition + ".");
            playerRecordXml.Replace("{[Value]}", entryInfo.Value);

            string description = SecurityElement.Escape(entryInfo.Description);

            if (stripFormatting)
                description = TMSPSPluginBase.StripTMColorsAndFormatting(description);

            playerRecordXml.Replace("{[Description]}", description);

            return XElement.Parse(playerRecordXml.ToString());
        }
    }

    public class PodiumPluginUIEntry
    {
        #region Properties

        public string Value { get; set; }
        public string Description { get; set; }

        #endregion

        #region Constructors

        public PodiumPluginUIEntry()
        {

        }

        public PodiumPluginUIEntry(string value, string description)
        {
            Value = value;
            Description = description;
        }

        #endregion
    }
}
