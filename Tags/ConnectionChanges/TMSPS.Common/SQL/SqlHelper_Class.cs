using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace TMSPS.Core.SQL
{
    public partial class SqlHelper
    {
        #region From DataReader

        /// <summary>
        /// Executes the stored procedure with the given name and used the generateMethod to return an instance of the passed generic type
        /// </summary>
        /// <typeparam name="ClassName">The type of the class to return.</typeparam>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <param name="generateMethod">The method expecting a DataRow and return an instance of the passed generic type.</param>
        /// <returns>An instance of the passed generic type </returns>
        public ClassName ExecuteClassQuery<ClassName>(string storedProcedureName, ObjectFromDataReaderHandler<ClassName> generateMethod)
        {
            return ExecuteClassQuery(storedProcedureName, generateMethod, (object[])null);
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

        #endregion

        #region From DataRow

        public ClassName ExecuteClassQuery<ClassName>(string storedProcedureName, ObjectFromDataRowHandler<ClassName> generateMethod)
        {
            return ExecuteClassQuery(storedProcedureName, generateMethod, (object[])null);
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

        #endregion
    }
}