using System;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords
{
	public class Position : IPositionSerializable
	{
		#region Properties

        public int? ID { get; private set; }
		public DateTime Created { get; private set; }
		public int PlayerID { get; set; }
		public int ChallengeID { get; set; }
		public int OwnPosition { get; set; }
		public int MaxPosition { get; set; }

		#endregion

		#region IPositionSerializable Members

        int? IPositionSerializable.ID
		{
			get { return ID; }
			set { ID = value; }
		}

		DateTime IPositionSerializable.Created
		{
			get { return Created; }
			set { Created = value; }
		}

		#endregion
	}

	public interface IPositionSerializable
	{
        int? ID { get; set; }
		DateTime Created { get; set; }
	}
}