using System;
using System.Configuration;
using System.Xml.Linq;
using SettingsBase=TMSPS.Core.Common.SettingsBase;

namespace TMSPS.Core.PluginSystem.Configuration
{
    public abstract class PagedUIDialogSettingsBase<T> : SettingsBase where T : PagedUIDialogSettingsBase<T>
    {
        public delegate void ReadXmlConfigDataDelegate(XElement rootElement, T result);

        #region Properties

        public double FirstEntryTopMargin { get; private set; }
        public double EntryHeight { get; private set; }
        public uint MaxEntriesPerPage { get; private set; }

        public string EntryTemplate { get; private set; }
        public string SinglePageTemplate { get; private set; }
        public string FirstPageTemplate { get; private set; }
        public string MiddlePageTemplate { get; private set; }
        public string LastPageTemplate { get; private set; }

        #endregion

        #region Methods

        public static PagedUIDialogSettingsBase<T> ReadFromFile(string xmlConfigurationFile)
        {
            return ReadFromFile(xmlConfigurationFile, null);
        }

        protected static T ReadFromFile(string xmlConfigurationFile, ReadXmlConfigDataDelegate additionReadConfigLogic)
        {
            T result = (T)Activator.CreateInstance(typeof(T), null);
            XDocument configDocument = XDocument.Load(xmlConfigurationFile);

            if (configDocument.Root == null)
                throw new ConfigurationErrorsException("Could not find root node in file: " + xmlConfigurationFile);

            result.FirstEntryTopMargin = ReadConfigDouble(configDocument.Root, "FirstEntryTopMargin", xmlConfigurationFile);
            result.EntryHeight = ReadConfigDouble(configDocument.Root, "EntryHeight", xmlConfigurationFile);
            result.MaxEntriesPerPage = ReadConfigUInt(configDocument.Root, "MaxEntriesPerPage", xmlConfigurationFile);


            XElement templatesElement = configDocument.Root.Element("Templates");

            if (templatesElement == null)
                throw new ConfigurationErrorsException("Could not find Templates-Node in file: " + xmlConfigurationFile);

            result.EntryTemplate = ReadConfigString(templatesElement, "EntryTemplate", xmlConfigurationFile);
            result.SinglePageTemplate = ReadConfigString(templatesElement, "SinglePageTemplate", xmlConfigurationFile);
            result.FirstPageTemplate = ReadConfigString(templatesElement, "FirstPageTemplate", xmlConfigurationFile);
            result.MiddlePageTemplate = ReadConfigString(templatesElement, "MiddlePageTemplate", xmlConfigurationFile);
            result.LastPageTemplate = ReadConfigString(templatesElement, "LastPageTemplate", xmlConfigurationFile);

            if (additionReadConfigLogic != null)
                additionReadConfigLogic(configDocument.Root, result);

            return result;
        }

        #endregion
    }
}
