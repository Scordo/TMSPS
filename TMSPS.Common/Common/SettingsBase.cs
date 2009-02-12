using System;
using System.Configuration;
using System.Xml.Linq;

namespace TMSPS.Core.Common
{
    public class SettingsBase
    {
        public static string ReadConfigString(XContainer parentElement, string elementName, string configFile)
        {
            return ReadConfigString(parentElement, elementName, null, configFile);
        }

        public static string ReadConfigString(XContainer parentElement, string elementName, string defaultValue, string configFile)
        {
            return ReadConfigValue(parentElement, elementName, defaultValue, defaultValue != null, configFile, s => s);
        }

        public static uint ReadConfigUInt(XContainer parentElement, string elementName, string configFile)
        {
            return ReadConfigUInt(parentElement, elementName, null, configFile);
        }

        public static uint ReadConfigUInt(XContainer parentElement, string elementName, uint? defaultValue, string configFile)
        {
            return ReadConfigValue(parentElement, elementName, defaultValue.HasValue ? defaultValue.Value : 0, defaultValue.HasValue, configFile, s => uint.Parse(s));
        }

        public static bool ReadConfigBool(XContainer parentElement, string elementName, string configFile)
        {
            return ReadConfigBool(parentElement, elementName, null, configFile);
        }

        public static bool ReadConfigBool(XContainer parentElement, string elementName, bool? defaultValue, string configFile)
        {
            return ReadConfigValue(parentElement, elementName, defaultValue.HasValue ? defaultValue.Value : false, defaultValue.HasValue, configFile, s => s != null && string.Compare(s.Trim(), "true", StringComparison.InvariantCultureIgnoreCase) == 0);
        }

        public static T ReadConfigValue<T>(XContainer parentElement, string elementName, T defaultValue, bool isOptional, string configFile, Converter<string, T> converter)
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

            return converter(valueElement.Value);
        }
    }
}
