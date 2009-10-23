using System;
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
    /// This delegate is used to create an instance of the provided Generic-Type using the provided DataRow
    /// </summary>
    public delegate ClassName ObjectFromDataRowHandler<ClassName>(DataRow row);

    public partial class SqlHelper
    {
        #region From DataRow

        public List<ClassName> ExecuteClassListQuery<ClassName>(string storedProcedureName, ObjectFromDataRowHandler<ClassName> generateMethod)
        {
            return ExecuteClassListQuery(storedProcedureName, generateMethod, (object[])null);
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

        public PagedList<ClassName> ExecutePagedClassListQuery<ClassName>(string storedProcedureName, ObjectFromDataRowHandler<ClassName> generateMethod, int? startIndex, int? endIndex)
        {
            return ExecutePagedClassListQuery(storedProcedureName, generateMethod, startIndex, endIndex, (object[])null);
        }

        public PagedList<ClassName> ExecutePagedClassListQuery<ClassName>(string storedProcedureName, ObjectFromDataRowHandler<ClassName> generateMethod, int? startIndex, int? endIndex, params object[] parameters)
        {
            return ExecutePagedClassListQuery(storedProcedureName, generateMethod, startIndex, endIndex, GetParametersFromObjectArray(parameters));
        }

        public PagedList<ClassName> ExecutePagedClassListQuery<ClassName>(string storedProcedureName, ObjectFromDataRowHandler<ClassName> generateMethod, int? startIndex, int? endIndex, Dictionary<string, object> parameters)
        {
            if (parameters == null)
                parameters = new Dictionary<string, object>();

            SqlParameter virtualCountParameter = new SqlParameter("@VirtualCount", SqlDbType.Int) { Direction = ParameterDirection.Output };
            SqlParameter startIndexParameter = new SqlParameter("@StartIndex", startIndex == null ? DBNull.Value : (object)startIndex.Value);
            SqlParameter endIndexParameter = new SqlParameter("@EndIndex", endIndex == null ? DBNull.Value : (object)endIndex.Value);

            SqlParameterCollection outputParameters;
            DataTable table = ExecuteDataTable(storedProcedureName, parameters, new[] { startIndexParameter, endIndexParameter, virtualCountParameter }, out outputParameters);

            PagedList<ClassName> result = new PagedList<ClassName> { VirtualCount = Convert.ToInt32(outputParameters["@VirtualCount"].Value) };

            foreach (DataRow row in table.Rows)
            {
                result.Add(generateMethod(row));
            }

            DoPostCommandProcessing();

            return result;
        }

        #endregion

        #region From DataReader

        public List<ClassName> ExecuteClassListQuery<ClassName>(string storedProcedureName, ObjectFromDataReaderHandler<ClassName> generateMethod)
        {
            return ExecuteClassListQuery(storedProcedureName, generateMethod, (object[])null);
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

        public PagedList<ClassName> ExecutePagedClassListQuery<ClassName>(string storedProcedureName, ObjectFromDataReaderHandler<ClassName> generateMethod, int? startIndex, int? endIndex)
        {
            return ExecutePagedClassListQuery(storedProcedureName, generateMethod, startIndex, endIndex, (object[])null);
        }

        public PagedList<ClassName> ExecutePagedClassListQuery<ClassName>(string storedProcedureName, ObjectFromDataReaderHandler<ClassName> generateMethod, int? startIndex, int? endIndex, params object[] parameters)
        {
            return ExecutePagedClassListQuery(storedProcedureName, generateMethod, startIndex, endIndex, GetParametersFromObjectArray(parameters));
        }

        public PagedList<ClassName> ExecutePagedClassListQuery<ClassName>(string storedProcedureName, ObjectFromDataReaderHandler<ClassName> generateMethod, int? startIndex, int? endIndex, Dictionary<string, object> parameters)
        {
            if (parameters == null)
                parameters = new Dictionary<string, object>();

            SqlParameter virtualCountParameter = new SqlParameter("@VirtualCount", SqlDbType.Int) { Direction = ParameterDirection.Output };
            SqlParameter startIndexParameter = new SqlParameter("@StartIndex", startIndex == null ? DBNull.Value : (object)startIndex.Value);
            SqlParameter endIndexParameter = new SqlParameter("@EndIndex", endIndex == null ? DBNull.Value : (object)endIndex.Value);

            SqlParameterCollection outputParameters;
            SqlDataReader reader = ExecuteReader(storedProcedureName, parameters, new[] { startIndexParameter, endIndexParameter, virtualCountParameter }, out outputParameters);

            PagedList<ClassName> result = new PagedList<ClassName> { VirtualCount = Convert.ToInt32(outputParameters["@VirtualCount"]) };

            while (reader.Read())
            {
                result.Add(generateMethod(reader));
            }

            DoPostCommandProcessing();

            return result;
        }

        #endregion
    }
}