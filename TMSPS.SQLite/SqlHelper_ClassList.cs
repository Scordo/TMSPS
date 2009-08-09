using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using TMSPS.Core.SQL;

namespace TMSPS.SQLite
{
    public partial class SqlHelper
    {
        #region From DataRow

        public List<ClassName> ExecuteClassListQuery<ClassName>(string selectStatement, ObjectFromDataRowHandler<ClassName> generateMethod)
        {
            return ExecuteClassListQuery(selectStatement, generateMethod, (object[])null);
        }

        public List<ClassName> ExecuteClassListQuery<ClassName>(string selectStatement, ObjectFromDataRowHandler<ClassName> generateMethod, params object[] parameters)
        {
            return ExecuteClassListQuery(selectStatement, generateMethod, GetParametersFromObjectArray(parameters));
        }

        public List<ClassName> ExecuteClassListQuery<ClassName>(string selectStatement, ObjectFromDataRowHandler<ClassName> generateMethod, Dictionary<string, object> parameters)
        {
            if (parameters == null)
                parameters = new Dictionary<string, object>();

            DataTable table = ExecuteDataTable(selectStatement, parameters);
            List<ClassName> result = new List<ClassName>();

            foreach (DataRow row in table.Rows)
            {
                result.Add(generateMethod(row));
            }

            DoPostCommandProcessing();

            return result;
        }

        public PagedList<ClassName> ExecutePagedClassListQuery<ClassName>(string countStatement, string selectStatement, ObjectFromDataRowHandler<ClassName> generateMethod, int? startIndex, int? endIndex)
        {
            return ExecutePagedClassListQuery(countStatement, selectStatement, generateMethod, startIndex, endIndex, (object[])null);
        }

        public PagedList<ClassName> ExecutePagedClassListQuery<ClassName>(string countStatement, string selectStatement, ObjectFromDataRowHandler<ClassName> generateMethod, int? startIndex, int? endIndex, params object[] parameters)
        {
            return ExecutePagedClassListQuery(countStatement, selectStatement, generateMethod, startIndex, endIndex, GetParametersFromObjectArray(parameters));
        }

        public PagedList<ClassName> ExecutePagedClassListQuery<ClassName>(string countStatement, string selectStatement, ObjectFromDataRowHandler<ClassName> generateMethod, int? startIndex, int? endIndex, Dictionary<string, object> parameters)
        {
            if (parameters == null)
                parameters = new Dictionary<string, object>();

            int virtualCount = ExecuteScalar<int>(countStatement, parameters);
            DataTable table = ExecuteDataTable(selectStatement, parameters);

            PagedList<ClassName> result = new PagedList<ClassName> { VirtualCount = virtualCount };

            foreach (DataRow row in table.Rows)
            {
                result.Add(generateMethod(row));
            }

            DoPostCommandProcessing();

            return result;
        }

        #endregion

        #region From DataReader

        public List<ClassName> ExecuteClassListQuery<ClassName>(string selectStatement, ObjectFromDataReaderHandler<ClassName> generateMethod)
        {
            return ExecuteClassListQuery(selectStatement, generateMethod, (object[])null);
        }

        public List<ClassName> ExecuteClassListQuery<ClassName>(string selectStatement, ObjectFromDataReaderHandler<ClassName> generateMethod, params object[] parameters)
        {
            return ExecuteClassListQuery(selectStatement, generateMethod, GetParametersFromObjectArray(parameters));
        }

        public List<ClassName> ExecuteClassListQuery<ClassName>(string selectStatement, ObjectFromDataReaderHandler<ClassName> generateMethod, Dictionary<string, object> parameters)
        {
            if (parameters == null)
                parameters = new Dictionary<string, object>();

            SQLiteDataReader reader = ExecuteReader(selectStatement, parameters);
            List<ClassName> result = new List<ClassName>();

            while (reader.Read())
            {
                result.Add(generateMethod(reader));
            }

            DoPostCommandProcessing();

            return result;
        }

        public PagedList<ClassName> ExecutePagedClassListQuery<ClassName>(string countStatement, string selectStatement, ObjectFromDataReaderHandler<ClassName> generateMethod, int? startIndex, int? endIndex)
        {
            return ExecutePagedClassListQuery(countStatement, selectStatement, generateMethod, startIndex, endIndex, (object[])null);
        }

        public PagedList<ClassName> ExecutePagedClassListQuery<ClassName>(string countStatement, string selectStatement, ObjectFromDataReaderHandler<ClassName> generateMethod, int? startIndex, int? endIndex, params object[] parameters)
        {
            return ExecutePagedClassListQuery(countStatement, selectStatement, generateMethod, startIndex, endIndex, GetParametersFromObjectArray(parameters));
        }

        public PagedList<ClassName> ExecutePagedClassListQuery<ClassName>(string countStatement, string selectStatement, ObjectFromDataReaderHandler<ClassName> generateMethod, int? startIndex, int? endIndex, Dictionary<string, object> parameters)
        {
            if (parameters == null)
                parameters = new Dictionary<string, object>();

            int virtualCount = ExecuteScalar<int>(countStatement, parameters);
            SQLiteDataReader reader = ExecuteReader(selectStatement, parameters);

            PagedList<ClassName> result = new PagedList<ClassName> { VirtualCount = virtualCount };

            while (reader.Read())
            {
                result.Add(generateMethod(reader));
            }

            DoPostCommandProcessing();

            return result;
        }

        public static string GetLimitStatement(int? startIndex, int? endIndex)
        {
            if (!startIndex.HasValue && !endIndex.HasValue)
                return string.Empty;

            if (!startIndex.HasValue)
                return string.Format("LIMIT {0}", endIndex.Value + 1);

            if (!endIndex.HasValue)
                endIndex = int.MaxValue;


            return string.Format("LIMIT {0},{1}", startIndex, endIndex);
        }

        #endregion
    }
}