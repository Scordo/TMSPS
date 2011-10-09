using System;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords.Entities;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords
{
	public class Player
	{
		#region Properties

        public int? ID { get; private set; }
		public string Login { get; set; }
		public string Nickname { get; set; }
		public DateTime Created { get; private set; }
		public DateTime? LastChanged { get; set; }
		public DateTime LastTimePlayedChanged { get; private set; }
		public uint Wins { get; set; }
		public TimeSpan TimePlayed { get; private set; }

		#endregion

	    #region Public Methods

        public void Assign(PlayerEntity player)
        {
            if (player == null)
                throw new ArgumentNullException("player");

            ID = player.Id;
            Login = player.Login;
            Nickname = player.Nickname;
            Created = player.Created;
            LastChanged = player.LastChanged;
            LastTimePlayedChanged = player.LastTimePlayedChanged;
            Wins = (uint)player.Wins;
            TimePlayed = TimeSpan.FromMilliseconds(player.TimePlayed);
        }

        public static Player FromPlayerEntity(PlayerEntity player)
        {
            if (player == null)
                return null;

            Player result = new Player();
            result.Assign(player);

            return result;
        }

	    #endregion
	}
}