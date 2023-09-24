using AddoCore.Db.MySql.Generator.Model;
using AddoCore.Db.MySql.Generator.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AddoCore.Db.MySql.Generator.Generators
{
    public static class DalGenerator
    {
        public static void Run(string path, string nspace, string tableName, List<Column> columns)
        {
            var fileString = GenerateFileString(nspace, tableName, columns);
            var filename = $"{tableName}Repository.Gen.cs";
            fileString.SaveTo($"{path}\\Dal\\{nspace}.Dal.MySql\\Gen\\{filename}");
        }

        private static string GenerateFileString(string nspace, string tableName, List<Column> columns)
        {
            var fColumns = columns.FilterOutDates();
            var primary = columns.GetPrimary();
            var uniqueColumn = columns.GetUnique();
            var foreignKeys = columns.GetForeignKeys();
            var stringColumns = columns.GetStringColumns();
            var ffk = foreignKeys.FirstOrDefault();

            StringBuilder sb = new StringBuilder();
            sb.Append($"using {nspace}.Dal.Interface.Gen;{Environment.NewLine}");
            sb.Append($"using {nspace}.Domain.Model;{Environment.NewLine}");
            sb.Append($"using {nspace}.Domain.Model.Gen;{Environment.NewLine}");
            sb.Append($"using Dapper;{Environment.NewLine}");
            sb.Append($"using log4net;{Environment.NewLine}");
            sb.Append($"using System;{Environment.NewLine}");
            sb.Append($"using System.Linq;{Environment.NewLine}");
            sb.Append($"using System.Threading.Tasks;{Environment.NewLine}");
            sb.Append($"using System.Collections.Generic;{Environment.NewLine}");
            sb.Append($"using {nspace}.Domain.Model.Model;{Environment.NewLine}");
            sb.Append($"using {nspace}.Dal.MySql.Repository;{Environment.NewLine}");

            sb.Append($"{Environment.NewLine}");
            sb.Append($"namespace {nspace}.Dal.MySql.Gen{Environment.NewLine}");
            sb.Append($"{{{Environment.NewLine}");
            sb.Append($"    public partial class {tableName}Repository : BaseRepository, I{tableName}Repository{Environment.NewLine}");
            sb.Append($"    {{{Environment.NewLine}");
            sb.Append($"        private ILog logger = LogManager.GetLogger(typeof({tableName}Repository));{Environment.NewLine}");
            sb.Append($"{Environment.NewLine}");
            sb.Append($"        public {tableName}Repository(string connectionString){Environment.NewLine}");
            sb.Append($"            : base(connectionString){Environment.NewLine}");
            sb.Append($"        {{ }}{Environment.NewLine}");
            sb.Append($"{Environment.NewLine}");


            /// InsUpd___ ///
            sb.Append($"        public async Task<{tableName}> InsUpd{tableName}({tableName} {tableName.ToCamelCase()}){Environment.NewLine}");
            sb.Append($"        {{{Environment.NewLine}");
            sb.Append($"            {tableName} toReturn = null;{Environment.NewLine}");
            sb.Append($"            try{Environment.NewLine}");
            sb.Append($"            {{{Environment.NewLine}");
            sb.Append($"                DynamicParameters param = new DynamicParameters();{Environment.NewLine}");

            foreach (var column in fColumns)
                sb.Append($"                param.Add(\"I_{column.COLUMN_NAME}\", {tableName.ToCamelCase()}.{column.COLUMN_NAME});{Environment.NewLine}");

            sb.Append($"{Environment.NewLine}");
            sb.Append($"                using (var connection = GetConnection()){Environment.NewLine}");
            sb.Append($"                {{{Environment.NewLine}");
            sb.Append($"                    toReturn = (await connection.QueryFirstAsync<{tableName}>(\"zgen_{tableName}_InsUpd\", param, commandType: System.Data.CommandType.StoredProcedure).ConfigureAwait(false));{Environment.NewLine}");
            sb.Append($"                }}{Environment.NewLine}");
            sb.Append($"{Environment.NewLine}");
            sb.Append($"            }}{Environment.NewLine}");
            sb.Append($"            catch (Exception ex){Environment.NewLine}");
            sb.Append($"            {{{Environment.NewLine}");
            sb.Append($"                logger.Error(\"{tableName}Repository->InsUpd{tableName}\", ex);{Environment.NewLine}");
            sb.Append($"            }}{Environment.NewLine}");
            sb.Append($"            return toReturn;{Environment.NewLine}");
            sb.Append($"        }}{Environment.NewLine}");
            sb.Append($"{Environment.NewLine}");

            /// Get___ById ///
            sb.Append($"		public async Task<List<{tableName}>> Get{tableName}ById(int? {primary.COLUMN_NAME.ToCamelCase()}, bool? isActive){Environment.NewLine}");
            sb.Append($"        {{{Environment.NewLine}");
            sb.Append($"            List<{tableName}> toReturn = null;{Environment.NewLine}");
            sb.Append($"            try{Environment.NewLine}");
            sb.Append($"            {{{Environment.NewLine}");
            sb.Append($"                DynamicParameters param = new DynamicParameters();{Environment.NewLine}");
            sb.Append($"                param.Add(\"I_{primary.COLUMN_NAME}\", {primary.COLUMN_NAME.ToCamelCase()});{Environment.NewLine}");
            sb.Append($"                param.Add(\"I_IsActive\", isActive);{Environment.NewLine}");
            sb.Append($"{Environment.NewLine}");
            sb.Append($"                using (var connection = GetConnection()){Environment.NewLine}");
            sb.Append($"                {{{Environment.NewLine}");
            sb.Append($"                    toReturn = (await connection.QueryAsync<{tableName}>(\"zgen_{tableName}_GetById\", param, commandType: System.Data.CommandType.StoredProcedure).ConfigureAwait(false)).AsList();{Environment.NewLine}");
            sb.Append($"                }}{Environment.NewLine}");
            sb.Append($"{Environment.NewLine}");
            sb.Append($"            }}{Environment.NewLine}");
            sb.Append($"            catch (Exception ex){Environment.NewLine}");
            sb.Append($"            {{{Environment.NewLine}");
            sb.Append($"                logger.Error(\"{tableName}Repository->Get{tableName}ById\", ex);{Environment.NewLine}");
            sb.Append($"            }}{Environment.NewLine}");
            sb.Append($"{Environment.NewLine}");
            sb.Append($"            return toReturn;{Environment.NewLine}");
            sb.Append($"        }}{Environment.NewLine}");
            sb.Append($"{Environment.NewLine}");


            if (uniqueColumn != null)
            {
                sb.Append($"		public async Task<{tableName}> Get{tableName}By{uniqueColumn.COLUMN_NAME}({uniqueColumn.GetCSharpDataType()} {uniqueColumn.COLUMN_NAME.ToCamelCase()}){Environment.NewLine}");
                sb.Append($"        {{{Environment.NewLine}");
                sb.Append($"            {tableName} toReturn = null;{Environment.NewLine}");
                sb.Append($"            try{Environment.NewLine}");
                sb.Append($"            {{{Environment.NewLine}");
                sb.Append($"                DynamicParameters param = new DynamicParameters();{Environment.NewLine}");
                sb.Append($"                param.Add(\"I_{uniqueColumn.COLUMN_NAME}\", {uniqueColumn.COLUMN_NAME.ToCamelCase()});{Environment.NewLine}");
                sb.Append($"{Environment.NewLine}");
                sb.Append($"                using (var connection = GetConnection()){Environment.NewLine}");
                sb.Append($"                {{{Environment.NewLine}");
                sb.Append($"                    toReturn = (await connection.QueryFirstAsync<{tableName}>(\"zgen_{tableName}_GetBy{uniqueColumn.COLUMN_NAME}\", param, commandType: System.Data.CommandType.StoredProcedure).ConfigureAwait(false));{Environment.NewLine}");
                sb.Append($"                }}{Environment.NewLine}");
                sb.Append($"{Environment.NewLine}");
                sb.Append($"            }}{Environment.NewLine}");
                sb.Append($"            catch (Exception ex){Environment.NewLine}");
                sb.Append($"            {{{Environment.NewLine}");
                sb.Append($"                logger.Error(\"{tableName}Repository->Get{tableName}By{uniqueColumn.COLUMN_NAME}\", ex);{Environment.NewLine}");
                sb.Append($"            }}{Environment.NewLine}");
                sb.Append($"{Environment.NewLine}");
                sb.Append($"            return toReturn;{Environment.NewLine}");
                sb.Append($"        }}{Environment.NewLine}");
                sb.Append($"{Environment.NewLine}");
            }

            if (foreignKeys.Any())
            {
                sb.Append($"		public async Task<List<{tableName}>> Get{tableName}ByIds(");

                foreach (var fk in foreignKeys)
                    sb.Append($"{(fk == ffk ? "" : ", ")}{fk.GetCSharpDataType()}? {fk.COLUMN_NAME.ToCamelCase()}");
                sb.Append($"){Environment.NewLine}");

                sb.Append($"        {{{Environment.NewLine}");
                sb.Append($"            List<{tableName}> toReturn = null;{Environment.NewLine}");
                sb.Append($"            try{Environment.NewLine}");
                sb.Append($"            {{{Environment.NewLine}");
                sb.Append($"                DynamicParameters param = new DynamicParameters();{Environment.NewLine}");

                foreach (var fk in foreignKeys)
                    sb.Append($"                param.Add(\"I_{fk.COLUMN_NAME}\", {fk.COLUMN_NAME.ToCamelCase()});{Environment.NewLine}");

                sb.Append($"{Environment.NewLine}");
                sb.Append($"                using (var connection = GetConnection()){Environment.NewLine}");
                sb.Append($"                {{{Environment.NewLine}");
                sb.Append($"                    toReturn = (await connection.QueryAsync<{tableName}>(\"zgen_{tableName}_GetByIds\", param, commandType: System.Data.CommandType.StoredProcedure).ConfigureAwait(false)).AsList();{Environment.NewLine}");
                sb.Append($"                }}{Environment.NewLine}");
                sb.Append($"{Environment.NewLine}");
                sb.Append($"            }}{Environment.NewLine}");
                sb.Append($"            catch (Exception ex){Environment.NewLine}");
                sb.Append($"            {{{Environment.NewLine}");
                sb.Append($"                logger.Error(\"{tableName}Repository->Get{tableName}ByIds\", ex);{Environment.NewLine}");
                sb.Append($"            }}{Environment.NewLine}");
                sb.Append($"{Environment.NewLine}");
                sb.Append($"            return toReturn;{Environment.NewLine}");
                sb.Append($"        }}{Environment.NewLine}");
                sb.Append($"{Environment.NewLine}");
            }

            if (stringColumns.Any())
            {
                sb.Append($"		public async Task<Paged<{tableName}>> GetPaged{tableName}Search(string filterString, int start, int pageSize){Environment.NewLine}");

                sb.Append($"        {{{Environment.NewLine}");
                sb.Append($"            Paged<{tableName}> toReturn = null;{Environment.NewLine}");
                sb.Append($"            List<{tableName}Count> countResult = null;{Environment.NewLine}");
                sb.Append($"            try{Environment.NewLine}");
                sb.Append($"            {{{Environment.NewLine}");
                sb.Append($"                DynamicParameters param = new DynamicParameters();{Environment.NewLine}");
                sb.Append($"                param.Add(\"I_FilterString\", filterString);{Environment.NewLine}");
                sb.Append($"                param.Add(\"I_Start\", start);{Environment.NewLine}");
                sb.Append($"                param.Add(\"I_End\", start + pageSize);{Environment.NewLine}");
                sb.Append($"{Environment.NewLine}");
                sb.Append($"                using (var connection = GetConnection()){Environment.NewLine}");
                sb.Append($"                {{{Environment.NewLine}");
                sb.Append($"                    countResult = (await connection.QueryAsync<{tableName}Count>(\"zgen_{tableName}_PagedSearch\", param, commandType: System.Data.CommandType.StoredProcedure).ConfigureAwait(false)).AsList();{Environment.NewLine}");
                sb.Append($"                }}{Environment.NewLine}");
                sb.Append($"{Environment.NewLine}");
                sb.Append($"                var totalEntry = countResult.FirstOrDefault();{Environment.NewLine}");
                sb.Append($"                toReturn = new Paged<{tableName}>(start, pageSize, totalEntry?.TotalCount ?? 0);{Environment.NewLine}");
                sb.Append($"                toReturn.Data.AddRange(countResult);{Environment.NewLine}");
                sb.Append($"            }}{Environment.NewLine}");
                sb.Append($"            catch (Exception ex){Environment.NewLine}");
                sb.Append($"            {{{Environment.NewLine}");
                sb.Append($"                logger.Error(\"{tableName}Repository->GetPaged{tableName}Search\", ex);{Environment.NewLine}");
                sb.Append($"            }}{Environment.NewLine}");
                sb.Append($"{Environment.NewLine}");
                sb.Append($"            return toReturn;{Environment.NewLine}");
                sb.Append($"        }}{Environment.NewLine}");
                sb.Append($"{Environment.NewLine}");
            }

            if (foreignKeys.Count() > 1 && columns.FilterOutUtility().Count() == foreignKeys.Count())
                foreach (var fc in foreignKeys)
                    foreach (var fc2 in foreignKeys.Where(s => s != fc))
                    {
                        var currentTableName = fc.COLUMN_NAME.Replace("Id", "");
                        var foreignTableName = fc2.COLUMN_NAME.Replace("Id", "");

                        sb.Append($"		public async Task<List<{currentTableName}>> Get{currentTableName}By{foreignTableName}Id({fc2.GetCSharpDataType()}? {fc2.COLUMN_NAME.ToCamelCase()}){Environment.NewLine}");
                        sb.Append($"        {{{Environment.NewLine}");
                        sb.Append($"            List<{currentTableName}> toReturn = null;{Environment.NewLine}");
                        sb.Append($"            try{Environment.NewLine}");
                        sb.Append($"            {{{Environment.NewLine}");
                        sb.Append($"                DynamicParameters param = new DynamicParameters();{Environment.NewLine}");
                        sb.Append($"                param.Add(\"I_{fc2.COLUMN_NAME}\", {fc2.COLUMN_NAME.ToCamelCase()});{Environment.NewLine}");

                        sb.Append($"{Environment.NewLine}");
                        sb.Append($"                using (var connection = GetConnection()){Environment.NewLine}");
                        sb.Append($"                {{{Environment.NewLine}");
                        sb.Append($"                    toReturn = (await connection.QueryAsync<{currentTableName}>(\"zgen_{tableName}_Get{currentTableName}By{foreignTableName}Id\", param, commandType: System.Data.CommandType.StoredProcedure).ConfigureAwait(false)).AsList();{Environment.NewLine}");
                        sb.Append($"                }}{Environment.NewLine}");
                        sb.Append($"{Environment.NewLine}");
                        sb.Append($"            }}{Environment.NewLine}");
                        sb.Append($"            catch (Exception ex){Environment.NewLine}");
                        sb.Append($"            {{{Environment.NewLine}");
                        sb.Append($"                logger.Error(\"{tableName}Repository->Get{currentTableName}By{foreignTableName}Id\", ex);{Environment.NewLine}");
                        sb.Append($"            }}{Environment.NewLine}");
                        sb.Append($"{Environment.NewLine}");
                        sb.Append($"            return toReturn;{Environment.NewLine}");
                        sb.Append($"        }}{Environment.NewLine}");
                        sb.Append($"{Environment.NewLine}");
                    }


            sb.Append($"    }}{Environment.NewLine}");
            sb.Append($"}}{Environment.NewLine}");

            return sb.ToString();
        }
    }
}
