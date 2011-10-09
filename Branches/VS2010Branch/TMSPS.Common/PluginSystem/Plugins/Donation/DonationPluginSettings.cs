using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Xml.Linq;
using TMSPS.Core.Common;
using TMSPS.Core.Logging;
using TMSPS.Core.PluginSystem.Configuration;
using SettingsBase=TMSPS.Core.Common.SettingsBase;

namespace TMSPS.Core.PluginSystem.Plugins.Donation
{
    public class DonationPluginSettings : SettingsBase
    {
        #region Constants

        public const string DONATION_TARGET_LOGIN = "";
        public const uint MIN_DONATION_VALUE = 10;
        public const string DONATION_TO_SMALL_MSG = "{[#ServerStyle]}>{[#ErrorStyle]} The amount of coppers is to small, you must donate at least {[Coppers]} coppers.";
        public const string PLAYER_HAS_NO_UNITED_ACCOUNT_MSG = "{[#ServerStyle]}>{[#ErrorStyle]} You can not donate coppers, because you do not have a united account.";
        public const string DONATION_HINT = "Please press yes to confirm your donation.";
        public const string REFUSE_MSG = "{[#ServerStyle]}>{[#MessageStyle]} You have chosen to refuse donation, that's sad :(";
        public const string DONATION_ERROR_MSG = "{[#ServerStyle]}>{[#ErrorStyle]} Error during donation: {[ErrorMessage]}";
        public const string DONATION_THANKSR_MSG = "{[#ServerStyle]}>{[#MessageStyle]} Thank you for donating {[Coppers]} coppers!";


        #endregion

        #region Properties

        public string DonationTargetLogin { get; private set; }
        public bool IsServerTargetLogin { get { return DonationTargetLogin == DONATION_TARGET_LOGIN; } }
        public uint MinDonationValue { get; private set; }
        public string DonationToSmallMessage { get; private set; }
        public string PlayerHasNoUnitedAccountMessage { get; private set; }
        public string DonationHint { get; private set; }
        public string RefuseMessage { get; private set; }
        public string DonationErrorMessage { get; private set; }
        public string DonationThanksMessage { get; private set; }
        

        #endregion

        #region Public Methods

        public static DonationPluginSettings ReadFromFile(string xmlConfigurationFile)
        {
            DonationPluginSettings result = new DonationPluginSettings();
            XDocument configDocument = XDocument.Load(xmlConfigurationFile);

            if (configDocument.Root == null)
                throw new ConfigurationErrorsException("Could not find root node in file: " + xmlConfigurationFile);

            result.DonationTargetLogin = ReadConfigString(configDocument.Root, "DonationTargetLogin", DONATION_TARGET_LOGIN, xmlConfigurationFile);
            result.MinDonationValue = ReadConfigUInt(configDocument.Root, "MinDonationValue", MIN_DONATION_VALUE, xmlConfigurationFile);
            result.DonationToSmallMessage = ReadConfigString(configDocument.Root, "DonationToSmallMessage", DONATION_TO_SMALL_MSG, xmlConfigurationFile);
            result.PlayerHasNoUnitedAccountMessage = ReadConfigString(configDocument.Root, "PlayerHasNoUnitedAccountMessage", PLAYER_HAS_NO_UNITED_ACCOUNT_MSG, xmlConfigurationFile);
            result.DonationHint = ReadConfigString(configDocument.Root, "DonationHint", DONATION_HINT, xmlConfigurationFile);
            result.RefuseMessage = ReadConfigString(configDocument.Root, "RefuseMessage", REFUSE_MSG, xmlConfigurationFile);
            result.DonationErrorMessage = ReadConfigString(configDocument.Root, "DonationErrorMessage", DONATION_ERROR_MSG, xmlConfigurationFile);
            result.DonationThanksMessage = ReadConfigString(configDocument.Root, "DonationThanksMessage", DONATION_THANKSR_MSG, xmlConfigurationFile);
            result.Plugins = PluginConfigEntryCollection.ReadFromDirectory(Path.GetDirectoryName(xmlConfigurationFile));

            return result;
        }

        public List<IDonationPluginPlugin> GetPlugins(IUILogger logger)
        {
            List<IDonationPluginPlugin> result = new List<IDonationPluginPlugin>();

            foreach (PluginConfigEntry pluginConfigEntry in Plugins.GetEnabledPlugins())
            {
                if (logger != null)
                    logger.Debug(string.Format("Instantiating IDonationPluginPlugin {0}", pluginConfigEntry.PluginClass));

                result.Add(Instancer.GetInstanceOfInterface<IDonationPluginPlugin>(pluginConfigEntry.AssemblyName, pluginConfigEntry.PluginClass, pluginConfigEntry.PluginDirectory));
            }

            return result;
        }

        #endregion
    }
}