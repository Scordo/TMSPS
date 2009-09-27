using System.IO;
using TMSPS.Core.Common;

namespace TMSPS.Core.PluginSystem.Plugins.Clock
{
    public class ClockPluginSettings : SettingsBase
    {
        #region Properties

        public string ClockTemplate { get; private set; }

        #endregion

        #region Public Methods

        public static ClockPluginSettings ReadFromFile(string xmlConfigurationFile)
        {
            string settingsDirectory = Path.GetDirectoryName(xmlConfigurationFile);
            string templateFilePath = Path.Combine(settingsDirectory, "ClockTemplate.xml");
            
            ClockPluginSettings result = new ClockPluginSettings();
            result.ClockTemplate = UITemplates.ClockTemplate;

            if (File.Exists(templateFilePath))
                result.ClockTemplate = File.ReadAllText(templateFilePath);

            return result;
        }

        #endregion
    }
}
