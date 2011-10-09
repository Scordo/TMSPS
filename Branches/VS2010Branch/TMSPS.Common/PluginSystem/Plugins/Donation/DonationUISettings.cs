using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using TMSPS.Core.ManiaLinking;
using SettingsBase = TMSPS.Core.Common.SettingsBase;

namespace TMSPS.Core.PluginSystem.Plugins.Donation
{
    public class DonationUISettings : SettingsBase
    {
        #region Constants

        public const string COPPER_VALUES = "20, 50, 100, 200, 500, 1000, 2000";
        public const byte EXPECTED_AMOUNT_COPPER_VALUES = 7;
        public const string PANEL_ID = "DonationPanelID";

        #endregion

        #region Properties

        public List<uint> CopperValues { get; private set; }
        public string PanelTemplate { get; private set; }

        #endregion

        #region Public Methods

        public static DonationUISettings ReadFromFile(string xmlConfigurationFile, ushort pluginId)
        {
            string settingsDirectory = Path.GetDirectoryName(xmlConfigurationFile);


            DonationUISettings result = new DonationUISettings();
            XDocument configDocument = XDocument.Load(xmlConfigurationFile);

            if (configDocument.Root == null)
                throw new ConfigurationErrorsException("Could not find root node in file: " + xmlConfigurationFile);

            string copperValuesAsString = ReadConfigString(configDocument.Root, "CopperValues", COPPER_VALUES, xmlConfigurationFile);
            List<string> copperValueStringList = copperValuesAsString.Split(new [] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            List<uint> copperValues = new List<uint>();

            copperValueStringList.ForEach(c =>
            {
                uint copper;
                if (!uint.TryParse(c, out copper))
                    throw new ConfigurationErrorsException(string.Format("Copper value '{0}' is not an integer!", c));

                copperValues.Add(copper);
            });

            if (copperValues.Count != EXPECTED_AMOUNT_COPPER_VALUES)
                throw new ConfigurationErrorsException(string.Format("{0} copper values expected, but only {1} copper value(s) given!", EXPECTED_AMOUNT_COPPER_VALUES, copperValues.Count));

            result.CopperValues = copperValues;
            result.PanelTemplate = GetReplacedPanelTemplateContent(ReadConfigString(configDocument.Root, "DonationPanelTemplate", xmlConfigurationFile), copperValues, pluginId);

            return result;
        }

        private static string GetReplacedPanelTemplateContent(string templateContent, List<uint> copperValues, ushort pluginId)
        {
            List<string> replaceValues = new List<string> { "ManiaLinkID", PANEL_ID };

            for (int i = 1; i <= 7; i++)
            {
                replaceValues.Add("Coppers" + i);
                replaceValues.Add(copperValues[i-1].ToString());

                replaceValues.Add("DonateAction" + i);
                replaceValues.Add(TMAction.CalculateActionID(pluginId, (byte)DonationUIArea.MainArea, Convert.ToByte(i)).ToString());
            }

            return TMSPSPluginBase.ReplaceMessagePlaceHolders(templateContent, replaceValues.ToArray());
        }

        #endregion
    }

    public enum DonationUIArea
    {
        MainArea = 1    
    }
}