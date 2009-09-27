using System;
using TMSPS.Core.Common;
using TMSPS.Core.PluginSystem.Plugins.LocalRecords;

namespace TMSPS.SQLite
{
    public class AdapterProvider : IAdapterProvider
    {
        #region Members

        private ConnectionManager _connectionManager;

        #endregion

        #region Constructor

        public AdapterProvider()
            : this(null)
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
            return new ChallengeAdapter(DetermineConnectionManager(_connectionManager));
        }

        public IChallengeAdapter GetChallengeAdapter(IBaseAdapter adapterToCopyContextFrom)
        {
            IChallengeAdapter adapter = new ChallengeAdapter();
            adapter.SetContextFrom(adapterToCopyContextFrom);

            return adapter;
        }

        public IPlayerAdapter GetPlayerAdapter()
        {
            return new PlayerAdapter(DetermineConnectionManager(_connectionManager));
        }

        public IPlayerAdapter GetPlayerAdapter(IBaseAdapter adapterToCopyContextFrom)
        {
            IPlayerAdapter adapter = new PlayerAdapter();
            adapter.SetContextFrom(adapterToCopyContextFrom);

            return adapter;
        }

        public IPositionAdapter GetPositionAdapter()
        {
            return new PositionAdapter(DetermineConnectionManager(_connectionManager));
        }

        public IPositionAdapter GetPositionAdapter(IBaseAdapter adapterToCopyContextFrom)
        {
            IPositionAdapter adapter = new PositionAdapter();
            adapter.SetContextFrom(adapterToCopyContextFrom);

            return adapter;
        }

        public IRankingAdapter GetRankingAdapter()
        {
            return new RankingAdapter(DetermineConnectionManager(_connectionManager));
        }

        public IRankingAdapter GetRankingAdapter(IBaseAdapter adapterToCopyContextFrom)
        {
            IRankingAdapter adapter = new RankingAdapter();
            adapter.SetContextFrom(adapterToCopyContextFrom);

            return adapter;
        }

        public IRatingAdapter GetRatingAdapter()
        {
            return new RatingAdapter(DetermineConnectionManager(_connectionManager));
        }

        public IRatingAdapter GetRatingAdapter(IBaseAdapter adapterToCopyContextFrom)
        {
            IRatingAdapter adapter = new RatingAdapter();
            adapter.SetContextFrom(adapterToCopyContextFrom);

            return adapter;
        }

        public IRecordAdapter GetRecordAdapter()
        {
            return new RecordAdapter(DetermineConnectionManager(_connectionManager));
        }

        public IRecordAdapter GetRecordAdapter(IBaseAdapter adapterToCopyContextFrom)
        {
            IRecordAdapter adapter = new RecordAdapter();
            adapter.SetContextFrom(adapterToCopyContextFrom);

            return adapter;
        }

        public ISessionAdapter GetSessionAdapter()
        {
            return new SessionAdapter(DetermineConnectionManager(_connectionManager));
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

        #region Non Public Methods

        public ConnectionManager DetermineConnectionManager(ConnectionManager connectionManager)
        {
            return connectionManager == null ? new ConnectionManager() : _connectionManager.Clone();
        }

        #endregion

        #endregion
    }
}
