using System;
using System.Xml;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace TMSPS.Core.SQL
{
    /// <summary>
    /// This delegate is used to create an instance of the provided Generic-Type using the provided SqlDataReader
    /// </summary>
    public delegate ClassName ObjectFromDataReaderHandler<ClassName>(SqlDataReader reader);
    /// <summary>
    /// This delegate is used to create an instance of the provided Generic-Type using the provided SqlDataReader with the ability to pass optional parameters
    /// </summary>
    public delegate ClassName ObjectFromParameterizedDataReaderHandler<ClassName>(SqlDataReader reader, object[] filterParameters);
    /// <summary>
    /// This delegate is used to create an instance of the provided Generic-Type using the provided DataRow
    /// </summary>
    public delegate ClassName ObjectFromDataRowHandler<ClassName>(DataRow row);
    /// <summary>
    /// This delegate is used to create an instance of the provided Generic-Type using the provided DataRow with the ability to pass optional parameters
    /// </summary>
    public delegate ClassName ObjectFromParameterizedDataRowHandler<ClassName>(DataRow row, object[] filterParameters);

    public class SqlHelper
    {
        #region Non Public Members

        private readonly ConnectionManager _connectionManager;
    	private bool _closeConnectionAfterCommandProcessing;

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
                if (result == null)
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
            SqlDataAdapter adapter = new SqlDataAdapter(GenerateCommand(storedProcedureName, parameters));
            DataSet dataSet = new DataSet();
            adapter.Fill(dataSet, "Table");
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

        /// <summary>
        /// Executes the stored procedure with the given name and used the generateMethod to return an instance of the passed generic type
        /// </summary>
        /// <typeparam name="ClassName">The type of the class to return.</typeparam>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <param name="generateMethod">The method expecting a DataRow and return an instance of the passed generic type.</param>
        /// <returns>An instance of the passed generic type </returns>
        public ClassName ExecuteClassQuery<ClassName>(string storedProcedureName, ObjectFromDataReaderHandler<ClassName> generateMethod)
        {
            return ExecuteClassQuery(storedProcedureName, generateMethod, (object[]) null);
        }

        /// <summary>
        /// Executes the stored procedure with the given name, passes the provided parameters and used the generateMethod to return an instance of the passed generic type
        /// </summary>
        /// <typeparam name="ClassName">The type of the class to return.</typeparam>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <param name="generateMethod">The method expecting a DataRow and return an instance of the passed generic type.</param>
        /// <param name="parameters">The parameters as objects in the shape of  paramname, paramvalue, paramname, paramvalue and so on.</param>
        /// <returns>An instance of the passed generic type</returns>
        public ClassName ExecuteClassQuery<ClassName>(string storedProcedureName, ObjectFromDataReaderHandler<ClassName> generateMethod, params object[] parameters)
        {
            return ExecuteClassQuery(storedProcedureName, generateMethod, GetParametersFromObjectArray(parameters));
        }

        /// <summary>
        /// Executes the stored procedure with the given name, passes the provided parameters and used the generateMethod to return an instance of the passed generic type
        /// </summary>
        /// <typeparam name="ClassName">The type of the class to return.</typeparam>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <param name="generateMethod">The method expecting a DataRow and return an instance of the passed generic type.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>An instance of the passed generic type</returns>
        public ClassName ExecuteClassQuery<ClassName>(string storedProcedureName, ObjectFromDataReaderHandler<ClassName> generateMethod, Dictionary<string, object> parameters)
        {
            if (parameters == null)
                parameters = new Dictionary<string, object>();

            SqlDataReader reader = ExecuteReader(storedProcedureName, parameters);

            if (reader.Read())
            {
                ClassName result = generateMethod(reader);
                DoPostCommandProcessing();
                return result;
            }

            return default(ClassName);
        }

        /// <summary>
        /// Executes the stored procedure with the given name and used the generateMethod to return an instance of the passed generic type. You can pass optional filter parameters which are passed to the generateMethod
        /// </summary>
        /// <typeparam name="ClassName">The type of the class to return.</typeparam>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <param name="generateMethod">The method expecting a DataRow and return an instance of the passed generic type.</param>
        /// <param name="filterParameters">The filter parameters.</param>
        /// <returns>An instance of the passed generic type</returns>
        public ClassName ExecuteClassQuery<ClassName>(string storedProcedureName, ObjectFromParameterizedDataReaderHandler<ClassName> generateMethod, object[] filterParameters)
        {
            return ExecuteClassQuery(storedProcedureName, generateMethod, filterParameters, (object[]) null);
        }

        /// <summary>
        /// Executes the stored procedure with the given name, passes the provided parameters and used the generateMethod to return an instance of the passed generic type. You can pass optional filter parameters which are passed to the generateMethod
        /// </summary>
        /// <typeparam name="ClassName">The type of the class to return.</typeparam>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <param name="generateMethod">The method expecting a DataRow and return an instance of the passed generic type.</param>
        /// <param name="filterParameters">The filter parameters.</param>
        /// <param name="parameters">The parameters as objects in the shape of  paramname, paramvalue, paramname, paramvalue and so on.</param>
        /// <returns>An instance of the passed generic type</returns>
        public ClassName ExecuteClassQuery<ClassName>(string storedProcedureName, ObjectFromParameterizedDataReaderHandler<ClassName> generateMethod, object[] filterParameters, params object[] parameters)
        {
            return ExecuteClassQuery(storedProcedureName, generateMethod, filterParameters, GetParametersFromObjectArray(parameters));
        }

        /// <summary>
        /// Executes the stored procedure with the given name, passes the provided parameters and used the generateMethod to return an instance of the passed generic type. You can pass optional filter parameters which are passed to the generateMethod
        /// </summary>
        /// <typeparam name="ClassName">The type of the class to return.</typeparam>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <param name="generateMethod">The method expecting a DataRow and return an instance of the passed generic type.</param>
        /// <param name="filterParameters">The filter parameters.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>An instance of the passed generic type</returns>
        public ClassName ExecuteClassQuery<ClassName>(string storedProcedureName, ObjectFromParameterizedDataReaderHandler<ClassName> generateMethod, object[] filterParameters, Dictionary<string, object> parameters)
        {
            if (parameters == null)
                parameters = new Dictionary<string, object>();

            SqlDataReader reader = ExecuteReader(storedProcedureName, parameters);

            if (reader.Read())
            {
                ClassName result = generateMethod(reader, filterParameters);
                DoPostCommandProcessing();
                return result;
            }

            return default(ClassName);
        }

        public ClassName ExecuteClassQuery<ClassName>(string storedProcedureName, ObjectFromDataRowHandler<ClassName> generateMethod)
        {
            return ExecuteClassQuery(storedProcedureName, generateMethod, (object[]) null);
        }

        public ClassName ExecuteClassQuery<ClassName>(string storedProcedureName, ObjectFromDataRowHandler<ClassName> generateMethod, params object[] parameters)
        {
            return ExecuteClassQuery(storedProcedureName, generateMethod, GetParametersFromObjectArray(parameters));
        }

        public ClassName ExecuteClassQuery<ClassName>(string storedProcedureName, ObjectFromDataRowHandler<ClassName> generateMethod, Dictionary<string, object> parameters)
        {
            if (parameters == null)
                parameters = new Dictionary<string, object>();

            DataTable table = ExecuteDataTable(storedProcedureName, parameters);
            DoPostCommandProcessing();

            if (table.Rows.Count > 0)
                return generateMethod(table.Rows[0]);
            
            return default(ClassName);
        }

        public ClassName ExecuteClassQuery<ClassName>(string storedProcedureName, ObjectFromParameterizedDataRowHandler<ClassName> generateMethod, object[] filterParameters)
        {
            return ExecuteClassQuery(storedProcedureName, generateMethod, filterParameters, (object[]) null);
        }

        public ClassName ExecuteClassQuery<ClassName>(string storedProcedureName, ObjectFromParameterizedDataRowHandler<ClassName> generateMethod, object[] filterParameters, params object[] parameters)
        {
            return ExecuteClassQuery(storedProcedureName, generateMethod, filterParameters, GetParametersFromObjectArray(parameters));
        }

        public ClassName ExecuteClassQuery<ClassName>(string storedProcedureName, ObjectFromParameterizedDataRowHandler<ClassName> generateMethod, object[] filterParameters, Dictionary<string, object> parameters)
        {
            if (parameters == null)
                parameters = new Dictionary<string, object>();

            DataTable table = ExecuteDataTable(storedProcedureName, parameters);
            DoPostCommandProcessing();

            if (table.Rows.Count > 0)
                return generateMethod(table.Rows[0], filterParameters);
            
            return default(ClassName);
        }

        public List<ClassName> ExecuteClassListQuery<ClassName>(string storedProcedureName, ObjectFromDataReaderHandler<ClassName> generateMethod)
        {
            return ExecuteClassListQuery(storedProcedureName, generateMethod, (object[]) null);
        }

        public List<ClassName> ExecuteClassListQuery<ClassName>(string storedProcedureName, ObjectFromDataReaderHandler<ClassName> generateMethod, params object[] parameters)
        {
            return ExecuteClassListQuery(storedProcedureName, generateMethod, GetParametersFromObjectArray(parameters));
        }

        public List<ClassName> ExecuteClassListQuery<ClassName>(string storedProcedureName, ObjectFromDataReaderHandler<ClassName> generateMethod, Dictionary<string, object> parameters)
        {
            if (parameters == null)
                parameters = new Dictionary<string, object>();

            SqlDataReader reader = ExecuteReader(storedProcedureName, parameters);
            List<ClassName> result = new List<ClassName>();

            while (reader.Read())
            {
                result.Add(generateMethod(reader));
            }

            DoPostCommandProcessing();

            return result;
        }

        public List<ClassName> ExecuteClassListQuery<ClassName>(string storedProcedureName, ObjectFromParameterizedDataReaderHandler<ClassName> generateMethod, object[] filterParameters)
        {
            return ExecuteClassListQuery(storedProcedureName, generateMethod, filterParameters, (object[]) null);
        }

        public List<ClassName> ExecuteClassListQuery<ClassName>(string storedProcedureName, ObjectFromParameterizedDataReaderHandler<ClassName> generateMethod, object[] filterParameters, params object[] parameters)
        {
            return ExecuteClassListQuery(storedProcedureName, generateMethod, filterParameters, GetParametersFromObjectArray(parameters));
        }

        public List<ClassName> ExecuteClassListQuery<ClassName>(string storedProcedureName, ObjectFromParameterizedDataReaderHandler<ClassName> generateMethod, object[] filterParameters, Dictionary<string, object> parameters)
        {
            if (parameters == null)
                parameters = new Dictionary<string, object>();

            SqlDataReader reader = ExecuteReader(storedProcedureName, parameters);
            List<ClassName> result = new List<ClassName>();

            while (reader.Read())
            {
                result.Add(generateMethod(reader, filterParameters));
            }

            DoPostCommandProcessing();

            return result;
        }

        public List<ClassName> ExecuteClassListQuery<ClassName>(string storedProcedureName, ObjectFromDataRowHandler<ClassName> generateMethod)
        {
            return ExecuteClassListQuery(storedProcedureName, generateMethod, (object[]) null);
        }

        public List<ClassName> ExecuteClassListQuery<ClassName>(string storedProcedureName, ObjectFromDataRowHandler<ClassName> generateMethod, params object[] parameters)
        {
            return ExecuteClassListQuery(storedProcedureName, generateMethod, GetParametersFromObjectArray(parameters));
        }

        public List<ClassName> ExecuteClassListQuery<ClassName>(string storedProcedureName, ObjectFromDataRowHandler<ClassName> generateMethod, Dictionary<string, object> parameters)
        {
            if (parameters == null)
                parameters = new Dictionary<string, object>();

            DataTable table = ExecuteDataTable(storedProcedureName, parameters);
            List<ClassName> result = new List<ClassName>();

            foreach (DataRow row in table.Rows)
            {
                result.Add(generateMethod(row));
            }

            DoPostCommandProcessing();

            return result;
        }

        public List<ClassName> ExecuteClassListQuery<ClassName>(string storedProcedureName, ObjectFromParameterizedDataRowHandler<ClassName> generateMethod, object[] filterParameters)
        {
            return ExecuteClassListQuery(storedProcedureName, generateMethod, filterParameters, (object[]) null);
        }

        public List<ClassName> ExecuteClassListQuery<ClassName>(string storedProcedureName, ObjectFromParameterizedDataRowHandler<ClassName> generateMethod, object[] filterParameters, params object[] parameters)
        {
            return ExecuteClassListQuery(storedProcedureName, generateMethod, filterParameters, GetParametersFromObjectArray(parameters));
        }

        public List<ClassName> ExecuteClassListQuery<ClassName>(string storedProcedureName, ObjectFromParameterizedDataRowHandler<ClassName> generateMethod, object[] filterParameters, Dictionary<string, object> parameters)
        {
            if (parameters == null)
                parameters = new Dictionary<string, object>();

            DataTable table = ExecuteDataTable(storedProcedureName, parameters);
            List<ClassName> result = new List<ClassName>();

            foreach (DataRow row in table.Rows)
            {
                result.Add(generateMethod(row, filterParameters));
            }

            DoPostCommandProcessing();

            return result;
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
            SqlCommand command = new SqlCommand {
                                                     Connection = ConnectionManager.Connection,
                                                     CommandType = CommandType.StoredProcedure,
                                                     CommandText = storedProcedureName,
                                                     CommandTimeout = ConnectionManager.Connection.ConnectionTimeout,
                                                     Transaction = ConnectionManager.Transaction
                                                };

            foreach (KeyValuePair<string, object> parameter in parameters)
            {
                command.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);
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