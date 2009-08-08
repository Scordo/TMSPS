using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using TMSPS.Core.SQL;

namespace TMSPS.SQLite
{
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

            SQLiteParameter virtualCountParameter = new SQLiteParameter("@VirtualCount", SqlDbType.Int) { Direction = ParameterDirection.Output };
            SQLiteParameter startIndexParameter = new SQLiteParameter("@StartIndex", startIndex == null ? DBNull.Value : (object)startIndex.Value);
            SQLiteParameter endIndexParameter = new SQLiteParameter("@EndIndex", endIndex == null ? DBNull.Value : (object)endIndex.Value);

            SQLiteParameterCollection outputParameters;
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

            SQLiteDataReader reader = ExecuteReader(storedProcedureName, parameters);
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

            SQLiteParameter virtualCountParameter = new SQLiteParameter("@VirtualCount", SqlDbType.Int) { Direction = ParameterDirection.Output };
            SQLiteParameter startIndexParameter = new SQLiteParameter("@StartIndex", startIndex == null ? DBNull.Value : (object)startIndex.Value);
            SQLiteParameter endIndexParameter = new SQLiteParameter("@EndIndex", endIndex == null ? DBNull.Value : (object)endIndex.Value);

            SQLiteParameterCollection outputParameters;
            SQLiteDataReader reader = ExecuteReader(storedProcedureName, parameters, new[] { startIndexParameter, endIndexParameter, virtualCountParameter }, out outputParameters);

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