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
		public uint Wins { get; private set; }
		public TimeSpan TimePlayed { get; private set; }

		#endregion

	    #region Constructors

        public Player() : this(null, null)
        {
            
        }

        public Player(string login, string nickname)
        {
            Login = login;
            Nickname = nickname;
        }

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

	    uint IPlayerSerializable.Wins
	    {
	        get { return Wins; }
	        set { Wins = value; }
	    }

	    TimeSpan IPlayerSerializable.TimePlayed
	    {
	        get { return TimePlayed; }
	        set { TimePlayed = value; }
	    }

	    #endregion
	}

    public class IndexedPlayer : Player, IIndexedPlayerSerializable
    {
        public int RowNumber { get; private set; }

        #region IIndexedPlayerSerializable Members

        int IIndexedPlayerSerializable.RowNumber
        {
            get { return RowNumber; }
            set { RowNumber = value; }
        }

        #endregion
    }

    public interface IPlayerSerializable
    {
        int? ID { get; set; }
        DateTime Created { get; set; }
        DateTime? LastChanged { get; set; }
        DateTime LastTimePlayedChanged { get; set; }
        uint Wins { get; set; }
        TimeSpan TimePlayed { get; set; }
    }

    public interface IIndexedPlayerSerializable
    {
        int RowNumber { get; set; }
    }
}