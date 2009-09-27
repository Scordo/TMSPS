using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace TMSPS.Core.SQL
{
    /// <summary>
    /// This delegate is used to create an instance of the provided Generic-Type using the provided SqlDataReader with the ability to pass optional parameters
    /// </summary>
    public delegate ClassName ObjectFromParameterizedDataReaderHandler<ClassName>(SqlDataReader reader, object[] filterParameters);

    /// <summary>
    /// This delegate is used to create an instance of the provided Generic-Type using the provided DataRow with the ability to pass optional parameters
    /// </summary>
    public delegate ClassName ObjectFromParameterizedDataRowHandler<ClassName>(DataRow row, object[] filterParameters);

    public partial class SqlHelper
    {
        #region From DataRow

        public List<ClassName> ExecuteClassListQuery<ClassName>(string storedProcedureName, ObjectFromParameterizedDataRowHandler<ClassName> generateMethod, object[] filterParameters)
        {
            return ExecuteClassListQuery(storedProcedureName, generateMethod, filterParameters, (object[])null);
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

        public PagedList<ClassName> ExecutePagedClassListQuery<ClassName>(string storedProcedureName, ObjectFromParameterizedDataRowHandler<ClassName> generateMethod, int? startIndex, int? endIndex, object[] filterParameters)
        {
            return ExecutePagedClassListQuery(storedProcedureName, generateMethod, startIndex, endIndex, filterParameters, (object[])null);
        }

        public PagedList<ClassName> ExecutePagedClassListQuery<ClassName>(string storedProcedureName, ObjectFromParameterizedDataRowHandler<ClassName> generateMethod, int? startIndex, int? endIndex, object[] filterParameters, params object[] parameters)
        {
            return ExecutePagedClassListQuery(storedProcedureName, generateMethod, startIndex, endIndex,  filterParameters, GetParametersFromObjectArray(parameters));
        }

        public PagedList<ClassName> ExecutePagedClassListQuery<ClassName>(string storedProcedureName, ObjectFromParameterizedDataRowHandler<ClassName> generateMethod, int? startIndex, int? endIndex, object[] filterParameters, Dictionary<string, object> parameters)
        {
            if (parameters == null)
                parameters = new Dictionary<string, object>();

            SqlParameter virtualCountParameter = new SqlParameter("@VirtualCount", SqlDbType.Int) { Direction = ParameterDirection.Output };
            SqlParameter startIndexParameter = new SqlParameter("@StartIndex", startIndex == null ? DBNull.Value : (object) startIndex.Value);
            SqlParameter endIndexParameter = new SqlParameter("@EndIndex", endIndex == null ? DBNull.Value : (object) endIndex.Value);

            SqlParameterCollection outputParameters;
            DataTable table = ExecuteDataTable(storedProcedureName, parameters, new[] { startIndexParameter, endIndexParameter, virtualCountParameter }, out outputParameters);

            PagedList<ClassName> result = new PagedList<ClassName> { VirtualCount = Convert.ToInt32(outputParameters["@VirtualCount"]) };

            foreach (DataRow row in table.Rows)
            {
                result.Add(generateMethod(row, filterParameters));
            }

            DoPostCommandProcessing();

            return result;
        }

        #endregion

        #region From DataReader

        public List<ClassName> ExecuteClassListQuery<ClassName>(string storedProcedureName, ObjectFromParameterizedDataReaderHandler<ClassName> generateMethod, object[] filterParameters)
        {
            return ExecuteClassListQuery(storedProcedureName, generateMethod, filterParameters, (object[])null);
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

        public PagedList<ClassName> ExecutePagedClassListQuery<ClassName>(string storedProcedureName, ObjectFromParameterizedDataReaderHandler<ClassName> generateMethod, int? startIndex, int? endIndex, object[] filterParameters)
        {
            return ExecutePagedClassListQuery(storedProcedureName, generateMethod, startIndex, endIndex, filterParameters, (object[])null);
        }

        public PagedList<ClassName> ExecutePagedClassListQuery<ClassName>(string storedProcedureName, ObjectFromParameterizedDataReaderHandler<ClassName> generateMethod, int? startIndex, int? endIndex, object[] filterParameters, params object[] parameters)
        {
            return ExecutePagedClassListQuery(storedProcedureName, generateMethod, startIndex, endIndex, filterParameters, GetParametersFromObjectArray(parameters));
        }

        public PagedList<ClassName> ExecutePagedClassListQuery<ClassName>(string storedProcedureName, ObjectFromParameterizedDataReaderHandler<ClassName> generateMethod, int? startIndex, int? endIndex, object[] filterParameters, Dictionary<string, object> parameters)
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
                result.Add(generateMethod(reader, filterParameters));
            }

            DoPostCommandProcessing();

            return result;
        }

        #endregion
    }
}