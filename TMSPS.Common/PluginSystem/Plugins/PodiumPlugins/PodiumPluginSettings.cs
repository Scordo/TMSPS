using System.Configuration;
using System.IO;
using System.Xml.Linq;
using SettingsBase=TMSPS.Core.Common.SettingsBase;

namespace TMSPS.Core.PluginSystem.Plugins.PodiumPlugins
{
    public class PodiumPluginSettings : SettingsBase
    {
        #region Constants

        public const uint MAX_ENTRIES_TO_SHOW = 10;
        public const double ENTRY_START_MARGIN = -2.9;
        public const double ENTRY_HEIGHT = 1.5;
        public const double ENTRY_END_MARGIN = 0;
        public const double ENTRY_TO_CONTAINER_MARGIN_Y = 0.7;

        #endregion

        #region Properties

        public uint MaxEntriesToShow { get; protected internal set; }
        public double EntryStartMargin { get; private set; }
        public double EntryHeight { get; private set; }
        public double EntryEndMargin { get; private set; }
        public double EntryToContainerMarginY { get; private set; }

        public string MainTemplate { get; private set; }
        public string EntryTemplate { get; private set; }
        public string Title { get; private set; }
        public double X { get; private set; }

        #endregion

        #region Methods

        public static PodiumPluginSettings ReadFromFile(string xmlConfigurationFile, string title, double x)
        {
            string settingsDirectory = Path.GetDirectoryName(xmlConfigurationFile);

            PodiumPluginSettings result = new PodiumPluginSettings();
            XDocument configDocument = XDocument.Load(xmlConfigurationFile);

            if (configDocument.Root == null)
                throw new ConfigurationErrorsException("Could not find root node in file: " + xmlConfigurationFile);

            result.MaxEntriesToShow = ReadConfigUInt(configDocument.Root, "MaxEntriesToShow", MAX_ENTRIES_TO_SHOW, xmlConfigurationFile);
            result.Title = ReadConfigString(configDocument.Root, "Title", title, xmlConfigurationFile);
            result.X = ReadConfigDouble(configDocument.Root, "PositionX", x, xmlConfigurationFile);
            result.EntryToContainerMarginY = ENTRY_TO_CONTAINER_MARGIN_Y;
            result.EntryStartMargin = ENTRY_START_MARGIN;
            result.EntryEndMargin = ENTRY_END_MARGIN;
            result.EntryHeight = ENTRY_HEIGHT;
            result.MainTemplate = UITemplates.MainTemplate;
            result.EntryTemplate = UITemplates.EntryTemplate;

            string recordListTemplateFile = Path.Combine(settingsDirectory, "EntryListTemplate.xml");

            if (File.Exists(recordListTemplateFile))
            {
                XDocument listTemplateDocument = XDocument.Load(recordListTemplateFile);

                if (listTemplateDocument.Root == null)
                    throw new ConfigurationErrorsException("Could not find root node in file: " + listTemplateDocument);

                result.EntryToContainerMarginY = ReadConfigDouble(listTemplateDocument.Root, "EntryToContainerMarginY", result.EntryToContainerMarginY, recordListTemplateFile);
                result.EntryStartMargin = ReadConfigDouble(listTemplateDocument.Root, "EntryStartMargin", result.EntryStartMargin, recordListTemplateFile);
                result.EntryEndMargin = ReadConfigDouble(listTemplateDocument.Root, "EntryEndMargin", result.EntryEndMargin, recordListTemplateFile);
                result.EntryHeight = ReadConfigDouble(listTemplateDocument.Root, "EntryHeight", result.EntryHeight, recordListTemplateFile);

                XElement templatesElement = listTemplateDocument.Root.Element("Templates");

                if (templatesElement != null)
                {
                    result.MainTemplate = ReadConfigString(templatesElement, "MainTemplate", result.MainTemplate, recordListTemplateFile);
                    result.EntryTemplate = ReadConfigString(templatesElement, "EntryTemplate", result.EntryTemplate, recordListTemplateFile);
                }
            }

            return result;
        }

        #endregion
    }
}
