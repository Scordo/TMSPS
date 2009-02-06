using System;
using TMSPS.Core.Common;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords
{
	public class LocalRecordsPlugin : TMSPSPluginBase
	{
		public override Version Version
		{
			get { return new Version("1.0.0.0"); }
		}

		public override string Author
		{
			get { return "Jens Hofmann"; }
		}

		public override string Name
		{
			get { return "Local Records Plugin"; }
		}

		public override string Description
		{
			get { return "Saves records and statistics in a local database."; }
		}

		public override string ShortNameForLogging
		{
			get { return "LocalRecords"; }
		}

		private IAdapterProvider AdapterProvider { get; set; }
		private IChallengeAdapter ChallengeAdapter { get; set;}
		private int CurrentChallengeID { get; set; }

		protected override void Init()
		{
			try
			{
				AdapterProvider = AdapterProviderFactory.GetAdapterProvider(Util.GetCalculatedPath("LocalRecords.xml"));
				ChallengeAdapter = AdapterProvider.GetChallengeAdapter();
			}
			catch (Exception ex)
			{
				Logger.Error("Error initializing AdapterProvider for local records.", ex);
				Logger.ErrorToUI(string.Format("An error occured. {0} not started!", Name));
				return;
			}
			
			Context.RPCClient.Callbacks.BeginRace += Callbacks_BeginRace;
		}

		private void Callbacks_BeginRace(object sender, Communication.EventArguments.Callbacks.BeginRaceEventArgs e)
		{
			Challenge challenge = new Challenge(e.ChallengeInfo.UId, e.ChallengeInfo.Name, e.ChallengeInfo.Author, e.ChallengeInfo.Environnement);
			ChallengeAdapter.IncreaseRaces(challenge);
			CurrentChallengeID = challenge.ID.Value;
		}

		protected override void Dispose()
		{
			Context.RPCClient.Callbacks.BeginRace -= Callbacks_BeginRace;
		}
	}
}
