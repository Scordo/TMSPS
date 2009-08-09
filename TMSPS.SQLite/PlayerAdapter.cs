using System;
using System.Collections.Generic;
using System.Data;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords;
using TMSPS.Core.SQL;

namespace TMSPS.SQLite
{
    public class PlayerAdapter : BaseAdapter, IPlayerAdapter
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerAdapter"/> class.
        /// </summary>
        public PlayerAdapter()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerAdapter"/> class.
        /// </summary>
        /// <param name="connectionManager">The connection manager.</param>
        public PlayerAdapter(ConnectionManager connectionManager) : base(connectionManager)
        {
        }

        #endregion

        #region IPlayerAdapter Members

        public void CreateOrUpdate(Player player)
        {
            if (player == null)
                throw new ArgumentNullException("player");

            if (player.Login.IsNullOrTimmedEmpty())
                throw new ArgumentException("Login is null or empty.");

            if (player.Nickname.IsNullOrTimmedEmpty())
                throw new ArgumentException("Nickname is null or empty.");

            int? playerID = GetPlayerID(player.Login);

            if (playerID.HasValue)
                UpdatePlayer(playerID.Value, player);
            else
                CreatePlayer(player);
        }

        public Player Deserialize(string login)
        {
            return SqlHelper.ExecuteClassQuery<Player>("Select * FROM [Player] WHERE [Login] = @login", PlayerFromDataRow, "login", login);
        }

        public uint IncreaseWins(string login)
        {
            if (login == null)
                throw new ArgumentNullException("login");

            return SqlHelper.ExecuteScalar<uint>("Update [Player] SET Wins = Wins + 1, LastChanged = CURRENT_TIMESTAMP where [Login] = @Login; Select [Wins] FROM [Player] WHERE [Login] = @Login", "Login", login);
        }

        public ulong? UpdateTimePlayed(string login)
        {
            Player player = Deserialize(login);

            if (player == null)
                return null;

            TimeSpan difference = DateTime.Now - player.LastTimePlayedChanged;
            TimeSpan timePlayed = player.TimePlayed;

            if (difference < TimeSpan.FromHours(12))
                timePlayed = timePlayed.Add(difference);

            ulong millisecondsPlayed = Convert.ToUInt64(timePlayed.TotalMilliseconds);

            SqlHelper.ExecuteNonQuery("Update [Player] SET TimePlayed = @TimePlayed, LastTimePlayedChanged = CURRENT_TIMESTAMP, LastChanged = CURRENT_TIMESTAMP where [Login] = @Login", "Login", login, "TimePlayed", millisecondsPlayed);

            return millisecondsPlayed;
        }

        public List<Player> DeserializeList(uint top, PlayerSortOrder sorting, bool ascending)
        {
            string selectStatement = string.Format("Select ROWID as RowNumber, * From [Player] order by {0} {1} Limit @top", sorting, ascending ? "asc" : "desc");
            return SqlHelper.ExecuteClassListQuery<Player>(selectStatement, IndexedPlayerFromDataRow, "top", (int) top);
        }

        public bool RemoveAllStatsForLogin(string login)
        {
            int? playerID = GetPlayerID(login);

            if (!playerID.HasValue)
                return false;

            SqlHelper.ExecuteNonQuery("DELETE Position where PlayerID = @playerID", "playerID", playerID.Value);
            SqlHelper.ExecuteNonQuery("DELETE Ranking where PlayerID = @playerID", "playerID", playerID.Value);
            SqlHelper.ExecuteNonQuery("DELETE Rating where PlayerID = @playerID", "playerID", playerID.Value);
            SqlHelper.ExecuteNonQuery("DELETE Record where PlayerID = @playerID", "playerID", playerID.Value);
            SqlHelper.ExecuteNonQuery("DELETE Session where PlayerID = @playerID", "playerID", playerID.Value);
            SqlHelper.ExecuteNonQuery("UPDATE Player set Wins = 0, TimePlayed = 0 where ID = @playerID", "playerID", playerID.Value);

            return true;
        }

        public PagedList<IndexedPlayer> DeserializeListByWins(int? startIndex, int? endIndex)
        {
            const string countStatement = "Select Count(*) From [Player]";
            const string selectStatement = "Select * From [Player] order by [Wins] desc";

            return SqlHelper.ExecutePagedClassListQuery<IndexedPlayer>(countStatement, selectStatement, IndexedPlayerFromDataRow, startIndex, endIndex);
        }

        #endregion

        #region Non Public Methods

        private Player Deserialize(int playerID)
        {
            return SqlHelper.ExecuteClassQuery<Player>("Select * FROM [Player] WHERE [ID] = @playerID", PlayerFromDataRow, "playerID", playerID);
        }

        private void CreatePlayer(Player player)
        {
            const string createCommand = "INSERT INTO [PLAYER] ([LOGIN], [Nickname], [Created], [LastTimePlayedChanged]) VALUES (@Login, @Nickname, @Created, @Created); select last_insert_rowid();";
            int playerID = SqlHelper.ExecuteScalar<int>(createCommand, "login", player.Login, "Nickname", player.Nickname, "Created", DateTime.Now);

            Player createdPlayer = Deserialize(playerID);
            player.Assign(createdPlayer);
        }

        private void UpdatePlayer(int playerID, Player player)
        {
            const string updateCommand = "UPDATE [PLAYER] SET [Nickname] = @Nickname, [LastTimePlayedChanged] = @CurrentDate, [LastChanged] = @CurrentDate WHERE [ID] = @ID";

            DateTime now = DateTime.Now;
            SqlHelper.ExecuteNonQuery(updateCommand, "CurrentDate", now, "Nickname", player.Nickname, "ID", playerID);

            IPlayerSerializable playerSerializable = player;
            playerSerializable.LastTimePlayedChanged = now;
            playerSerializable.LastChanged = now;
        }

        private static Player PlayerFromDataRow(DataRow row)
        {
            Player player = new Player();
            AssignPlayerFromDataRow(player, row);

            return player;
        }

        private static IndexedPlayer IndexedPlayerFromDataRow(DataRow row)
        {
            IndexedPlayer player = new IndexedPlayer();
            AssignPlayerFromDataRow(player, row);
            IIndexedPlayerSerializable indexedPlayerSerializable = player;
            indexedPlayerSerializable.RowNumber = Convert.ToInt32(row["RowNumber"]);

            return player;
        }

        private static void AssignPlayerFromDataRow(Player player, DataRow row)
        {
            player.Nickname = Convert.ToString(row["Nickname"]);
            player.Login = Convert.ToString(row["Login"]);

            IPlayerSerializable playerSerializable = player;
            playerSerializable.ID = Convert.ToInt32(row["ID"]);
            playerSerializable.Created = Convert.ToDateTime(row["Created"]);
            playerSerializable.LastChanged = row["LastChanged"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(row["LastChanged"]);
            playerSerializable.LastTimePlayedChanged = Convert.ToDateTime(row["LastTimePlayedChanged"]);
            playerSerializable.Wins = Convert.ToUInt32(row["Wins"]);
            playerSerializable.TimePlayed = TimeSpan.FromMilliseconds(Convert.ToInt64(row["TimePlayed"]));
        }

        #endregion
    }
}
