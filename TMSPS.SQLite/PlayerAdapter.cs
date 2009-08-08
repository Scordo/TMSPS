using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords;

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
            throw new NotImplementedException();
        }

        public ulong? UpdateTimePlayed(string login)
        {
            throw new NotImplementedException();
        }

        public List<Player> DeserializeList(uint top, PlayerSortOrder sorting, bool ascending)
        {
            throw new NotImplementedException();
        }

        public bool RemoveAllStatsForLogin(string login)
        {
            throw new NotImplementedException();
        }

        public Core.SQL.PagedList<IndexedPlayer> DeserializeListByWins(int? startIndex, int? endIndex)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Non Public Methods

        private int? GetPlayerID(string login)
        {
            return SqlHelper.ExecuteScalar<int?>("Select [ID] FROM [Player] WHERE [Login] = @Login", "login", login);
        }

        private Player Deserialize(int playerID)
        {
            return SqlHelper.ExecuteClassQuery<Player>("Select * FROM [Player] WHERE [ID] = @playerID", PlayerFromDataRow, "playerID", playerID);
        }

        private void CreatePlayer(Player player)
        {
            const string createCommand = "INSERT INTO [PLAYER] ([LOGIN], [Nickname]) VALUES (@Login, @Nickname); select last_insert_rowid();";
            int playerID = SqlHelper.ExecuteScalar<int>(createCommand, "login", player.Login, "Nickname", player.Nickname);

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
