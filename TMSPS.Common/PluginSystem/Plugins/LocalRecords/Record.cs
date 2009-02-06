using System;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords
{
	public class Record : IRecordSerializable
	{
		#region Properties

        public int? ID { get; private set; }
		public DateTime Created { get; private set; }
		public DateTime? LastChanged { get; private set; }
		public int ChallengeID { get; set; }
		public int PlayerID { get; set; }
		public int TimeOrScore { get; set; }

		#endregion

		#region IRecordSerializable Members

        int? IRecordSerializable.ID
		{
			get { return ID; }
			set { ID = value; }
		}

		DateTime IRecordSerializable.Created
		{
			get { return Created; }
			set { Created = value; }
		}

		DateTime? IRecordSerializable.LastChanged
		{
			get { return LastChanged; }
			set { LastChanged = value; }
		}

		#endregion
	}

	public interface IRecordSerializable
	{
        int? ID { get; set; }
		DateTime Created { get; set; }
		DateTime? LastChanged { get; set; }
	}
}