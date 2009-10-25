using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMSPS.Core.Common;
using System.Xml.Linq;
using System.Configuration;

namespace TMSPS.Core.PluginSystem.Plugins.Restart
{
    public class RestartPluginSettings : TMSPS.Core.Common.SettingsBase
    {
        #region Constants

        public const ushort NO_RESTART_PLAYER_LIMIT = 20; // do not restart when 20 ore more players are on the server
        public const double NO_RESTART_VOTES_RATIO = 0.25; // no restart if 25% or more of the player actively voted against a restart
        public const double SIMPLE_RESTART_VOTE_RATIO = 1; // when 100% of the players voted for a restart, the restart is done
        public const double ADVANCED_RESTART_VOTE_RATIO = 0.5; // when 50% of the players voted for a restart, the restart is done. Negative votes are substracted from the positive votes. If the percentage of the resulting percentage is larger or equal than 50% the vote succeeds.
        public const ushort FINISH_DLAY = 12000; // check 12 seconds after race has finished for the result
        
        #endregion

        #region Properties

        public ushort NoRestartPlayerLimit { get; private set; }
        public double NoRestartVotesRatio { get; private set; }
        public double SimpleRestartVoteRatio { get; private set; }
        public double AdvancedRestartVoteRatio { get; private set; }
        public ushort FinishDelay { get; private set; }

        #endregion

        #region Public Methods

        public static RestartPluginSettings ReadFromFile(string xmlConfigurationFile)
        {
            RestartPluginSettings result = new RestartPluginSettings();
            XDocument configDocument = XDocument.Load(xmlConfigurationFile);
            if (configDocument.Root == null)
                throw new ConfigurationErrorsException("Could not find root node in file: " + xmlConfigurationFile);

            result.NoRestartPlayerLimit = ReadConfigUShort(configDocument.Root, "NoRestartPlayerLimit", NO_RESTART_PLAYER_LIMIT, xmlConfigurationFile);
            result.NoRestartVotesRatio = ReadConfigDouble(configDocument.Root, "NoRestartVotesRatio", NO_RESTART_VOTES_RATIO, xmlConfigurationFile);
            result.SimpleRestartVoteRatio = ReadConfigDouble(configDocument.Root, "SimpleRestartVoteRatio", SIMPLE_RESTART_VOTE_RATIO, xmlConfigurationFile);
            result.AdvancedRestartVoteRatio = ReadConfigDouble(configDocument.Root, "AdvancedRestartVoteRatio", ADVANCED_RESTART_VOTE_RATIO, xmlConfigurationFile);
            result.FinishDelay = ReadConfigUShort(configDocument.Root, "FinishDelay", FINISH_DLAY, xmlConfigurationFile);

            return result;
        }

        #endregion
    }
}
