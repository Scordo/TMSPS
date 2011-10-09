using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords.Entities;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords.Repositories;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords.Repositories.Interfaces;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords
{
    public class PlayerCache
    {
        #region Fields

        private readonly Dictionary<string, PlayerEntity> _loginsPlayersDictionary;
        private readonly Dictionary<int, PlayerEntity> _idsPlayersDictionary;
        private readonly Dictionary<int, DateTime> _lastAccessDictionary;
        private readonly object _lock = new object();

        #endregion

        #region Properties

        public static PlayerCache Instance { get; private set; }
        public uint Capacity { get; private set; }

        #endregion

        #region Constructor

        private PlayerCache()
        {
            _loginsPlayersDictionary = new Dictionary<string, PlayerEntity>();
            _idsPlayersDictionary = new Dictionary<int, PlayerEntity>();
            _lastAccessDictionary = new Dictionary<int, DateTime>();
        }

        #endregion

        #region Public Methods

        public static void Init(uint capacity)
        {
            if (Instance == null)
                Instance = new PlayerCache {Capacity = capacity};
        }

        public PlayerEntity Get(string login)
        {
            return  AccessLock(() =>
            {
                if (_loginsPlayersDictionary.ContainsKey(login))
                {
                    PlayerEntity player = _loginsPlayersDictionary[login];
                    _lastAccessDictionary[player.Id.Value] = DateTime.Now;
                    return player.Clone();
                }


                IPlayerRepository playerRepository = RepositoryFactory.Get<IPlayerRepository>();
                PlayerEntity loadedPlayer = playerRepository.Get(login);

                if (loadedPlayer != null)
                    AddPlayerInternal(loadedPlayer);

                return loadedPlayer;
            });
        }

        public PlayerEntity Get(int id)
        {
            return AccessLock(() =>
            {
                if (_idsPlayersDictionary.ContainsKey(id))
                {
                    PlayerEntity player = _idsPlayersDictionary[id];
                    _lastAccessDictionary[player.Id.Value] = DateTime.Now;
                    return player.Clone();
                }


                IPlayerRepository playerRepository = RepositoryFactory.Get<IPlayerRepository>();
                PlayerEntity loadedPlayer = playerRepository.Get(id);

                if (loadedPlayer != null)
                    AddPlayerInternal(loadedPlayer);

                return loadedPlayer;
            });
        }

        public bool Add(PlayerEntity player, bool allowUpdate = false)
        {
            if (player == null)
                throw new ArgumentNullException("player");

            if (!player.Id.HasValue)
                throw new ArgumentException("Provided player does not have an id!", "player");

            if (player.Login.IsNullOrTimmedEmpty())
                throw new ArgumentException("Provided player does not have a login!", "player");


            bool bolAdded = false;
            AccessLock(() =>
            {
                if (_idsPlayersDictionary.ContainsKey(player.Id.Value) && !allowUpdate)
                {
                    _lastAccessDictionary[player.Id.Value] = DateTime.Now;
                    return;
                }

                AddPlayerInternal(player);
                EnsureCapacityLimit();

                bolAdded = true;
            });

            return bolAdded;
        }

        public void AddRange(IEnumerable<PlayerEntity> players, bool allowUpdate = false)
        {
            if (players == null)
                throw new ArgumentNullException("players");

            bool bolAdded = false;
            AccessLock(() =>
            {
                foreach (PlayerEntity player in players)
                {
                    if (!player.Id.HasValue)
                        throw new InvalidOperationException("Provided player does not have an id!");

                    if (player.Login.IsNullOrTimmedEmpty())
                        throw new InvalidOperationException("Provided player does not have a login!");

                    if (_idsPlayersDictionary.ContainsKey(player.Id.Value) && !allowUpdate)
                    {
                        _lastAccessDictionary[player.Id.Value] = DateTime.Now;
                        continue;
                    }

                    AddPlayerInternal(player);
                    EnsureCapacityLimit();

                    bolAdded = true;
                }
            });
        }

        public PlayerEntity EnsureExists(string login, string nickname)
        {
            if (login.IsNullOrTimmedEmpty())
                throw new ArgumentException("Provided login is empty!", "login");

            if (nickname.IsNullOrTimmedEmpty())
                throw new ArgumentException("Provided nickname is empty!", "nickname");

            return AccessLock(() =>
            {
                PlayerEntity loadedPlayer;
                if (_loginsPlayersDictionary.ContainsKey(login))
                {
                    loadedPlayer = _loginsPlayersDictionary[login];

                    if (loadedPlayer.Nickname.Equals(nickname, StringComparison.InvariantCulture))
                    {
                        _lastAccessDictionary[loadedPlayer.Id.Value] = DateTime.Now;
                        return loadedPlayer.Clone();
                    }
                }


                IPlayerRepository playerRepository = RepositoryFactory.Get<IPlayerRepository>();
                loadedPlayer = playerRepository.EnsurePlayerExistsAndUptodate(login, nickname); 

                AddPlayerInternal(loadedPlayer);
                EnsureCapacityLimit();

                return loadedPlayer.Clone();
            });
        }

        #endregion

        #region Non Public Methods

        private void AddPlayerInternal(PlayerEntity player)
        {
            PlayerEntity clonedPlayer = player.Clone();
            _loginsPlayersDictionary[player.Login] = clonedPlayer;
            _idsPlayersDictionary[player.Id.Value] = clonedPlayer;
            _lastAccessDictionary[player.Id.Value] = DateTime.Now;
        }

        private void EnsureCapacityLimit()
        {
            long diff = Capacity - _lastAccessDictionary.Count;

            if (diff > 0)
                return;

            diff = Math.Abs(diff);

            List<PlayerEntity> toBeDeleted = _lastAccessDictionary.OrderBy(kv => kv.Value).Take((int)diff).Select(kv => _idsPlayersDictionary[kv.Key]).ToList();
           
            toBeDeleted.ForEach((player) =>
            {
                _loginsPlayersDictionary.Remove(player.Login);
                _idsPlayersDictionary.Remove(player.Id.Value);
                _lastAccessDictionary.Remove(player.Id.Value);                   
            });
        }

        private void AccessLock(Action action)
        {
            lock (_lock)
            {
                action();
            }
        }

        private TResult AccessLock<TResult>(Func<TResult> func)
        {
            lock (_lock)
            {
                return func();
            }
        }

        #endregion
    }
}
