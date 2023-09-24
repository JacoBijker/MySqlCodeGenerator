using AddoCore.Db.MySql.Generator.Model;
using AddoCore.Db.MySql.Generator.Utils;
using Dapper;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AddoCore.Db.MySql.Generator.Generators
{
    public static class StoredProcGenerator
    {
        public static void Run(string connectionString, string tableName, List<Column> columns)
        {
            ExecuteScript(connectionString, GenerateInsUpd(tableName, columns));
            ExecuteScript(connectionString, GenerateGetById(tableName, columns));

            var uniqueColumn = columns.GetUnique();
            if (uniqueColumn != null)
                ExecuteScript(connectionString, GenerateGetByUniqueId(tableName, uniqueColumn));

            var foreignColumns = columns.GetForeignKeys();
            if (foreignColumns.Any())
                ExecuteScript(connectionString, GenerateGetByIds(tableName, foreignColumns));

            if (foreignColumns.Count() > 1 && columns.FilterOutUtility().Count() == foreignColumns.Count())
                foreach(var fc in foreignColumns)
                    foreach(var fc2 in foreignColumns.Where(s => s != fc))
                        ExecuteScript(connectionString, GenerateGetByForeignIds(tableName, fc, fc2));

            var stringColumns = columns.GetStringColumns();
            if (stringColumns.Any())
                ExecuteScript(connectionString, GeneratePagedSearch(tableName, stringColumns));

        }

        private static void ExecuteScript(string connectionString, string script)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
                conn.Execute(script);
        }

        private static string GenerateGetById(string tableName, List<Column> columns)
        {
            StringBuilder sb = new StringBuilder();
            var primary = columns.GetPrimary();

            sb.Append($"CREATE PROCEDURE `zgen_{tableName}_GetById`{Environment.NewLine}");
            sb.Append($"(IN I_{primary.COLUMN_NAME} INTEGER,{Environment.NewLine}");
            sb.Append($" IN I_IsActive BIT){Environment.NewLine}");
            sb.Append($"BEGIN{Environment.NewLine}");
            sb.Append($"	IF I_{primary.COLUMN_NAME} IS NULL THEN{Environment.NewLine}");
            sb.Append($"    BEGIN{Environment.NewLine}");
            sb.Append($"      IF I_IsActive IS NULL THEN{Environment.NewLine}");
            sb.Append($"	  BEGIN{Environment.NewLine}");
            sb.Append($"        SELECT * FROM {tableName} ORDER BY {primary.COLUMN_NAME} ASC;{Environment.NewLine}");
            sb.Append($"	  END;{Environment.NewLine}");
            sb.Append($"      ELSE{Environment.NewLine}");
            sb.Append($"	    SELECT * FROM {tableName} WHERE IsActive = I_IsActive ORDER BY {primary.COLUMN_NAME} ASC;{Environment.NewLine}");
            sb.Append($"	  END IF;{Environment.NewLine}");
            sb.Append($"    END;{Environment.NewLine}");
            sb.Append($"	ELSE{Environment.NewLine}");
            sb.Append($"    BEGIN{Environment.NewLine}");
            sb.Append($"      IF I_IsActive IS NULL THEN{Environment.NewLine}");
            sb.Append($"	  BEGIN{Environment.NewLine}");
            sb.Append($"	    SELECT * FROM {tableName} WHERE {primary.COLUMN_NAME} = I_{primary.COLUMN_NAME} ORDER BY {primary.COLUMN_NAME} ASC;{Environment.NewLine}");
            sb.Append($"	  END;{Environment.NewLine}");
            sb.Append($"      ELSE	  {Environment.NewLine}");
            sb.Append($"	    SELECT * FROM {tableName} WHERE {primary.COLUMN_NAME} = I_{primary.COLUMN_NAME} AND IsActive = I_IsActive ORDER BY {primary.COLUMN_NAME} ASC;{Environment.NewLine}");
            sb.Append($"	  END IF;{Environment.NewLine}");
            sb.Append($"    END;{Environment.NewLine}");
            sb.Append($"	END IF;{Environment.NewLine}");
            sb.Append($"END;");

            return sb.ToString();
        }

        private static string GenerateGetByUniqueId(string tableName, Column uniqueColumn)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"CREATE PROCEDURE `zgen_{tableName}_GetBy{uniqueColumn.COLUMN_NAME}`{Environment.NewLine}");
            sb.Append($"(IN I_{uniqueColumn.COLUMN_NAME} {uniqueColumn.COLUMN_TYPE}){Environment.NewLine}");
            sb.Append($"BEGIN{Environment.NewLine}");
            sb.Append($"    SELECT * FROM {tableName} WHERE {uniqueColumn.COLUMN_NAME} = I_{uniqueColumn.COLUMN_NAME}; {Environment.NewLine}");
            sb.Append($"END;");

            return sb.ToString();
        }

        private static string GeneratePagedSearch(string tableName, IEnumerable<Column> stringColumns)
        {
            var first = stringColumns.First();

            StringBuilder sb = new StringBuilder();
            sb.Append($"CREATE PROCEDURE `zgen_{tableName}_PagedSearch`({Environment.NewLine}");
            sb.Append($" IN I_FilterString VARCHAR(256),{Environment.NewLine}");
            sb.Append($" IN I_Start INT,{Environment.NewLine}");
            sb.Append($" IN I_End INT{Environment.NewLine}");
            sb.Append($"){Environment.NewLine}");

            sb.Append($"BEGIN{Environment.NewLine}");
            sb.Append($"    SET I_FilterString := IFNULL(I_FilterString, '');{Environment.NewLine}");
            sb.Append($"    SET @Count := (SELECT COUNT(*) FROM {tableName} WHERE");
            foreach (var sc in stringColumns)
                sb.Append($"{(first == sc ? "" : "OR")} {sc.COLUMN_NAME} LIKE CONCAT('%',I_FilterString,'%') ");
            sb.Append($");{Environment.NewLine}");

            sb.Append($"    SET @Statement := CONCAT(\" SELECT *, \", CAST(@Count AS CHAR), \" as TotalCount FROM {tableName} WHERE");
            foreach (var sc in stringColumns)
                sb.Append($"{(first == sc ? "" : "OR")} {sc.COLUMN_NAME} LIKE '%\", I_FilterString, \"%' ");

            sb.Append($" LIMIT ?,?; \");{Environment.NewLine}");

            sb.Append($"    PREPARE STMT FROM @Statement;{Environment.NewLine}");
            sb.Append($"    SET @Start = I_Start;{Environment.NewLine}");
            sb.Append($"    SET @End = I_End;{Environment.NewLine}");
            sb.Append($"    EXECUTE STMT USING @Start, @End;{Environment.NewLine}");
            sb.Append($"    DEALLOCATE PREPARE STMT;{Environment.NewLine}");

            sb.Append($"END;");

            return sb.ToString();
        }

        private static string GenerateGetByIds(string tableName, IEnumerable<Column> foreignKeys)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"CREATE PROCEDURE `zgen_{tableName}_GetByIds`({Environment.NewLine}");

            var first = foreignKeys.First();
            var last = foreignKeys.Last();
            foreach (var fk in foreignKeys)
                sb.Append($" IN I_{fk.COLUMN_NAME} {fk.COLUMN_TYPE}{(fk == last ? "" : ",")}{Environment.NewLine}");

            sb.Append($"){Environment.NewLine}");

            sb.Append($"BEGIN{Environment.NewLine}");
            sb.Append($"   SELECT * FROM {tableName} WHERE ");

            foreach (var fk in foreignKeys)
                sb.Append($"{(fk == first ? "" : " AND ")}(ISNULL(I_{fk.COLUMN_NAME}) OR {fk.COLUMN_NAME} = I_{fk.COLUMN_NAME})");

            sb.Append($";{Environment.NewLine}");
            sb.Append($"END;");

            return sb.ToString();
        }

        private static string GenerateGetByForeignIds(string tableName, Column current, Column foreign)
        {
            var currentTableName = current.COLUMN_NAME.Replace("Id", "");
            var foreignTableName = foreign.COLUMN_NAME.Replace("Id", "");

            StringBuilder sb = new StringBuilder();
            sb.Append($"CREATE PROCEDURE `zgen_{tableName}_Get{currentTableName}By{foreignTableName}Id`({Environment.NewLine}");

            sb.Append($" IN I_{foreign.COLUMN_NAME} {foreign.COLUMN_TYPE}{Environment.NewLine}");

            sb.Append($"){Environment.NewLine}");

            sb.Append($"BEGIN{Environment.NewLine}");
            sb.Append($"   SELECT * FROM {currentTableName} J1 INNER JOIN {tableName} J2 ON J1.{current.COLUMN_NAME} = J2.{current.COLUMN_NAME} WHERE ");

            sb.Append($"(ISNULL(I_{foreign.COLUMN_NAME}) OR J2.{foreign.COLUMN_NAME} = I_{foreign.COLUMN_NAME})");

            sb.Append($";{Environment.NewLine}");
            sb.Append($"END;");

            return sb.ToString();
        }


        private static string GenerateInsUpd(string tableName, List<Column> columns)
        {
            var rCols = columns.FilterOutDates();
            var fCols = columns.FilterPrimary();
            var primary = columns.GetPrimary();
            var last = rCols.Last();

            StringBuilder sb = new StringBuilder();

            sb.Append($"CREATE PROCEDURE `zgen_{tableName}_InsUpd` {Environment.NewLine}");
            sb.Append($"(" + Environment.NewLine);

            foreach (var col in rCols)
                sb.Append($" IN I_{col.COLUMN_NAME} {col.COLUMN_TYPE}{(last == col ? "" : ",")} {Environment.NewLine}");

            sb.Append($")" + Environment.NewLine);
            sb.Append($"BEGIN" + Environment.NewLine);
            sb.Append($"   IF(I_{primary.COLUMN_NAME} > 0) THEN" + Environment.NewLine);
            sb.Append($"   BEGIN" + Environment.NewLine);
            sb.Append($"       UPDATE {tableName}" + Environment.NewLine);
            sb.Append($"        Set " + Environment.NewLine);

            foreach (var col in fCols)
                sb.Append($"            {col.COLUMN_NAME} = I_{col.COLUMN_NAME}, {Environment.NewLine}");
            sb.Append($"            UpdateDT = NOW() {Environment.NewLine}");
            sb.Append($"        WHERE {primary.COLUMN_NAME} = I_{primary.COLUMN_NAME};" + Environment.NewLine);

            sb.Append($"   END;" + Environment.NewLine);
            sb.Append($"   ELSE" + Environment.NewLine);
            sb.Append($"   BEGIN" + Environment.NewLine);
            sb.Append($"        INSERT INTO {tableName}({primary.COLUMN_NAME}, {string.Join(", ", fCols.Select(s => s.COLUMN_NAME))}, UpdateDT, CreateDT)" + Environment.NewLine);
            sb.Append($"        VALUES(NULL, {string.Join(", ", fCols.Select(s => "I_" + s.COLUMN_NAME))}, NOW(), NOW());" + Environment.NewLine);
            sb.Append($"        SET I_{primary.COLUMN_NAME} := LAST_INSERT_ID(); " + Environment.NewLine);
            sb.Append($"   END; " + Environment.NewLine);
            sb.Append($"   END IF; " + Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append($"   SELECT * FROM {tableName} Where {primary.COLUMN_NAME} = I_{primary.COLUMN_NAME}; " + Environment.NewLine);
            sb.Append($"END;" + Environment.NewLine);

            return sb.ToString();
        }

    }
}
