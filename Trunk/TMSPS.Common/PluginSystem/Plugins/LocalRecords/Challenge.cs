using System;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords
{
	public class Challenge : IChallengeSerializable
	{
		#region Properties

		public int ID { get; private set; }
		public DateTime Created { get; private set; }
		public DateTime LastChanged { get; private set; }
		public string UniqueID { get; set; }
		public string Name { get; set; }
		public string Author { get; set; }
		public string Environment { get; set; }
		public int Races { get; set; }

		#endregion

		#region IChallengeSerializable Members

		int IChallengeSerializable.ID
		{
			get { return ID; }
			set { ID = value; }
		}

		DateTime IChallengeSerializable.Created
		{
			get { return Created; }
			set { Created = value; }
		}

		DateTime IChallengeSerializable.LastChanged
		{
			get { return LastChanged; }
			set { LastChanged = value; }
		}

		#endregion
	}

	public interface IChallengeSerializable
	{
		int ID { get; set; }
		DateTime Created { get; set; }
		DateTime LastChanged { get; set; }
	}
}