using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Linq;
using TMSPS.Core.Common;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords.Entities;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords.Repositories.Interfaces;
using System.Linq;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords.Repositories
{
    public class PlayerRepository : RepositoryBase, IPlayerRepository
    {
        #region Properties

        private IPlayerRepository Interface
        {
            get { return this; }
        }

        #endregion

        #region Constructors

        public PlayerRepository()
        {

        }

        public PlayerRepository(ISession session) : base(session)
        {

        }

        #endregion

        #region IPlayerRepository

        PlayerEntity IPlayerRepository.EnsurePlayerExistsAndUptodate(string login, string nickname)
        {
            PlayerEntity player = Interface.Get(login);

            if (player == null)
            {
                player = new PlayerEntity{Login = login, Nickname = nickname};
                
                UseSession(session =>
                {
                    session.Save(player);
                    session.Flush();
                });

                PlayerCache.Add(player, true);

                return player;
            }

            if (!nickname.Equals(player.Nickname, StringComparison.InvariantCulture))
            {
                player.LastChanged = DateTime.Now;
                player.Nickname = nickname;
                
                UseSession(session =>
                {
                    session.Update(player);
                    session.Flush();
                });
            }

            PlayerCache.Add(player, true);

            return player;
        }

        uint IPlayerRepository.IncreaseWins(string login)
        {
            PlayerEntity player = Interface.Get(login);

            if (player == null)
                throw new ArgumentException("The provided login '{0}' does not exist!", login);

            player.Wins++;
            player.LastChanged = DateTime.Now;
            
            UseSession(session =>
            {
                session.Update(player);
                session.Flush();
            });

            PlayerCache.Add(player, true);

            return (uint)player.Wins;
        }

        void IPlayerRepository.SetLastTimePlayedChanged(PlayerEntity player, DateTime dateTime)
        {
            if (player == null)
                throw new ArgumentNullException("player");

            player.LastTimePlayedChanged = dateTime;
            player.LastChanged = dateTime;
            
            UseSession(session =>
            {
                session.Update(player);
                session.Flush();
            });

            PlayerCache.Add(player, true);
        }

        ulong IPlayerRepository.UpdateTimePlayed(string login)
        {
            PlayerEntity player = Interface.Get(login);

            if (player == null)
                throw new ArgumentException("The provided login '{0}' does not exist!", login);

            DateTime now = DateTime.Now;
            player.TimePlayed += (long) Math.Floor((now - player.LastTimePlayedChanged).TotalMilliseconds);
            player.LastChanged = now;
            player.LastTimePlayedChanged = now;
            
            UseSession(session =>
            {
                session.Update(player);
                session.Flush();
            });

            PlayerCache.Add(player, true);

            return (ulong)player.TimePlayed;
        }

        List<PlayerEntity> IPlayerRepository.GetList(uint top, PlayerSortOrder sorting, bool ascending)
        {
            List<PlayerEntity> result = null;

            UseSession(session =>
            {
                IQueryable<PlayerEntity> query = session.Query<PlayerEntity>();

                switch (sorting)
                {
                    case PlayerSortOrder.Wins:
                        query = ascending ? query.OrderBy(p => p.Wins) : query.OrderByDescending(p => p.Wins);
                        break;
                    case PlayerSortOrder.TimePlayed:
                        query = ascending ? query.OrderBy(p => p.TimePlayed) : query.OrderByDescending(p => p.TimePlayed);
                        break;
                }

                result = new List<PlayerEntity>(query.Take((int)top));
            });

            PlayerCache.AddRange(result, true);

            return result;
        }

        bool IPlayerRepository.DeleteData(string login)
        {
            PlayerEntity player = Interface.Get(login);

            if (player == null)
                return false;

            int playerId = player.Id.Value;

            UseSession(session =>
            {
                using (ITransaction transaction = session.BeginTransaction())
                {
                    session.CreateQuery("DELETE FROM LapResultEntity WHERE PlayerId = :PlayerID").SetInt32("PlayerID", playerId).ExecuteUpdate();
                    session.CreateQuery("DELETE FROM RecordEntity WHERE PlayerId = :PlayerID").SetInt32("PlayerID", playerId).ExecuteUpdate();
                    session.CreateQuery("DELETE FROM RatingEntity WHERE PlayerId = :PlayerID").SetInt32("PlayerID", playerId).ExecuteUpdate();
                    session.CreateQuery("DELETE FROM ChallengeRankEntity WHERE PlayerId = :PlayerID").SetInt32("PlayerID", playerId).ExecuteUpdate();
                    session.CreateQuery("DELETE FROM RaceResultEntity WHERE PlayerId = :PlayerID").SetInt32("PlayerID", playerId).ExecuteUpdate();

                    player.Wins = 0;
                    player.TimePlayed = 0;
                    player.LastTimePlayedChanged = DateTime.Now;
                    player.LastChanged = DateTime.Now;
                    session.Merge(player);

                    transaction.Commit();
                }                   
            });

            return true;
        }

        PlayerEntity IPlayerRepository.Get(string login)
        {
            PlayerEntity result = null;

            UseSession(session =>
            {
                result = session.Query<PlayerEntity>().Where(p => p.Login.Equals(login)).FirstOrDefault();
            });

            PlayerCache.Add(result, true);

            return result;
        }

        PlayerEntity IPlayerRepository.Get(int id)
        {
            PlayerEntity result = null;
            
            UseSession(session =>
            {
                result = session.Query<PlayerEntity>().Where(p => p.Id == id).FirstOrDefault();
            });

            PlayerCache.Add(result, true);

            return result;
        }

        PagedList<PlayerEntity> IPlayerRepository.GetListByWins(int? startIndex, int? endIndex)
        {
            startIndex = startIndex ?? 0;

            PagedList<PlayerEntity> result = null;
            UseSession(session =>
            {
                IQueryable<PlayerEntity> query = session.Query<PlayerEntity>().OrderByDescending(p => p.Wins);

                if (startIndex > 0)
                    query = query.Skip(startIndex.Value);

                if (endIndex.HasValue)
                    query = query.Take(endIndex.Value - startIndex.Value);

                result = new PagedList<PlayerEntity>(query) { StartIndex = startIndex.Value, VirtualCount = session.Query<PlayerEntity>().Count() };        
            });

            PlayerCache.AddRange(result, true);

            return result;
        }

        #endregion


        
    }
}