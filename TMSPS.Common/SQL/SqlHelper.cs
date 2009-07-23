using System;
using System.Xml;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace TMSPS.Core.SQL
{
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
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <returns></returns>
        public object ExecuteScalar(string storedProcedureName)
        {
            return ExecuteScalar(storedProcedureName, (Dictionary<string, object>) null);
        }

        /// <summary>
        /// Executes the the stored procedure with the given name, passes the provided parameters and returns a single value as object.
        /// </summary>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <param name="parameters">The parameters as objects in the shape of  paramname, paramvalue, paramname, paramvalue and so on.</param>
        /// <returns></returns>
        public object ExecuteScalar(string storedProcedureName, params object[] parameters)
        {
            return ExecuteScalar(storedProcedureName, GetParametersFromObjectArray(parameters));
        }

        /// <summary>
        /// Executes the the stored procedure with the given name, passes the provided parameters and returns a single value as object.
        /// </summary>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public object ExecuteScalar(string storedProcedureName, Dictionary<string, object> parameters)
        {
            if (parameters == null)
                parameters = new Dictionary<string, object>();

            SqlCommand command = GenerateCommand(storedProcedureName, parameters);
            object result = command.ExecuteScalar();
            DoPostCommandProcessing();

            return result;
        }

        /// <summary>
        /// Executes the the stored procedure with the given name and returns a single value as integer.
        /// </summary>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <returns></returns>
        public int ExecuteScalarReturnInteger(string storedProcedureName)
        {
            return Convert.ToInt32(ExecuteScalar(storedProcedureName, (object[]) null));
        }

        /// <summary>
        /// Executes the the stored procedure with the given name, passes the provided parameters and returns a single value as integer.
        /// </summary>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <param name="parameters">The parameters as objects in the shape of  paramname, paramvalue, paramname, paramvalue and so on.</param>
        /// <returns></returns>
        public int ExecuteScalarReturnInteger(string storedProcedureName, params object[] parameters)
        {
            return Convert.ToInt32(ExecuteScalar(storedProcedureName, parameters));
        }

        /// <summary>
        /// Executes the the stored procedure with the given name, passes the provided parameters and returns a single value as integer.
        /// </summary>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public int ExecuteScalarReturnInteger(string storedProcedureName, Dictionary<string, object> parameters)
        {
            return Convert.ToInt32(ExecuteScalar(storedProcedureName, parameters));
        }

        /// <summary>
        /// Executes the the stored procedure with the given name and returns a single value of the passed generic type.
        /// </summary>
        /// <typeparam name="TReturnType">The type of the return value.</typeparam>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <returns></returns>
        public TReturnType ExecuteScalar<TReturnType>(string storedProcedureName)
        {
            return ExecuteScalar<TReturnType>(storedProcedureName, (Dictionary<string, object>) null);
        }

        /// <summary>
        /// Executes the the stored procedure with the given name, passes the provided parameters and returns a single value of the passed generic type.
        /// </summary>
        /// <typeparam name="TReturnType">The type of the return value.</typeparam>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <param name="parameters">The parameters as objects in the shape of  paramname, paramvalue, paramname, paramvalue and so on.</param>
        /// <returns></returns>
        public TReturnType ExecuteScalar<TReturnType>(string storedProcedureName, params object[] parameters)
        {
            return ExecuteScalar<TReturnType>(storedProcedureName, GetParametersFromObjectArray(parameters));
        }

        /// <summary>
        /// Executes the the stored procedure with the given name, passes the provided parameters and returns a single value of the passed generic type.
        /// </summary>
        /// <typeparam name="TReturnType">The type of the return value.</typeparam>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public TReturnType ExecuteScalar<TReturnType>(string storedProcedureName, Dictionary<string, object> parameters)
        {
            object result = ExecuteScalar(storedProcedureName, parameters);
            Type convertToType = typeof (TReturnType);

            if (typeof(TReturnType).IsGenericType && typeof(TReturnType).GetGenericTypeDefinition() == typeof(Nullable<>).GetGenericTypeDefinition())
            {
                if (result == null || result == DBNull.Value)
                    return default(TReturnType);

                convertToType = typeof (TReturnType).GetGenericArguments()[0];
            }

            return (TReturnType)Convert.ChangeType(result, convertToType);
        }

        /// <summary>
        /// Executes the the stored procedure with the given name and returns a single value as boolean.
        /// </summary>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <returns></returns>
        public bool ExecuteScalarReturnBool(string storedProcedureName)
        {
            return ExecuteScalarReturnBool(storedProcedureName, (object[]) null);
        }

        /// <summary>
        /// Executes the the stored procedure with the given name, passes the provided parameters and returns a single value as boolean.
        /// </summary>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <param name="parameters">The parameters as objects in the shape of  paramname, paramvalue, paramname, paramvalue and so on.</param>
        /// <returns></returns>
        public bool ExecuteScalarReturnBool(string storedProcedureName, params object[] parameters)
        {
            return ExecuteScalarReturnInteger(storedProcedureName, parameters) != 0;
        }

        /// <summary>
        /// Executes the the stored procedure with the given name, passes the provided parameters and returns a single value as boolean.
        /// </summary>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public bool ExecuteScalarReturnBool(string storedProcedureName, Dictionary<string, object> parameters)
        {
            return ExecuteScalarReturnInteger(storedProcedureName, parameters) != 0;
        }

        /// <summary>
        /// Executes the the stored procedure with the given name without expecting a result
        /// </summary>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <returns></returns>
        public int ExecuteNonQuery(string storedProcedureName)
        {
            return ExecuteNonQuery(storedProcedureName, (object[]) null);
        }

        /// <summary>
        /// Executes the the stored procedure with the given name and passes the provided parameters without expecting a result
        /// </summary>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <param name="parameters">The parameters as objects in the shape of  paramname, paramvalue, paramname, paramvalue and so on.</param>
        /// <returns></returns>
        public int ExecuteNonQuery(string storedProcedureName, params object[] parameters)
        {
            return ExecuteNonQuery(storedProcedureName, GetParametersFromObjectArray(parameters));
        }

        /// <summary>
        /// Executes the the stored procedure with the given name and passes the provided parameters without expecting a result
        /// </summary>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public int ExecuteNonQuery(string storedProcedureName, Dictionary<string, object> parameters)
        {
            if (parameters == null)
                parameters = new Dictionary<string, object>();

            SqlCommand command = GenerateCommand(storedProcedureName, parameters);
            int result = command.ExecuteNonQuery();
            DoPostCommandProcessing();

            return result;
        }

        /// <summary>
        /// Executes the stored procedure with the given name andreturns the result as a DataTable
        /// </summary>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(string storedProcedureName)
        {
            return ExecuteDataTable(storedProcedureName, (object[]) null);
        }

        /// <summary>
        /// Executes the stored procedure with the given name, passes the provided parameters and returns the result as a DataTable
        /// </summary>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <param name="parameters">The parameters as objects in the shape of  paramname, paramvalue, paramname, paramvalue and so on.</param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(string storedProcedureName, params object[] parameters)
        {
            return ExecuteDataTable(storedProcedureName, GetParametersFromObjectArray(parameters));
        }

        /// <summary>
        /// Executes the stored procedure with the given name, passes the provided parameters and returns the result as a DataTable
        /// </summary>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(string storedProcedureName, Dictionary<string, object> parameters)
        {
            return ExecuteDataTable(storedProcedureName, parameters, null);
        }

        /// <summary>
        /// Executes the stored procedure with the given name, passes the provided parameters and returns the result as a DataTable
        /// </summary>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="outputParameters">The output parameters.</param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(string storedProcedureName, Dictionary<string, object> parameters, out SqlParameterCollection outputParameters)
        {
            return ExecuteDataTable(storedProcedureName, parameters, null, out outputParameters);
        }

        /// <summary>
        /// Executes the stored procedure with the given name, passes the provided parameters and returns the result as a DataTable
        /// </summary>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(string storedProcedureName, IEnumerable<SqlParameter> parameters)
        {
            return ExecuteDataTable(storedProcedureName, null, parameters);
        }

        /// <summary>
        /// Executes the stored procedure with the given name, passes the provided parameters and returns the result as a DataTable
        /// </summary>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="outputParameters">The output parameters.</param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(string storedProcedureName, IEnumerable<SqlParameter> parameters, out SqlParameterCollection outputParameters)
        {
            return ExecuteDataTable(storedProcedureName, null, parameters, out outputParameters);
        }

        /// <summary>
        /// Executes the stored procedure with the given name, passes the provided parameters and returns the result as a DataTable
        /// </summary>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="sqlParameters">The sql parameters.</param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(string storedProcedureName, Dictionary<string, object> parameters, IEnumerable<SqlParameter> sqlParameters)
        {
            SqlParameterCollection outParameters;
            return ExecuteDataTable(storedProcedureName, parameters, sqlParameters, out outParameters);
        }

        /// <summary>
        /// Executes the stored procedure with the given name, passes the provided parameters and returns the result as a DataTable
        /// </summary>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="sqlParameters">The sql parameters.</param>
        /// <param name="outputParameters">The output parameters.</param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(string storedProcedureName, Dictionary<string, object> parameters, IEnumerable<SqlParameter> sqlParameters, out SqlParameterCollection outputParameters)
        {
            SqlCommand command = GenerateCommand(storedProcedureName, parameters, sqlParameters);
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            DataSet dataSet = new DataSet();
            adapter.Fill(dataSet, "Table");
            outputParameters = command.Parameters;
            DoPostCommandProcessing();

            return dataSet.Tables["Table"];
        }

        /// <summary>
        /// Executes the stored procedure with the given name and returns a SqlDataReader
        /// </summary>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <returns></returns>
        public SqlDataReader ExecuteReader(string storedProcedureName)
        {
            return ExecuteReader(storedProcedureName, (object[]) null);
        }

        /// <summary>
        /// Executes the stored procedure with the given name, passes the provided parameters and returns a SqlDataReader
        /// </summary>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public SqlDataReader ExecuteReader(string storedProcedureName, params object[] parameters)
        {
            return ExecuteReader(storedProcedureName, GetParametersFromObjectArray(parameters));
        }

        /// <summary>
        /// Executes the stored procedure with the given name, passes the provided parameters and returns a SqlDataReader
        /// </summary>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public SqlDataReader ExecuteReader(string storedProcedureName, Dictionary<string, object> parameters)
        {
            return GenerateCommand(storedProcedureName, parameters).ExecuteReader();
        }

        /// <summary>
        /// Executes the stored procedure with the given name, passes the provided parameters and returns a SqlDataReader
        /// </summary>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="outputParameters">The output parameters.</param>
        /// <returns></returns>
        public SqlDataReader ExecuteReader(string storedProcedureName, Dictionary<string, object> parameters, out SqlParameterCollection outputParameters)
        {
            return ExecuteReader(storedProcedureName, parameters, null, out outputParameters);
        }

        /// <summary>
        /// Executes the stored procedure with the given name, passes the provided parameters and returns a SqlDataReader
        /// </summary>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public SqlDataReader ExecuteReader(string storedProcedureName, IEnumerable<SqlParameter> parameters)
        {
            SqlParameterCollection outputParameters;
            return ExecuteReader(storedProcedureName, null, parameters, out outputParameters);
        }

        /// <summary>
        /// Executes the stored procedure with the given name, passes the provided parameters and returns a SqlDataReader
        /// </summary>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="outputParameters">The output parameters.</param>
        /// <returns></returns>
        public SqlDataReader ExecuteReader(string storedProcedureName, IEnumerable<SqlParameter> parameters, out SqlParameterCollection outputParameters)
        {
            return ExecuteReader(storedProcedureName, null, parameters, out outputParameters);
        }

        /// <summary>
        /// Executes the stored procedure with the given name, passes the provided parameters and returns a SqlDataReader
        /// </summary>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="sqlParameters">The SQL parameters.</param>
        /// <returns></returns>
        public SqlDataReader ExecuteReader(string storedProcedureName, Dictionary<string, object> parameters, IEnumerable<SqlParameter> sqlParameters)
        {
            SqlParameterCollection outputParameters;
            return ExecuteReader(storedProcedureName, parameters, sqlParameters, out outputParameters);
        }

        /// <summary>
        /// Executes the stored procedure with the given name, passes the provided parameters and returns a SqlDataReader
        /// </summary>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="sqlParameters">The SQL parameters.</param>
        /// <param name="outputParameters">The output parameters.</param>
        /// <returns></returns>
        public SqlDataReader ExecuteReader(string storedProcedureName, Dictionary<string, object> parameters, IEnumerable<SqlParameter> sqlParameters, out SqlParameterCollection outputParameters)
        {
            SqlCommand sqlCommand = GenerateCommand(storedProcedureName, parameters);

            try
            {
                return sqlCommand.ExecuteReader();
            }
            finally 
            {
                outputParameters = sqlCommand.Parameters;
            }
        }

        /// <summary>
        /// Executes the stored procedure with the given name and returns a XmlReader
        /// </summary>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <returns></returns>
        public XmlReader ExecuteXMLReader(string storedProcedureName)
        {
            return ExecuteXMLReader(storedProcedureName, (object[]) null);
        }

        /// <summary>
        /// Executes the stored procedure with the given name, passes the provided parameters and returns a XmlReader
        /// </summary>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <param name="parameters">The parameters as objects in the shape of  paramname, paramvalue, paramname, paramvalue and so on.</param>
        /// <returns></returns>
        public XmlReader ExecuteXMLReader(string storedProcedureName, params object[] parameters)
        {
            return GenerateCommand(storedProcedureName, GetParametersFromObjectArray(parameters)).ExecuteXmlReader();
        }

        /// <summary>
        /// Executes the stored procedure with the given name, passes the provided parameters and returns a XmlReader
        /// </summary>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public XmlReader ExecuteXMLReader(string storedProcedureName, Dictionary<string, object> parameters)
        {
            return GenerateCommand(storedProcedureName, parameters).ExecuteXmlReader();
        }

        #endregion

        #region Non Public Methods

        /// <summary>
        /// Generates a command for the provided stored procedure and parameters
        /// </summary>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        private SqlCommand GenerateCommand(string storedProcedureName, IEnumerable<KeyValuePair<string, object>> parameters)
        {
            return GenerateCommand(storedProcedureName, parameters, null);
        }

        /// <summary>
        /// Generates a command for the provided stored procedure and parameters
        /// </summary>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <param name="sqlParameters">The parameters.</param>
        /// <returns></returns>
        private SqlCommand GenerateCommand(string storedProcedureName, IEnumerable<SqlParameter> sqlParameters)
        {
            return GenerateCommand(storedProcedureName, null, sqlParameters);
        }

        /// <summary>
        /// Generates a command for the provided stored procedure and parameters
        /// </summary>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="sqlParameters">The sql parameters.</param>
        /// <returns></returns>
        private SqlCommand GenerateCommand(string storedProcedureName, IEnumerable<KeyValuePair<string, object>> parameters, IEnumerable<SqlParameter> sqlParameters)
        {
            SqlCommand command = new SqlCommand
            {
                Connection = ConnectionManager.Connection,
                CommandType = CommandType.StoredProcedure,
                CommandText = storedProcedureName,
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
                foreach (SqlParameter parameter in sqlParameters)
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
                    throw new NotSupportedException(string.Format("The parameter name no. {0} is null.", (i/2) + 1));

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