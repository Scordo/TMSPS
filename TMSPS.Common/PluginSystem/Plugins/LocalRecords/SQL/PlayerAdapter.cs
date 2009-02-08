using System;
using System.Collections.Generic;
using System.Data;
using TMSPS.Core.SQL;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords.SQL
{
    public class PlayerAdapter : SQLBaseAdapter, IPlayerAdapter
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

        #region Public Methods

        public void CreateOrUpdate(Player player)
        {
            if (player == null)
                throw new ArgumentNullException("player");

            if (player.Login.IsNullOrTimmedEmpty())
                throw new ArgumentException("Login is null or empty.");

            if (player.Nickname.IsNullOrTimmedEmpty())
                throw new ArgumentException("Nickname is null or empty.");

            if (player.Nickname.IsNullOrTimmedEmpty())
                throw new ArgumentException("Nickname is null or empty.");

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"Login", player.Login.Trim()},
                {"Nickname", player.Nickname.Trim()}
            };

            DataTable resultTable = SqlHelper.ExecuteDataTable("Player_CreateOrUpdate", parameters);

            if (resultTable.Rows.Count > 0)
            {
                IPlayerSerializable playerSerializable = player;
                DataRow row = resultTable.Rows[0];

                playerSerializable.ID = Convert.ToInt32(row["ID"]);
                playerSerializable.Created = Convert.ToDateTime(row["Created"]);
                playerSerializable.LastChanged = row["LastChanged"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(row["LastChanged"]);
                playerSerializable.LastTimePlayedChanged = Convert.ToDateTime(row["LastTimePlayedChanged"]);
                playerSerializable.Wins = Convert.ToUInt32(row["Wins"]);
                playerSerializable.TimePlayed = TimeSpan.FromMilliseconds(Convert.ToInt64(row["TimePlayed"]));
            }
        }

        public uint IncreaseWins(string login)
        {
            if (login == null)
                throw new ArgumentNullException("login");

            return SqlHelper.ExecuteScalar<uint>("Player_IncreaseWins", "Login", login);
        }

        public ulong UpdateTimePlayed(string login)
        {
            if (login == null)
                throw new ArgumentNullException("login");

            return SqlHelper.ExecuteScalar<ulong>("Player_UpdateTimePlayed", "Login", login);
        }

        #endregion

    }
}
