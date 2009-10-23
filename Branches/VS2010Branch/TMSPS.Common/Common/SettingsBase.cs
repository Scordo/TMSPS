using System;
using System.Configuration;
using System.Globalization;
using System.Xml.Linq;
using TMSPS.Core.PluginSystem.Configuration;

namespace TMSPS.Core.Common
{
    public class SettingsBase
    {
        private static CultureInfo Culture { get { return CultureInfo.InvariantCulture; } }
        protected PluginConfigEntryCollection Plugins { get; set; }

        public static string ReadConfigString(XContainer parentElement, string elementName, string configFile)
        {
            return ReadConfigString(parentElement, elementName, null, configFile);
        }

        public static string ReadConfigString(XContainer parentElement, string elementName, string defaultValue, string configFile)
        {
            return ReadConfigValue(parentElement, elementName, defaultValue, defaultValue != null, configFile, s => s.HasElements ? s.InnerXML() : s.Value);
        }

        public static uint ReadConfigUInt(XContainer parentElement, string elementName, string configFile)
        {
            return ReadConfigUInt(parentElement, elementName, null, configFile);
        }

        public static uint ReadConfigUInt(XContainer parentElement, string elementName, uint? defaultValue, string configFile)
        {
            return ReadConfigValue(parentElement, elementName, defaultValue.HasValue ? defaultValue.Value : 0, defaultValue.HasValue, configFile, s => uint.Parse(s.Value, Culture));
        }

        public static double ReadConfigDouble(XContainer parentElement, string elementName, string configFile)
        {
            return ReadConfigDouble(parentElement, elementName, null, configFile);
        }

        public static double ReadConfigDouble(XContainer parentElement, string elementName, double? defaultValue, string configFile)
        {
            return ReadConfigValue(parentElement, elementName, defaultValue.HasValue ? defaultValue.Value : 0, defaultValue.HasValue, configFile, s => double.Parse(s.Value, Culture));
        }

        public static bool ReadConfigBool(XContainer parentElement, string elementName, string configFile)
        {
            return ReadConfigBool(parentElement, elementName, null, configFile);
        }

        public static bool ReadConfigBool(XContainer parentElement, string elementName, bool? defaultValue, string configFile)
        {
            return ReadConfigValue(parentElement, elementName, defaultValue.HasValue ? defaultValue.Value : false, defaultValue.HasValue, configFile, s => s != null && string.Compare(s.Value.Trim(), "true", StringComparison.InvariantCultureIgnoreCase) == 0);
        }

        public static T ReadConfigValue<T>(XContainer parentElement, string elementName, T defaultValue, bool isOptional, string configFile, Converter<XElement, T> converter)
        {
            if (parentElement == null)
                throw new ArgumentNullException("parentElement");

            XElement valueElement = parentElement.Element(elementName);

            if (valueElement == null)
            {
                if (isOptional)
                    return defaultValue;

                throw new ConfigurationErrorsException(string.Format("Could not find {0} node in config: {1}", elementName, configFile));
            }

            return converter(valueElement);
        }
    }
}
