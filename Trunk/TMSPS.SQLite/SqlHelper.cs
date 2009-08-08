using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

namespace TMSPS.SQLite
{
    /// <summary>
    /// This delegate is used to create an instance of the provided Generic-Type using the provided SQLiteDataReader
    /// </summary>
    public delegate ClassName ObjectFromDataReaderHandler<ClassName>(SQLiteDataReader reader);

    public partial class SqlHelper
    {
        #region Non Public Members

        private readonly ConnectionManager _connectionManager;
        private readonly bool _closeConnectionAfterCommandProcessing;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the connection manager.
        /// </summary>
        /// <value>The connection manager.</value>
        public ConnectionManager ConnectionManager
        {
            get { return _connectionManager; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlHelper"/> class.
        /// </summary>
        public SqlHelper() : this(ConnectionManager.NewInstance, true)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlHelper"/> class.
        /// </summary>
        /// <param name="connectionManager">The connection manager.</param>
        /// <param name="closeConnectionAfterCommandProcessing">if set to <c>true</c> the connection is closed after command processing.</param>
        public SqlHelper(ConnectionManager connectionManager, bool closeConnectionAfterCommandProcessing)
        {
            if (connectionManager == null)
                throw new ArgumentNullException("connectionManager");

            _connectionManager = connectionManager;
            _closeConnectionAfterCommandProcessing = closeConnectionAfterCommandProcessing;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Executes the the stored procedure with the given name and returns a single value as object.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <returns></returns>
        public object ExecuteScalar(string sql)
        {
            return ExecuteScalar(sql, (Dictionary<string, object>)null);
        }

        /// <summary>
        /// Executes the the stored procedure with the given name, passes the provided parameters and returns a single value as object.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters as objects in the shape of  paramname, paramvalue, paramname, paramvalue and so on.</param>
        /// <returns></returns>
        public object ExecuteScalar(string sql, params object[] parameters)
        {
            return ExecuteScalar(sql, GetParametersFromObjectArray(parameters));
        }

        /// <summary>
        /// Executes the the stored procedure with the given name, passes the provided parameters and returns a single value as object.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public object ExecuteScalar(string sql, Dictionary<string, object> parameters)
        {
            if (parameters == null)
                parameters = new Dictionary<string, object>();

            SQLiteCommand command = GenerateCommand(sql, parameters);
            object result = command.ExecuteScalar();
            DoPostCommandProcessing();

            return result;
        }

        /// <summary>
        /// Executes the the stored procedure with the given name and returns a single value as integer.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <returns></returns>
        public int ExecuteScalarReturnInteger(string sql)
        {
            return Convert.ToInt32(ExecuteScalar(sql, (object[])null));
        }

        /// <summary>
        /// Executes the the stored procedure with the given name, passes the provided parameters and returns a single value as integer.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters as objects in the shape of  paramname, paramvalue, paramname, paramvalue and so on.</param>
        /// <returns></returns>
        public int ExecuteScalarReturnInteger(string sql, params object[] parameters)
        {
            return Convert.ToInt32(ExecuteScalar(sql, parameters));
        }

        /// <summary>
        /// Executes the the stored procedure with the given name, passes the provided parameters and returns a single value as integer.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public int ExecuteScalarReturnInteger(string sql, Dictionary<string, object> parameters)
        {
            return Convert.ToInt32(ExecuteScalar(sql, parameters));
        }

        /// <summary>
        /// Executes the the stored procedure with the given name and returns a single value of the passed generic type.
        /// </summary>
        /// <typeparam name="TReturnType">The type of the return value.</typeparam>
        /// <param name="sql">The SQL.</param>
        /// <returns></returns>
        public TReturnType ExecuteScalar<TReturnType>(string sql)
        {
            return ExecuteScalar<TReturnType>(sql, (Dictionary<string, object>)null);
        }

        /// <summary>
        /// Executes the the stored procedure with the given name, passes the provided parameters and returns a single value of the passed generic type.
        /// </summary>
        /// <typeparam name="TReturnType">The type of the return value.</typeparam>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters as objects in the shape of  paramname, paramvalue, paramname, paramvalue and so on.</param>
        /// <returns></returns>
        public TReturnType ExecuteScalar<TReturnType>(string sql, params object[] parameters)
        {
            return ExecuteScalar<TReturnType>(sql, GetParametersFromObjectArray(parameters));
        }

        /// <summary>
        /// Executes the the stored procedure with the given name, passes the provided parameters and returns a single value of the passed generic type.
        /// </summary>
        /// <typeparam name="TReturnType">The type of the return value.</typeparam>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public TReturnType ExecuteScalar<TReturnType>(string sql, Dictionary<string, object> parameters)
        {
            object result = ExecuteScalar(sql, parameters);
            Type convertToType = typeof(TReturnType);

            if (typeof(TReturnType).IsGenericType && typeof(TReturnType).GetGenericTypeDefinition() == typeof(Nullable<>).GetGenericTypeDefinition())
            {
                if (result == null || result == DBNull.Value)
                    return default(TReturnType);

                convertToType = typeof(TReturnType).GetGenericArguments()[0];
            }

            return (TReturnType)Convert.ChangeType(result, convertToType);
        }

        /// <summary>
        /// Executes the the stored procedure with the given name and returns a single value as boolean.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <returns></returns>
        public bool ExecuteScalarReturnBool(string sql)
        {
            return ExecuteScalarReturnBool(sql, (object[])null);
        }

        /// <summary>
        /// Executes the the stored procedure with the given name, passes the provided parameters and returns a single value as boolean.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters as objects in the shape of  paramname, paramvalue, paramname, paramvalue and so on.</param>
        /// <returns></returns>
        public bool ExecuteScalarReturnBool(string sql, params object[] parameters)
        {
            return ExecuteScalarReturnInteger(sql, parameters) != 0;
        }

        /// <summary>
        /// Executes the the stored procedure with the given name, passes the provided parameters and returns a single value as boolean.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public bool ExecuteScalarReturnBool(string sql, Dictionary<string, object> parameters)
        {
            return ExecuteScalarReturnInteger(sql, parameters) != 0;
        }

        /// <summary>
        /// Executes the the stored procedure with the given name without expecting a result
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <returns></returns>
        public int ExecuteNonQuery(string sql)
        {
            return ExecuteNonQuery(sql, (object[])null);
        }

        /// <summary>
        /// Executes the the stored procedure with the given name and passes the provided parameters without expecting a result
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters as objects in the shape of  paramname, paramvalue, paramname, paramvalue and so on.</param>
        /// <returns></returns>
        public int ExecuteNonQuery(string sql, params object[] parameters)
        {
            return ExecuteNonQuery(sql, GetParametersFromObjectArray(parameters));
        }

        /// <summary>
        /// Executes the the stored procedure with the given name and passes the provided parameters without expecting a result
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public int ExecuteNonQuery(string sql, Dictionary<string, object> parameters)
        {
            if (parameters == null)
                parameters = new Dictionary<string, object>();

            SQLiteCommand command = GenerateCommand(sql, parameters);
            int result = command.ExecuteNonQuery();
            DoPostCommandProcessing();

            return result;
        }

        /// <summary>
        /// Executes the stored procedure with the given name andreturns the result as a DataTable
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(string sql)
        {
            return ExecuteDataTable(sql, (object[])null);
        }

        /// <summary>
        /// Executes the stored procedure with the given name, passes the provided parameters and returns the result as a DataTable
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters as objects in the shape of  paramname, paramvalue, paramname, paramvalue and so on.</param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(string sql, params object[] parameters)
        {
            return ExecuteDataTable(sql, GetParametersFromObjectArray(parameters));
        }

        /// <summary>
        /// Executes the stored procedure with the given name, passes the provided parameters and returns the result as a DataTable
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(string sql, Dictionary<string, object> parameters)
        {
            return ExecuteDataTable(sql, parameters, null);
        }

        /// <summary>
        /// Executes the stored procedure with the given name, passes the provided parameters and returns the result as a DataTable
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="outputParameters">The output parameters.</param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(string sql, Dictionary<string, object> parameters, out SQLiteParameterCollection outputParameters)
        {
            return ExecuteDataTable(sql, parameters, null, out outputParameters);
        }

        /// <summary>
        /// Executes the stored procedure with the given name, passes the provided parameters and returns the result as a DataTable
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(string sql, IEnumerable<SQLiteParameter> parameters)
        {
            return ExecuteDataTable(sql, null, parameters);
        }

        /// <summary>
        /// Executes the stored procedure with the given name, passes the provided parameters and returns the result as a DataTable
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="outputParameters">The output parameters.</param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(string sql, IEnumerable<SQLiteParameter> parameters, out SQLiteParameterCollection outputParameters)
        {
            return ExecuteDataTable(sql, null, parameters, out outputParameters);
        }

        /// <summary>
        /// Executes the stored procedure with the given name, passes the provided parameters and returns the result as a DataTable
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="sqlParameters">The sql parameters.</param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(string sql, Dictionary<string, object> parameters, IEnumerable<SQLiteParameter> sqlParameters)
        {
            SQLiteParameterCollection outParameters;
            return ExecuteDataTable(sql, parameters, sqlParameters, out outParameters);
        }

        /// <summary>
        /// Executes the stored procedure with the given name, passes the provided parameters and returns the result as a DataTable
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="sqlParameters">The sql parameters.</param>
        /// <param name="outputParameters">The output parameters.</param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(string sql, Dictionary<string, object> parameters, IEnumerable<SQLiteParameter> sqlParameters, out SQLiteParameterCollection outputParameters)
        {
            SQLiteCommand command = GenerateCommand(sql, parameters, sqlParameters);
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
            DataSet dataSet = new DataSet();
            adapter.Fill(dataSet, "Table");
            outputParameters = command.Parameters;
            DoPostCommandProcessing();

            return dataSet.Tables["Table"];
        }

        /// <summary>
        /// Executes the stored procedure with the given name and returns a SQLiteDataReader
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <returns></returns>
        public SQLiteDataReader ExecuteReader(string sql)
        {
            return ExecuteReader(sql, (object[])null);
        }

        /// <summary>
        /// Executes the stored procedure with the given name, passes the provided parameters and returns a SQLiteDataReader
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public SQLiteDataReader ExecuteReader(string sql, params object[] parameters)
        {
            return ExecuteReader(sql, GetParametersFromObjectArray(parameters));
        }

        /// <summary>
        /// Executes the stored procedure with the given name, passes the provided parameters and returns a SQLiteDataReader
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public SQLiteDataReader ExecuteReader(string sql, Dictionary<string, object> parameters)
        {
            return GenerateCommand(sql, parameters).ExecuteReader();
        }

        /// <summary>
        /// Executes the stored procedure with the given name, passes the provided parameters and returns a SQLiteDataReader
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="outputParameters">The output parameters.</param>
        /// <returns></returns>
        public SQLiteDataReader ExecuteReader(string sql, Dictionary<string, object> parameters, out SQLiteParameterCollection outputParameters)
        {
            return ExecuteReader(sql, parameters, null, out outputParameters);
        }

        /// <summary>
        /// Executes the stored procedure with the given name, passes the provided parameters and returns a SQLiteDataReader
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public SQLiteDataReader ExecuteReader(string sql, IEnumerable<SQLiteParameter> parameters)
        {
            SQLiteParameterCollection outputParameters;
            return ExecuteReader(sql, null, parameters, out outputParameters);
        }

        /// <summary>
        /// Executes the stored procedure with the given name, passes the provided parameters and returns a SQLiteDataReader
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="outputParameters">The output parameters.</param>
        /// <returns></returns>
        public SQLiteDataReader ExecuteReader(string sql, IEnumerable<SQLiteParameter> parameters, out SQLiteParameterCollection outputParameters)
        {
            return ExecuteReader(sql, null, parameters, out outputParameters);
        }

        /// <summary>
        /// Executes the stored procedure with the given name, passes the provided parameters and returns a SQLiteDataReader
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="sqlParameters">The SQL parameters.</param>
        /// <returns></returns>
        public SQLiteDataReader ExecuteReader(string sql, Dictionary<string, object> parameters, IEnumerable<SQLiteParameter> sqlParameters)
        {
            SQLiteParameterCollection outputParameters;
            return ExecuteReader(sql, parameters, sqlParameters, out outputParameters);
        }

        /// <summary>
        /// Executes the stored procedure with the given name, passes the provided parameters and returns a SQLiteDataReader
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="sqlParameters">The SQL parameters.</param>
        /// <param name="outputParameters">The output parameters.</param>
        /// <returns></returns>
        public SQLiteDataReader ExecuteReader(string sql, Dictionary<string, object> parameters, IEnumerable<SQLiteParameter> sqlParameters, out SQLiteParameterCollection outputParameters)
        {
            SQLiteCommand sqlCommand = GenerateCommand(sql, parameters);

            try
            {
                return sqlCommand.ExecuteReader();
            }
            finally
            {
                outputParameters = sqlCommand.Parameters;
            }
        }

        #endregion

        #region Non Public Methods

        /// <summary>
        /// Generates a command for the provided stored procedure and parameters
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        private SQLiteCommand GenerateCommand(string sql, IEnumerable<KeyValuePair<string, object>> parameters)
        {
            return GenerateCommand(sql, parameters, null);
        }

        /// <summary>
        /// Generates a command for the provided stored procedure and parameters
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="sqlParameters">The parameters.</param>
        /// <returns></returns>
        private SQLiteCommand GenerateCommand(string sql, IEnumerable<SQLiteParameter> sqlParameters)
        {
            return GenerateCommand(sql, null, sqlParameters);
        }

        /// <summary>
        /// Generates a command for the provided stored procedure and parameters
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="sqlParameters">The sql parameters.</param>
        /// <returns></returns>
        private SQLiteCommand GenerateCommand(string sql, IEnumerable<KeyValuePair<string, object>> parameters, IEnumerable<SQLiteParameter> sqlParameters)
        {
            SQLiteCommand command = new SQLiteCommand
            {
                Connection = ConnectionManager.Connection,
                CommandType = CommandType.Text,
                CommandText = sql,
                CommandTimeout = ConnectionManager.Connection.ConnectionTimeout,
                Transaction = ConnectionManager.Transaction
            };

            if (parameters != null)
            {
                foreach (KeyValuePair<string, object> parameter in parameters)
                {
                    command.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);
                }
            }

            if (sqlParameters != null)
            {
                foreach (SQLiteParameter parameter in sqlParameters)
                {
                    command.Parameters.Add(parameter);
                }
            }

            if (command.Connection.State == ConnectionState.Closed)
                command.Connection.Open();

            return command;
        }

        /// <summary>
        /// Does the post command processing (currently closing the connection).
        /// </summary>
        private void DoPostCommandProcessing()
        {
            if (!ConnectionManager.OpenTransactionExists && _closeConnectionAfterCommandProcessing)
                ConnectionManager.CloseConnection();
        }

        /// <summary>
        /// Gets the parameters from an object array.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        private static Dictionary<string, object> GetParametersFromObjectArray(object[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
                return new Dictionary<string, object>();

            if (parameters.Length % 2 != 0)
                throw new ArgumentException("Parameters must be provided as pairs, the amount of parameters provided is not even.");

            Dictionary<string, object> result = new Dictionary<string, object>();

            for (int i = 0; i < parameters.Length; i += 2)
            {
                if (parameters[i] == null)
                    throw new NotSupportedException(string.Format("The parameter name no. {0} is null.", (i / 2) + 1));

                if (!(parameters[i] is string))
                    throw new NotSupportedException(string.Format("The parameter name no. {0} is not of type string.", (i / 2) + 1));

                string parameterName = Convert.ToString(parameters[i]);
                object parameterValue = parameters[i + 1];

                result.Add(parameterName, parameterValue);
            }

            return result;
        }

        #endregion
    }
}