using System;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords
{
	public class Player : IPlayerSerializable
	{
		#region Properties

        public int? ID { get; private set; }
		public string Login { get; set; }
		public string Nickname { get; set; }
		public DateTime Created { get; private set; }
		public DateTime? LastChanged { get; private set; }
		public DateTime LastTimePlayedChanged { get; private set; }
		public int Wins { get; set; }
		public long TimePlayed { get; set; }

		#endregion

		#region IPlayerSerializable Members

        int? IPlayerSerializable.ID
		{
			get { return ID; }
			set { ID = value; }
		}

		DateTime IPlayerSerializable.Created
		{
			get { return Created; }
			set { Created = value; }
		}

		DateTime? IPlayerSerializable.LastChanged
		{
			get { return LastChanged; }
			set { LastChanged = value; }
		}

		DateTime IPlayerSerializable.LastTimePlayedChanged
		{
			get { return LastTimePlayedChanged; }
			set { LastTimePlayedChanged = value; }
		}

		#endregion
	}

	public interface IPlayerSerializable
	{
        int? ID { get; set; }
		DateTime Created { get; set; }
		DateTime? LastChanged { get; set; }
		DateTime LastTimePlayedChanged { get; set; }
	}
}