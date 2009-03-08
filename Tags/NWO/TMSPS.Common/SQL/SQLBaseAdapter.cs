using System;
using TMSPS.Core.Common;

namespace TMSPS.Core.SQL
{
    /// <summary>
    /// Serves as the base class for all Adapters
    /// </summary>
    public abstract class SQLBaseAdapter : IBaseAdapter
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
                lock (typeof(SQLBaseAdapter))
                {
                    if (_connectionManager == null)
                    {
                        _connectionManager = SQL.ConnectionManager.NewInstance;
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
        /// Initializes a new instance of the <see cref="SQLBaseAdapter"/> class.
        /// </summary>
        protected SQLBaseAdapter() : this(null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SQLBaseAdapter"/> class.
        /// </summary>
        /// <param name="connectionManager">The connection manager.</param>
        protected SQLBaseAdapter(ConnectionManager connectionManager) 
        {
            _connectionManager = connectionManager;
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
        public void RunInTransaction(ParameterlessMethodDelegate delLogic)
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

            if (!(adapter is SQLBaseAdapter))
                throw new ArgumentException(string.Format("The provided adapter of type '{0}' does not derive from type '{1}'", adapter.GetType().FullName,  typeof(SQLBaseAdapter).FullName));

            SQLBaseAdapter sourceAdapter = (SQLBaseAdapter) adapter;
            ConnectionManager = sourceAdapter.ConnectionManager;
        }

        #endregion
    }
}