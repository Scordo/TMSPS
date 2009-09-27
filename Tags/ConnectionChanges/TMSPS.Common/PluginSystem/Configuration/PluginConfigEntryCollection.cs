using System;
using System.IO;
using System.Xml.Linq;
using System.Collections.Generic;
using TMSPS.Core.Logging;

namespace TMSPS.Core.PluginSystem.Configuration
{
	public class PluginConfigEntryCollection : List<PluginConfigEntry>
	{
		#region Constructor

		private PluginConfigEntryCollection()
		{
			
		}

		#endregion

		#region Public Methods

        public static PluginConfigEntryCollection ReadFromDirectory(string directoryPath)
        {
            return ReadFromDirectory(directoryPath, null);
        }

        public static PluginConfigEntryCollection ReadFromDirectory(string directoryPath, ILogger logger)
        {
            return ReadFromDirectory(directoryPath, 1, logger);
        }

        public static PluginConfigEntryCollection ReadFromDirectory(string directoryPath, int? recursionDepth, ILogger logger)
        {
            if (recursionDepth <= 0)
                throw new ArgumentOutOfRangeException("recursionDepth", recursionDepth, "recursionDepth can not be 0 or less than 0.");

            List<string> settingsFilePathList = GetSettingsFilePaths(directoryPath, recursionDepth);
            PluginConfigEntryCollection result = new PluginConfigEntryCollection();

            foreach (string settingsFilePath in settingsFilePathList)
            {
                XElement pluginSettingsElement;

                try
                {
                    pluginSettingsElement = XElement.Load(settingsFilePath);
                }
                catch (Exception ex)
                {
                    if (logger != null && !(logger is IUILogger))
                        logger.Error(string.Format("Couldn't load settings from  plugin settings path '{0}'. File is not a valid xml file.", settingsFilePath), ex);

                    if (logger != null && logger is IUILogger)
                        ((IUILogger)logger).ErrorToUI(string.Format("Couldn't load settings from  plugin settings path '{0}'. File is not a valid xml file.", settingsFilePath), ex);

                    continue;
                }

                try
                {
                    result.Add(PluginConfigEntry.ReadFromXElement(pluginSettingsElement, Path.GetDirectoryName(settingsFilePath)));
                }
                catch (Exception ex)
                {
                    if (logger != null && !(logger is IUILogger))
                        logger.Error(string.Format("Couldn't load settings from  plugin settings path '{0}'. File is missing some settings.", settingsFilePath), ex);

                    if (logger != null && logger is IUILogger)
                        ((IUILogger)logger).ErrorToUI(string.Format("Couldn't load settings from  plugin settings path '{0}'. File is missing some settings.", settingsFilePath), ex);
                }
            }

            result.Sort((p1, p2) => p1.Order - p2.Order);

            return result;
        }

        private static List<string> GetSettingsFilePaths(string startDirectoryPath, int? recursionDepth)
        {
            List<string> result = new List<string>();
            AddSettingsFilePathsFromDirectory(startDirectoryPath, 0, recursionDepth, result);

            return result;
        }

        private static void AddSettingsFilePathsFromDirectory(string directoryPath, int currentRecursionDepth, int? maxRrecursionDepth, ICollection<string> settingsFilePathList)
        {
            if (currentRecursionDepth > maxRrecursionDepth)
                return;

            string currentSettingsFilePath = Path.Combine(directoryPath, "Settings.xml");
            if (currentRecursionDepth > 0 && File.Exists(currentSettingsFilePath))
                settingsFilePathList.Add(currentSettingsFilePath);

            foreach (string subDirectoryPath in Directory.GetDirectories(directoryPath))
            {
                AddSettingsFilePathsFromDirectory(subDirectoryPath, currentRecursionDepth + 1, maxRrecursionDepth, settingsFilePathList);
            } 
        }

		public PluginConfigEntryCollection GetEnabledPlugins()
		{
			PluginConfigEntryCollection result = new PluginConfigEntryCollection();
			result.AddRange(FindAll(plugin => plugin.Enabled));

			return result;
		}

		#endregion
	}
}