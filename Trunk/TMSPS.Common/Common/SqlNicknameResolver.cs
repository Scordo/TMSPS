using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Xml.Linq;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords.SQL;
using TMSPS.Core.SQL;

namespace TMSPS.Core.Common
{
	public class SqlNicknameResolver : NicknameResolverBase
	{
		private ConnectionManager ConnectionManager { get; set; }
		private PlayerAdapter PlayerAdapter { get; set; }

		public override string Get(string login)
		{
			if (NicknameCache.ContainsKey(login))
				return NicknameCache[login].Nickname;

			Player player = PlayerAdapter.Deserialize(login);

			return player == null ? null : player.Nickname;
		}

		public override void Set(string login, string nickname)
		{
			PlayerAdapter.CreateOrUpdate(new Player(login, nickname));
			UpdateCacheForLogin(login, nickname);
		}

		/// <exception cref="ConfigurationErrorsException">Could not find connectionString Attribute for SqlNicknameResolver in NicknameResolver-Config Xml-Node.</exception>
		/// <exception cref="ConfigurationErrorsException">connectionString Attribute for SqlNicknameResolver in NicknameResolver-Config Xml-Node is empty.</exception>
		public override void ReadConfigSettings(XElement configElement)
		{
			XAttribute connectionStringAttribute = configElement.Attribute("connectionString");
			if (connectionStringAttribute == null)
				throw new ConfigurationErrorsException("Could not find connectionString Attribute for SqlNicknameResolver in NicknameResolver-Config Xml-Node.", (Exception) null);

			if (connectionStringAttribute.Value.IsNullOrTimmedEmpty())
				throw new ConfigurationErrorsException("connectionString Attribute for SqlNicknameResolver in NicknameResolver-Config Xml-Node is empty.", (Exception)null);

			try
			{
				new SqlConnectionStringBuilder(connectionStringAttribute.Value.Trim());
			}
			catch (Exception ex)
			{
				throw new ConfigurationErrorsException("connectionString Attribute for SqlNicknameResolver in NicknameResolver-Config Xml-Node has a wrong format.", ex);
			}

			ConnectionManager = new ConnectionManager(connectionStringAttribute.Value.Trim() , false);
			PlayerAdapter = new PlayerAdapter(ConnectionManager);
		}
	}
}
