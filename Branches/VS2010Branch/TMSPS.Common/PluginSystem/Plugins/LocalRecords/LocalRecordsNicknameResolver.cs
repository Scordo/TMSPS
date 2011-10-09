using System;
using System.Configuration;
using System.Xml.Linq;
using TMSPS.Core.Common;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords.Entities;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords.Repositories;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords
{
	public class LocalRecordsNicknameResolver : NicknameResolverBase
	{
        public override string Get(string login, bool returnLoginOnFailure)
		{
			if (NicknameCache.ContainsKey(login))
				return NicknameCache[login].Nickname;

            PlayerEntity player = PlayerCache.Instance.Get(login);

            if (player == null)
                return returnLoginOnFailure ? login : null;

			return player.Nickname;
		}

		public override void Set(string login, string nickname)
		{
		    PlayerCache.Instance.EnsureExists(login, nickname);
			UpdateCacheForLogin(login, nickname);
		}

		public override void ReadConfigSettings(XElement configElement)
		{
            XAttribute connectionStringAttribute = configElement.Attribute("connectionString");
            if (connectionStringAttribute == null)
                throw new ConfigurationErrorsException("Could not find connectionString Attribute for LocalRecordsNicknameResolver in NicknameResolver-Config Xml-Node.", (Exception)null);

            if (connectionStringAttribute.Value.IsNullOrTimmedEmpty())
                throw new ConfigurationErrorsException("connectionString Attribute for LocalRecordsNicknameResolver in NicknameResolver-Config Xml-Node is empty.", (Exception)null);
            
            XAttribute databaseTypeAttribute = configElement.Attribute("databaseType");

	        if (databaseTypeAttribute == null)
                throw new ConfigurationErrorsException("Could not find databaseType Attribute for LocalRecordsNicknameResolver in NicknameResolver-Config Xml-Node.", (Exception)null);


		    string databaseTypeName = databaseTypeAttribute.Value;
            if (databaseTypeName.IsNullOrTimmedEmpty())
                throw new ConfigurationErrorsException("databaseType Attribute for LocalRecordsNicknameResolver in NicknameResolver-Config Xml-Node is empty.", (Exception)null);

            DatabaseType databaseType;

	        try
	        {
                databaseType = (DatabaseType)Enum.Parse(typeof(DatabaseType), databaseTypeAttribute.Value.Trim(), true);

                if (!Enum.IsDefined(typeof(DatabaseType), databaseType))
                    throw new ArgumentException();
	        }
	        catch (ArgumentException)
	        {
	            throw new ConfigurationErrorsException(string.Format("databaseType Attribute has an invalid value of '{0}': {1}", databaseTypeAttribute.Value), (Exception)null);
	        }

            RepositoryFactory.Init(databaseType, connectionStringAttribute.Value.Trim());
            PlayerCache.Init(10000);
		}
	}
}