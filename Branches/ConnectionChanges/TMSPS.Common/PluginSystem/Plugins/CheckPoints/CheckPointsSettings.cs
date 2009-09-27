using System.Configuration;
using System.Xml.Linq;
using SettingsBase=TMSPS.Core.Common.SettingsBase;

namespace TMSPS.Core.PluginSystem.Plugins.CheckPoints
{
    public class CheckPointsSettings : SettingsBase
    {
        #region Properties

        public string Template { get; private set; }
        public string SuperiorTimeTextStyle { get; private set; }
        public string InferiorTimeTextStyle { get; private set; }
        public uint TimeoutInSeconds { get; private set; }

        #endregion

        #region Public Methods

        public static CheckPointsSettings ReadFromFile(string xmlConfigurationFile)
        {
            CheckPointsSettings result = new CheckPointsSettings();
            XDocument configDocument = XDocument.Load(xmlConfigurationFile);

            if (configDocument.Root == null)
                throw new ConfigurationErrorsException("Could not find root node in file: " + xmlConfigurationFile);

            result.Template = ReadConfigString(configDocument.Root, "Template", xmlConfigurationFile);
            result.SuperiorTimeTextStyle = ReadConfigString(configDocument.Root, "SuperiorTimeTextStyle", string.Empty, xmlConfigurationFile);
            result.InferiorTimeTextStyle = ReadConfigString(configDocument.Root, "InferiorTimeTextStyle", string.Empty, xmlConfigurationFile);
            result.TimeoutInSeconds = ReadConfigUInt(configDocument.Root, "TimeoutInSeconds", 3, xmlConfigurationFile);
            
            return result;
        }

        #endregion
    }
}