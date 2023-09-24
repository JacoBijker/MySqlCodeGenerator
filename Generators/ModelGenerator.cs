using AddoCore.Db.MySql.Generator.Model;
using AddoCore.Db.MySql.Generator.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AddoCore.Db.MySql.Generator.Generators
{
    public class ModelGenerator
    {
        public static void Run(string path, string nspace, string tableName, List<Column> columns)
        {
            var fileString = GenerateModelFileString(nspace, tableName, columns);
            var filename = $"{tableName}.Gen.cs";
            fileString.SaveTo($"{path}\\Domain\\{nspace}.Domain.Model\\Gen\\Model\\{filename}");

            var stringColumns = columns.GetStringColumns();
            if (stringColumns.Any())
            {
                var countFileString = GenerateCountModelFileString(nspace, tableName, columns);
                var countFilename = $"{tableName}Count.Gen.cs";
                countFileString.SaveTo($"{path}\\Domain\\{nspace}.Domain.Model\\Gen\\Model\\{countFilename}");
            }
        }

        private static string GenerateModelFileString(string nspace, string tableName, List<Column> columns)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"using System;{Environment.NewLine}");
            sb.Append($"{Environment.NewLine}");
            sb.Append($"namespace {nspace}.Domain.Model.Gen{Environment.NewLine}");
            sb.Append($"{{{Environment.NewLine}");

            sb.Append($"    public partial class {tableName}{Environment.NewLine}");
            sb.Append($"    {{{Environment.NewLine}");

            foreach (var col in columns)
                sb.Append($"        public {col.GetCSharpDataType()} {col.COLUMN_NAME} {{ get; set; }}{Environment.NewLine}");

            sb.Append($"    }}{Environment.NewLine}");
            sb.Append($"}}{Environment.NewLine}");

            return sb.ToString();
        }

        private static string GenerateCountModelFileString(string nspace, string tableName, List<Column> columns)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"using System;{Environment.NewLine}");
            sb.Append($"{Environment.NewLine}");
            sb.Append($"namespace {nspace}.Domain.Model.Gen{Environment.NewLine}");
            sb.Append($"{{{Environment.NewLine}");

            sb.Append($"    public partial class {tableName}Count: {tableName}{Environment.NewLine}");
            sb.Append($"    {{{Environment.NewLine}");
            sb.Append($"        public int TotalCount {{ get; set; }}{Environment.NewLine}");
            sb.Append($"    }}{Environment.NewLine}");
            sb.Append($"}}{Environment.NewLine}");

            return sb.ToString();
        }


    }
}
