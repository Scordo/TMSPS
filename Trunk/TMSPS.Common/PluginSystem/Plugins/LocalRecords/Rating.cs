using System;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords
{
	public class Rating : IRatingSerializable
	{
		#region Properties

        public int? ID { get; private set; }
		public DateTime Created { get; private set; }
		public ushort Value { get; set; }
		public int PlayerID { get; set; }
		public int ChallengeID { get; set; }

		#endregion

		#region IRatingSerializable Members

        int? IRatingSerializable.ID
		{
			get { return ID; }
			set { ID = value; }
		}

		DateTime IRatingSerializable.Created
		{
			get { return Created; }
			set { Created = value; }
		}

		#endregion
	}

	public interface IRatingSerializable
	{
        int? ID { get; set; }
		DateTime Created { get; set; }
	}
}
