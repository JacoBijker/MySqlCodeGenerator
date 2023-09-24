using AddoCore.Db.MySql.Generator.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AddoCore.Db.MySql.Generator.Utils
{
    public static class ColumnExtensions
    {
        public static IEnumerable<Column> FilterOutUtility(this IEnumerable<Column> columns)
        {
            return FilterOutDates(FilterPrimary(columns.Where(s => s.COLUMN_NAME != "IsActive")));
        }

        public static IEnumerable<Column> FilterOutDates(this IEnumerable<Column> columns)
        {
            return columns.Where(s => s.COLUMN_NAME != "CreateDT" && s.COLUMN_NAME != "UpdateDT");
        }

        public static Column GetPrimary(this IEnumerable<Column> columns)
        {
            return columns.First(s => s.COLUMN_KEY == "PRI");
        }

        public static Column GetUnique(this IEnumerable<Column> columns)
        {
            return columns.FirstOrDefault(s => s.COLUMN_KEY == "UNI");
        }

        public static IEnumerable<Column> GetForeignKeys(this IEnumerable<Column> columns)
        {
            return columns.Where(s => s.COLUMN_KEY == "MUL");
        }

        public static IEnumerable<Column> GetStringColumns(this IEnumerable<Column> columns)
        {
            return columns.Where(s => s.DATA_TYPE == "varchar");
        }

        public static IEnumerable<Column> FilterPrimary(this IEnumerable<Column> columns)
        {
            return FilterOutDates(columns.Where(s => s.COLUMN_KEY != "PRI"));
        }

        public static string GetCSharpDataType(this Column col)
        {
            switch (col.DATA_TYPE)
            {
                case "int":
                    return "int";
                case "datetime":
                    return "DateTime";
                case "varchar":
                    return "string";
                case "bit":
                    return "bool";
                case "decimal":
                    return "decimal";
                case "double":
                    return "double";
            }

            throw new ArgumentOutOfRangeException(col.DATA_TYPE);
        }
    }
}
