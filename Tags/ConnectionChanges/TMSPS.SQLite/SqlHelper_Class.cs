using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using TMSPS.Core.SQL;

namespace TMSPS.SQLite
{
    public partial class SqlHelper
    {
        #region From DataReader

        /// <summary>
        /// Executes the stored procedure with the given name and used the generateMethod to return an instance of the passed generic type
        /// </summary>
        /// <typeparam name="ClassName">The type of the class to return.</typeparam>
        /// <param name="sql">The SQL.</param>
        /// <param name="generateMethod">The method expecting a DataRow and return an instance of the passed generic type.</param>
        /// <returns>An instance of the passed generic type </returns>
        public ClassName ExecuteClassQuery<ClassName>(string sql, ObjectFromDataReaderHandler<ClassName> generateMethod)
        {
            return ExecuteClassQuery(sql, generateMethod, (object[])null);
        }

        /// <summary>
        /// Executes the stored procedure with the given name, passes the provided parameters and used the generateMethod to return an instance of the passed generic type
        /// </summary>
        /// <typeparam name="ClassName">The type of the class to return.</typeparam>
        /// <param name="sql">The SQL.</param>
        /// <param name="generateMethod">The method expecting a DataRow and return an instance of the passed generic type.</param>
        /// <param name="parameters">The parameters as objects in the shape of  paramname, paramvalue, paramname, paramvalue and so on.</param>
        /// <returns>An instance of the passed generic type</returns>
        public ClassName ExecuteClassQuery<ClassName>(string sql, ObjectFromDataReaderHandler<ClassName> generateMethod, params object[] parameters)
        {
            return ExecuteClassQuery(sql, generateMethod, GetParametersFromObjectArray(parameters));
        }

        /// <summary>
        /// Executes the stored procedure with the given name, passes the provided parameters and used the generateMethod to return an instance of the passed generic type
        /// </summary>
        /// <typeparam name="ClassName">The type of the class to return.</typeparam>
        /// <param name="sql">The SQL.</param>
        /// <param name="generateMethod">The method expecting a DataRow and return an instance of the passed generic type.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>An instance of the passed generic type</returns>
        public ClassName ExecuteClassQuery<ClassName>(string sql, ObjectFromDataReaderHandler<ClassName> generateMethod, Dictionary<string, object> parameters)
        {
            if (parameters == null)
                parameters = new Dictionary<string, object>();

            SQLiteDataReader reader = ExecuteReader(sql, parameters);

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

        public ClassName ExecuteClassQuery<ClassName>(string sql, ObjectFromDataRowHandler<ClassName> generateMethod)
        {
            return ExecuteClassQuery(sql, generateMethod, (object[])null);
        }

        public ClassName ExecuteClassQuery<ClassName>(string sql, ObjectFromDataRowHandler<ClassName> generateMethod, params object[] parameters)
        {
            return ExecuteClassQuery(sql, generateMethod, GetParametersFromObjectArray(parameters));
        }

        public ClassName ExecuteClassQuery<ClassName>(string sql, ObjectFromDataRowHandler<ClassName> generateMethod, Dictionary<string, object> parameters)
        {
            if (parameters == null)
                parameters = new Dictionary<string, object>();

            DataTable table = ExecuteDataTable(sql, parameters);
            DoPostCommandProcessing();

            if (table.Rows.Count > 0)
                return generateMethod(table.Rows[0]);

            return default(ClassName);
        }

        #endregion
    }
}