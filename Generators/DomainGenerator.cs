using AddoCore.Db.MySql.Generator.Model;
using AddoCore.Db.MySql.Generator.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AddoCore.Db.MySql.Generator.Generators
{
    public static class DomainGenerator
    {
        public static void Run(string path, string nspace, string tableName, List<Column> columns)
        {
            var fileString = GenerateFileString(nspace, tableName, columns);
            var filename = $"{tableName}Service.Gen.cs";
            fileString.SaveTo($"{path}\\Domain\\{nspace}.Domain.Service\\Gen\\{filename}");
        }

        private static string GenerateFileString(string nspace, string tableName, List<Column> columns)
        {
            var primary = columns.GetPrimary();
            var uniqueColumn = columns.GetUnique();
            var foreignKeys = columns.GetForeignKeys();
            var ffk = foreignKeys.FirstOrDefault();
            var stringColumns = columns.GetStringColumns();

            StringBuilder sb = new StringBuilder();
            sb.Append($"using System;{Environment.NewLine}");
            sb.Append($"using System.Threading.Tasks;{Environment.NewLine}");
            sb.Append($"using System.Collections.Generic;{Environment.NewLine}");
            sb.Append($"using {nspace}.Dal.Interface.Gen;{Environment.NewLine}");
            sb.Append($"using {nspace}.Domain.Interface.Gen;{Environment.NewLine}");            

            sb.Append($"{Environment.NewLine}");
            sb.Append($"namespace {nspace}.Domain.Gen{Environment.NewLine}");
            sb.Append($"{{{Environment.NewLine}");

            sb.Append($"    public partial class {tableName}Service : I{tableName}Service{Environment.NewLine}");
            sb.Append($"    {{{Environment.NewLine}");
            sb.Append($"        private I{tableName}Repository {tableName.ToCamelCase()}repository;{Environment.NewLine}");
            sb.Append($"        public {tableName}Service(I{tableName}Repository {tableName.ToCamelCase()}repository){Environment.NewLine}");
            sb.Append($"        {{{Environment.NewLine}");
            sb.Append($"            this.{tableName.ToCamelCase()}repository = {tableName.ToCamelCase()}repository;{Environment.NewLine}");
            sb.Append($"        }}{Environment.NewLine}");
            sb.Append($"{Environment.NewLine}");

            sb.Append($"        public async Task<{nspace}.Domain.Model.Gen.{tableName}> InsUpd{tableName}({nspace}.Domain.Model.Gen.{tableName} {tableName.ToCamelCase()}){Environment.NewLine}");
            sb.Append($"        {{{Environment.NewLine}");
            sb.Append($"            return await {tableName.ToCamelCase()}repository.InsUpd{tableName}({tableName.ToCamelCase()});{Environment.NewLine}");
            sb.Append($"        }}{Environment.NewLine}");
            sb.Append($"{Environment.NewLine}");

            sb.Append($"		public async Task<List<{nspace}.Domain.Model.Gen.{tableName}>> Get{tableName}ById(int? {primary.COLUMN_NAME.ToCamelCase()}, bool? isActive){Environment.NewLine}");
            sb.Append($"        {{{Environment.NewLine}");
            sb.Append($"            return await {tableName.ToCamelCase()}repository.Get{tableName}ById({primary.COLUMN_NAME.ToCamelCase()}, isActive);{Environment.NewLine}");
            sb.Append($"        }}{Environment.NewLine}");
            sb.Append($"{Environment.NewLine}");

            if (uniqueColumn != null)
            {
                sb.Append($"		public async Task<{nspace}.Domain.Model.Gen.{tableName}> Get{tableName}By{uniqueColumn.COLUMN_NAME}({uniqueColumn.GetCSharpDataType()} {uniqueColumn.COLUMN_NAME.ToCamelCase()}){Environment.NewLine}");
                sb.Append($"        {{{Environment.NewLine}");
                sb.Append($"            return await {tableName.ToCamelCase()}repository.Get{tableName}By{uniqueColumn.COLUMN_NAME}({uniqueColumn.COLUMN_NAME.ToCamelCase()});{Environment.NewLine}");
                sb.Append($"        }}{Environment.NewLine}");
                sb.Append($"{Environment.NewLine}");
            }

            if (foreignKeys.Any())
            {
                sb.Append($"		public async Task<List<{nspace}.Domain.Model.Gen.{tableName}>> Get{tableName}ByIds(");
                foreach (var fk in foreignKeys)
                    sb.Append($"{(fk == ffk ? "" : ", ")}{fk.GetCSharpDataType()}? {fk.COLUMN_NAME.ToCamelCase()}");
                sb.Append($"){Environment.NewLine}");

                sb.Append($"        {{{Environment.NewLine}");
                sb.Append($"            return await {tableName.ToCamelCase()}repository.Get{tableName}ByIds(");

                foreach (var fk in foreignKeys)
                    sb.Append($"{(fk == ffk ? "" : ", ")}{fk.COLUMN_NAME.ToCamelCase()}");
                sb.Append($");{Environment.NewLine}");

                sb.Append($"        }}{Environment.NewLine}");
                sb.Append($"{Environment.NewLine}");
            }

            if (stringColumns.Any())
            {
                sb.Append($"		public async Task<{nspace}.Domain.Model.Model.Paged<{nspace}.Domain.Model.Gen.{tableName}>> GetPaged{tableName}Search(string filterString, int start, int pageSize){Environment.NewLine}");
                sb.Append($"        {{{Environment.NewLine}");
                sb.Append($"            return await {tableName.ToCamelCase()}repository.GetPaged{tableName}Search(filterString, start, pageSize);{Environment.NewLine}");
                sb.Append($"        }}{Environment.NewLine}");
                sb.Append($"{Environment.NewLine}");
            }

            if (foreignKeys.Count() > 1 && columns.FilterOutUtility().Count() == foreignKeys.Count())
                foreach (var fc in foreignKeys)
                    foreach (var fc2 in foreignKeys.Where(s => s != fc))
                    {
                        var currentTableName = fc.COLUMN_NAME.Replace("Id", "");
                        var foreignTableName = fc2.COLUMN_NAME.Replace("Id", "");

                        sb.Append($"		public async Task<List<{nspace}.Domain.Model.Gen.{currentTableName}>> Get{currentTableName}By{foreignTableName}Id({fc2.GetCSharpDataType()}? {fc2.COLUMN_NAME.ToCamelCase()}){Environment.NewLine}");
                        sb.Append($"        {{{Environment.NewLine}");
                        sb.Append($"            return await {tableName.ToCamelCase()}repository.Get{currentTableName}By{foreignTableName}Id({fc2.COLUMN_NAME.ToCamelCase()});{Environment.NewLine}");
                        sb.Append($"        }}{Environment.NewLine}");
                        sb.Append($"{Environment.NewLine}");
                    }

            sb.Append($"    }}{Environment.NewLine}");
            sb.Append($"}}{Environment.NewLine}");

            return sb.ToString();
        }
    }
}
