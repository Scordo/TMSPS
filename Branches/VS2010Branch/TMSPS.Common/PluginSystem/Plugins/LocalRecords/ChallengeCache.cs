using System;
using System.Collections.Generic;
using NHibernate;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords.Entities;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords.Repositories;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords.Repositories.Interfaces;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords
{
    public class ChallengeCache
    {
        #region Fields

        private readonly Dictionary<string, ChallengeEntity> _uidChallengeDictionary;
        private readonly object _lock = new object();

        #endregion

        #region Properties

        public static ChallengeCache Instance { get; private set; }
        
        #endregion

        #region Constructor

        private ChallengeCache()
        {
            _uidChallengeDictionary = new Dictionary<string, ChallengeEntity>();
        }

        #endregion

        #region Public Methods

        public static void Init()
        {
            Instance = new ChallengeCache();
        }

        public ChallengeEntity Get(string uid)
        {
            if (uid.IsNullOrTimmedEmpty())
                throw new ArgumentException("uid is empty!", "uid");

            return  AccessLock(() =>
            {
                if (_uidChallengeDictionary.ContainsKey(uid))
                    return _uidChallengeDictionary[uid].Clone();


                IChallengeRepository challengeRepository = RepositoryFactory.Get<IChallengeRepository>();
                ChallengeEntity loadedChallenge = challengeRepository.Get(uid);

                if (loadedChallenge != null)
                    _uidChallengeDictionary[uid] = loadedChallenge.Clone();

                return loadedChallenge;
            });
        }


        public bool Add(ChallengeEntity challenge, bool allowUpdate = false)
        {
            if (challenge == null)
                throw new ArgumentNullException("challenge");

            if (challenge.UniqueId.IsNullOrTimmedEmpty())
                throw new ArgumentException("Provided challenge has empty uid!", "challenge");


            bool bolAdded = false;
            AccessLock(() =>
            {
                if (_uidChallengeDictionary.ContainsKey(challenge.UniqueId) && !allowUpdate)
                    return;

                _uidChallengeDictionary[challenge.UniqueId] = challenge.Clone();

                bolAdded = true;
            });

            return bolAdded;
        }

        #endregion

        #region Non Public Methods

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
