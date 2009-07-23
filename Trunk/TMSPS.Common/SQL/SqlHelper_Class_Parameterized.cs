using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace TMSPS.Core.SQL
{
    public partial class SqlHelper
    {
        #region From DataReader

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
            return ExecuteClassQuery(storedProcedureName, generateMethod, filterParameters, (object[])null);
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

        #endregion

        #region From DataRow

        public ClassName ExecuteClassQuery<ClassName>(string storedProcedureName, ObjectFromParameterizedDataRowHandler<ClassName> generateMethod, object[] filterParameters)
        {
            return ExecuteClassQuery(storedProcedureName, generateMethod, filterParameters, (object[])null);
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

        #endregion
    }
}