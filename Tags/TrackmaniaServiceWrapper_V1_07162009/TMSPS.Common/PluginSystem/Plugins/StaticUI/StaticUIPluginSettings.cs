using System.Configuration;
using System.IO;
using System.Xml.Linq;
using SettingsBase=TMSPS.Core.Common.SettingsBase;

namespace TMSPS.Core.PluginSystem.Plugins.StaticUI
{
    public class StaticUIPluginSettings : SettingsBase
    {
        #region Constants

        public const bool HIDE_ON_FINISH = true;

        #endregion

        #region Properties

        public bool HidOnFinish { get; protected internal set; }
        public string ManiaLinkPageContent { get; protected internal set; }


        #endregion

        #region Public Methods

        public static StaticUIPluginSettings ReadFromFile(string xmlConfigurationFile)
        {
            string settingsDirectory = Path.GetDirectoryName(xmlConfigurationFile);
            StaticUIPluginSettings result = new StaticUIPluginSettings();
            XDocument configDocument = XDocument.Load(xmlConfigurationFile);

            if (configDocument.Root == null)
                throw new ConfigurationErrorsException("Could not find root node in file: " + xmlConfigurationFile);

            result.HidOnFinish = ReadConfigBool(configDocument.Root, "HidOnFinish", HIDE_ON_FINISH, xmlConfigurationFile);
            string contentFile = Path.Combine(settingsDirectory, "Content.xml");

            if (!File.Exists(contentFile))
                throw new FileNotFoundException("Could not find file content to display!", contentFile);

            result.ManiaLinkPageContent = File.ReadAllText(contentFile);

            return result;
        }

        #endregion
    }
}