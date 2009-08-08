﻿using System;
using System.Configuration;
using System.Data.SQLite;
using System.Xml.Linq;
using TMSPS.Core.Common;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords;

namespace TMSPS.SQLite
{
    public class SQLiteNicknameResolver : NicknameResolverBase
    {
        private ConnectionManager ConnectionManager { get; set; }
        private PlayerAdapter PlayerAdapter { get; set; }

        public override string Get(string login, bool returnLoginOnFailure)
        {
            if (NicknameCache.ContainsKey(login))
                return NicknameCache[login].Nickname;

            Player player = PlayerAdapter.Deserialize(login);

            if (player == null)
                return returnLoginOnFailure ? login : null;

            return player.Nickname;
        }

        public override void Set(string login, string nickname)
        {
            PlayerAdapter.CreateOrUpdate(new Player(login, nickname));
            UpdateCacheForLogin(login, nickname);
        }

        /// <exception cref="ConfigurationErrorsException">Could not find connectionString Attribute for SQLiteNicknameResolver in NicknameResolver-Config Xml-Node.</exception>
        /// <exception cref="ConfigurationErrorsException">connectionString Attribute for SQLiteNicknameResolver in NicknameResolver-Config Xml-Node is empty.</exception>
        public override void ReadConfigSettings(XElement configElement)
        {
            XAttribute connectionStringAttribute = configElement.Attribute("connectionString");
            if (connectionStringAttribute == null)
                throw new ConfigurationErrorsException("Could not find connectionString Attribute for SQLiteNicknameResolver in NicknameResolver-Config Xml-Node.", (Exception)null);

            if (connectionStringAttribute.Value.IsNullOrTimmedEmpty())
                throw new ConfigurationErrorsException("connectionString Attribute for SQLiteNicknameResolver in NicknameResolver-Config Xml-Node is empty.", (Exception)null);

            try
            {
                new SQLiteConnectionStringBuilder(connectionStringAttribute.Value.Trim());
            }
            catch (Exception ex)
            {
                throw new ConfigurationErrorsException("connectionString Attribute for SQLiteNicknameResolver in NicknameResolver-Config Xml-Node has a wrong format.", ex);
            }

            ConnectionManager = new ConnectionManager(connectionStringAttribute.Value.Trim(), false);
            PlayerAdapter = new PlayerAdapter(ConnectionManager);
        }
    }
}
