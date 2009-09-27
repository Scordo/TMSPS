using System;
using TMSPS.Core.Common;

namespace TMSPS.SQLite
{
    /// <summary>
    /// Serves as the base class for all Adapters
    /// </summary>
    public abstract class BaseAdapter : IBaseAdapter
    {
        #region Non Public Members

        private ConnectionManager _connectionManager;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the connection manager.
        /// </summary>
        /// <value>The connection manager.</value>
        protected ConnectionManager ConnectionManager
        {
            get
            {
                lock (typeof(BaseAdapter))
                {
                    if (_connectionManager == null)
                    {
                        _connectionManager = ConnectionManager.NewInstance;
                    }

                    return _connectionManager;
                }
            }
            private set { _connectionManager = value; }
        }

        /// <summary>
        /// Gets the SQL helper.
        /// </summary>
        /// <value>The SQL helper.</value>
        protected SqlHelper SqlHelper
        {
            get
            {
                return ConnectionManager.SqlHelper;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseAdapter"/> class.
        /// </summary>
        protected BaseAdapter()
            : this(null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseAdapter"/> class.
        /// </summary>
        /// <param name="connectionManager">The connection manager.</param>
        protected BaseAdapter(ConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }

        #endregion

        #region Destructor

        ~BaseAdapter()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }

        #endregion

        #region IBaseAdapter Members

        /// <summary>
        /// Starts a transaction.
        /// </summary>
        /// <returns></returns>
        public bool BeginTransaction()
        {
            return ConnectionManager.BeginTransaction();
        }

        /// <summary>
        /// Commits a running transaction.
        /// </summary>
        public void CommitTransaction()
        {
            ConnectionManager.CommitTransaction();
        }

        /// <summary>
        /// Commits a running transaction if the provided parameter is set to false.
        /// </summary>
        /// <param name="transactionExisted">if set to <c>true</c> [transaction existed].</param>
        public void CommitTransaction(bool transactionExisted)
        {
            ConnectionManager.CommitTransaction(transactionExisted);
        }

        /// <summary>
        /// Rollbacks a running transaction.
        /// </summary>
        public void RollbackTransaction()
        {
            ConnectionManager.RollbackTransaction();
        }

        /// <summary>
        /// Rollbacks a running transaction if the provided parameter is set to false.
        /// </summary>
        /// <param name="transactionExisted">if set to <c>true</c> [transaction existed].</param>
        public void RollbackTransaction(bool transactionExisted)
        {
            ConnectionManager.RollbackTransaction(transactionExisted);
        }

        /// <summary>
        /// Executes the provided logic in an transaction and conditionally does a rollback and commit
        /// </summary>
        /// <param name="delLogic">The logic to execute in an transaction</param>
        public void RunInTransaction(Action delLogic)
        {
            ConnectionManager.RunInTransaction(delLogic);
        }

        /// <summary>
        /// Copies the context from <paramref name="adapter"/> to the current instance. A context is for example a connection for db-adapter or something simmilar.
        /// </summary>
        /// <param name="adapter">The adapter.</param>
        public void SetContextFrom(IBaseAdapter adapter)
        {
            if (adapter == null)
                throw new ArgumentNullException("adapter");

            if (!(adapter is BaseAdapter))
                throw new ArgumentException(string.Format("The provided adapter of type '{0}' does not derive from type '{1}'", adapter.GetType().FullName, typeof(BaseAdapter).FullName));

            BaseAdapter sourceAdapter = (BaseAdapter)adapter;
            ConnectionManager = sourceAdapter.ConnectionManager;
        }


        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Non Public Methods

        protected int? GetPlayerID(string login)
        {
            return SqlHelper.ExecuteScalar<int?>("Select [ID] FROM [Player] WHERE [Login] = @Login", "login", login);
        }

        protected int? GetChallengeID(string uniqueID)
        {
            return SqlHelper.ExecuteScalar<int?>("Select ID FROM [Challenge] WHERE UniqueID = @UniqueID", "UniqueID", uniqueID);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            // free managed resources
            if (ConnectionManager != null)
                ConnectionManager.Dispose();
        }

        #endregion

    }
}