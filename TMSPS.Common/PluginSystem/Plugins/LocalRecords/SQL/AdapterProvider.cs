using System;
using TMSPS.Core.Common;
using TMSPS.Core.SQL;

namespace TMSPS.Core.PluginSystem.Plugins.LocalRecords.SQL
{
    public class AdapterProvider : IAdapterProvider
    {
        #region Members

        private ConnectionManager _connectionManager;

        #endregion

        #region Constructor

        public AdapterProvider() : this(null)
        {
            
        }

        public AdapterProvider(ConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }

        #endregion

        #region IAdapterProvider Members

        public IChallengeAdapter GetChallengeAdapter()
        {
            return new ChallengeAdapter(_connectionManager);
        }

        public IChallengeAdapter GetChallengeAdapter(IBaseAdapter adapterToCopyContextFrom)
        {
            IChallengeAdapter adapter = new ChallengeAdapter();
            adapter.SetContextFrom(adapterToCopyContextFrom);

            return adapter;
        }

        public IPlayerAdapter GetPlayerAdapter()
        {
            return new PlayerAdapter(_connectionManager);
        }

        public IPlayerAdapter GetPlayerAdapter(IBaseAdapter adapterToCopyContextFrom)
        {
            IPlayerAdapter adapter = new PlayerAdapter();
            adapter.SetContextFrom(adapterToCopyContextFrom);

            return adapter;
        }

        public IPositionAdapter GetPositionAdapter()
        {
            return new PositionAdapter(_connectionManager);
        }

        public IPositionAdapter GetPositionAdapter(IBaseAdapter adapterToCopyContextFrom)
        {
            IPositionAdapter adapter = new PositionAdapter();
            adapter.SetContextFrom(adapterToCopyContextFrom);

            return adapter;
        }

        public IRankAdapter GetRankAdapter()
        {
            return new RankAdapter(_connectionManager);
        }

        public IRankAdapter GetRankAdapter(IBaseAdapter adapterToCopyContextFrom)
        {
            IRankAdapter adapter = new RankAdapter();
            adapter.SetContextFrom(adapterToCopyContextFrom);

            return adapter;
        }

        public IRatingAdapter GetRatingAdapter()
        {
            return new RatingAdapter(_connectionManager);
        }

        public IRatingAdapter GetRatingAdapter(IBaseAdapter adapterToCopyContextFrom)
        {
            IRatingAdapter adapter = new RatingAdapter();
            adapter.SetContextFrom(adapterToCopyContextFrom);

            return adapter;
        }

        public IRecordAdapter GetRecordAdapter()
        {
            return new RecordAdapter(_connectionManager);
        }

        public IRecordAdapter GetRecordAdapter(IBaseAdapter adapterToCopyContextFrom)
        {
            IRecordAdapter adapter = new RecordAdapter();
            adapter.SetContextFrom(adapterToCopyContextFrom);

            return adapter;
        }

        public ISessionAdapter GetSessionAdapter()
        {
            return new SessionAdapter(_connectionManager);
        }

        public ISessionAdapter GetSessionAdapter(IBaseAdapter adapterToCopyContextFrom)
        {
            ISessionAdapter adapter = new SessionAdapter();
            adapter.SetContextFrom(adapterToCopyContextFrom);

            return adapter;
        }

        void IAdapterProvider.Init(string parameter)
        {
            // parameter is the connectionstring

            if (parameter == null)
                throw new ArgumentNullException();

            _connectionManager = new ConnectionManager(parameter, false);
        }

        #endregion
    }
}