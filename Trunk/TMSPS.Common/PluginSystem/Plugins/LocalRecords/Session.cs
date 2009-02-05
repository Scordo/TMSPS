using System;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords
{
	public class Session : ISessionSerializable
	{
		#region Properties

		public int ID { get; private set; }
		public DateTime Created { get; private set; }
		public int ChallengeID { get; set; }
		public int PlayerID { get; set; }
		public int TimeOrScore { get; set; }

		#endregion

		#region ISessionSerializable Members

		int ISessionSerializable.ID
		{
			get { return ID; }
			set { ID = value; }
		}

		DateTime ISessionSerializable.Created
		{
			get { return Created; }
			set { Created = value; }
		}

		#endregion
	}

	public interface ISessionSerializable
	{
		int ID { get; set; }
		DateTime Created { get; set; }
	}
}
