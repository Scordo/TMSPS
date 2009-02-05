using System;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords
{
	public class Rank : IRankSerializable
	{
		#region Properties

		public int ID { get; private set; }
		public DateTime Created { get; private set; }
		public DateTime LastChanged { get; private set; }
		public int PlayerID { get; set; }
		public double AveragePosition { get; set; }
		public double AverageRecord { get; set; }

		#endregion

		#region IRankSerializable Members

		int IRankSerializable.ID
		{
			get { return ID; }
			set { ID = value; }
		}

		DateTime IRankSerializable.Created
		{
			get { return Created; }
			set { Created = value; }
		}

		DateTime IRankSerializable.LastChanged
		{
			get { return LastChanged; }
			set { LastChanged = value; }
		}

		#endregion
	}

	public interface IRankSerializable
	{
		int ID { get; set; }
		DateTime Created { get; set; }
		DateTime LastChanged { get; set; }
	}
}