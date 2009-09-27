using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace TMSPS.Core.SQL
{
    /// <summary>
    /// This class is used to gain access to the Database. It handles Transaction and so on.
    /// </summary>
    public class ConnectionManager
    {
        #region Private Members

        private readonly string _connectionString;
        private SqlConnection _connection;
        private SqlTransaction _transaction;
        private SqlHelper _sqlHelper;
        private readonly bool _closeConnectionAfterCommandProcessing;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current active transaction.
        /// </summary>
        public SqlTransaction Transaction
        {
            get { return _transaction; }
        }

        /// <summary>
        /// Returns a new instance of the ConnectionManager using web.config settings
        /// </summary>
        public static ConnectionManager NewInstance
        {
            get { return new ConnectionManager(); }
        }

        /// <summary>
        /// Gets the connection string.
        /// </summary>
        /// <value>The connection string.</value>
        public string ConnectionString
        {
            get { return _connectionString; }
        }


        /// <summary>
        /// Gives access to the current connection
        /// </summary>
        /// <value>The connection.</value>
        public SqlConnection Connection
        {
            get
            {
                lock (this)
                {
                    if (_connection == null)
                    {
                        _connection = new SqlConnection(_connectionString);
                    }
                }

                return _connection;
            }
        }

        /// <summary>
        /// Gets a value indicating whether an open transaction exists.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if an open transaction exists, otherwise, <c>false</c>.
        /// </value>
        public bool OpenTransactionExists
        {
            get { return _transaction != null; }
        }

        /// <summary>
        /// Gives access to the SqlHelper, a class providing a lot of helper methods to query the database
        /// </summary>
        /// <value>The SQL helper.</value>
        public SqlHelper SqlHelper
        {
            get
            {
                if (_sqlHelper == null)
                    _sqlHelper = new SqlHelper(this, _closeConnectionAfterCommandProcessing);

                return _sqlHelper;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionManager"/> class.
        /// </summary>
        public ConnectionManager()
            : this(ConfigurationManager.ConnectionStrings["Default"].ConnectionString, true)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionManager"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="closeConnectionAfterCommandProcessing">if set to <c>true</c> the connection is closed after command processing.</param>
        public ConnectionManager(string connectionString, bool closeConnectionAfterCommandProcessing)
        {
            _connectionString = connectionString;
            _closeConnectionAfterCommandProcessing = closeConnectionAfterCommandProcessing;
        }

        #endregion

        #region Destructor

        ~ConnectionManager()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Starts a transaction.
        /// </summary>
        /// <returns></returns>
        public bool BeginTransaction()
        {
            if (OpenTransactionExists)
                return true;

            if (Connection.State != ConnectionState.Open)
                Connection.Open();

            _transaction = Connection.BeginTransaction(IsolationLevel.ReadCommitted);

            return false;
        }

        /// <summary>
        /// Commits a running transaction.
        /// </summary>
        public void CommitTransaction()
        {
            CommitTransaction(false);
        }

        /// <summary>
        /// Commits a running transaction if the provided parameter is set to false.
        /// </summary>
        public void CommitTransaction(bool transactionExisted)
        {
            if (!transactionExisted)
                _transaction.Commit();
        }

        /// <summary>
        /// Rollbacks a running transaction.
        /// </summary>
        public void RollbackTransaction()
        {
            RollbackTransaction(false);
        }

        /// <summary>
        /// Rollbacks a running transaction if the provided parameter is set to false. 
        /// </summary>
        public void RollbackTransaction(bool transactionExisted)
        {
            if (!transactionExisted)
                _transaction.Rollback();
        }

        /// <summary>
        /// Closes the underlying connection.
        /// </summary>
        public void CloseConnection()
        {
            if (_connection != null && _connection.State == ConnectionState.Open)
                _connection.Close();
        }

        /// <summary>
        /// Executes the provided logic in an transaction and conditionally does a rollback and commit
        /// </summary>
        /// <param name="delLogic">The logic to execute in an transaction</param>
        public void RunInTransaction(Action delLogic)
        {
            if (delLogic == null)
                throw new ArgumentNullException("delLogic");

            bool bolTransactionExisted = BeginTransaction();

            try
            {
                delLogic();
                CommitTransaction(bolTransactionExisted);
            }
            catch
            {
                RollbackTransaction(bolTransactionExisted);
                throw;
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            // free managed resources
            if (_connection != null)
                _connection.Dispose();
        }

        public ConnectionManager Clone()
        {
            return new ConnectionManager(ConnectionString, _closeConnectionAfterCommandProcessing);
        }

        #endregion
    }
}
