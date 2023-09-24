using AddoCore.Db.MySql.Generator.Generators;
using AddoCore.Db.MySql.Generator.Model;
using Dapper;
using Microsoft.Extensions.CommandLineUtils;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AddoCore.Db.MySql.Generator
{
    class Program
    {
        static void Main(string[] args)
        {
            CommandLineApplication commandLineApplication = new CommandLineApplication(throwOnUnexpectedArg: false);

            CommandOption path = commandLineApplication.Option("-p |--path <Path>", "Path to create and save generated model class files", CommandOptionType.SingleValue);
            CommandOption classNamespace = commandLineApplication.Option("-n |--classNamespace <ClassNamespace>", "Class namespace", CommandOptionType.SingleValue);
            CommandOption conString = commandLineApplication.Option("-c |--constring <ConnectionString>", "Database connection string", CommandOptionType.MultipleValue);
            CommandOption schema = commandLineApplication.Option("-s |--schema <DbSchema>", "Database schema name", CommandOptionType.SingleValue);

            commandLineApplication.HelpOption("-? | -h | --help");
            commandLineApplication.OnExecute(() =>
            {
                Console.WriteLine($"Path : {path.Value()}");
                Console.WriteLine($"ClassNamepsace : {classNamespace.Value()}");
                Console.WriteLine($"ConnectionString : {conString.Value()}");
                Console.WriteLine($"DB Schema: {schema.Value()}");

                if (path.HasValue() && classNamespace.HasValue() && conString.HasValue() && schema.HasValue())
                    Process(path.Value(), classNamespace.Value(), conString.Value(), schema.Value());
                else
                {
                    Console.WriteLine("ERROR");
                    Console.WriteLine($"Path, Class-namespace, Connection-string and Schema is required");
                }

                return 0;
            });
            commandLineApplication.Execute(args);
        }

        private static void Process(string path, string classNamespace, string connectionString, string schema)
        {
            var allProcs = GetDbGeneratedProcs(connectionString, schema).Result;
            foreach (var proc in allProcs)
                DropProcedure(connectionString, proc).Wait();

            var allColumns = GetDbColumns(connectionString, schema).Result;
            var columns = allColumns.GroupBy(s => s.TABLE_NAME).ToDictionary(s => s.Key, s => s.AsList());

            foreach (var tableColumns in columns)
            {
                if (!char.IsUpper(tableColumns.Key[0]) || tableColumns.Key.StartsWith("History_"))
                    continue;

                StoredProcGenerator.Run(connectionString, tableColumns.Key, tableColumns.Value);
                ModelGenerator.Run(path, classNamespace, tableColumns.Key, tableColumns.Value);
                IDomainGenerator.Run(path, classNamespace, tableColumns.Key, tableColumns.Value);
                DomainGenerator.Run(path, classNamespace, tableColumns.Key, tableColumns.Value);
                IDalGenerator.Run(path, classNamespace, tableColumns.Key, tableColumns.Value);
                DalGenerator.Run(path, classNamespace, tableColumns.Key, tableColumns.Value);
            }
        }

        public static async Task DropProcedure(string connectionString, StoredProcedure proc)
        {
            var dropSql = $"DROP procedure {proc.Db}.{proc.Name}";
            using (MySqlConnection conn = new MySqlConnection(connectionString))
                await conn.ExecuteAsync(dropSql);
        }

        public static async Task<List<Column>> GetDbColumns(string connectionString, string schemaName)
        {
            var selectSql = "SELECT TABLE_SCHEMA, TABLE_NAME, COLUMN_NAME, ORDINAL_POSITION, COLUMN_DEFAULT, IS_NULLABLE, DATA_TYPE, COLUMN_TYPE, COLUMN_KEY, EXTRA FROM `INFORMATION_SCHEMA`.`COLUMNS` " +
                            "WHERE `TABLE_SCHEMA`= @I_SCHEMA " +
                            "ORDER BY TABLE_NAME, ORDINAL_POSITION";

            List<Column> toReturn = new List<Column>();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                DynamicParameters dParams = new DynamicParameters();
                dParams.Add("@I_SCHEMA", schemaName);
                toReturn = (await conn.QueryAsync<Column>(selectSql, dParams, commandType: System.Data.CommandType.Text)).AsList();
            }

            return toReturn;
        }

        public static async Task<List<StoredProcedure>> GetDbGeneratedProcs(string connectionString, string schemaName)
        {
            var selectSql = "SHOW PROCEDURE STATUS WHERE Db = @I_SCHEMA AND Name LIKE 'zgen%'";

            List<StoredProcedure> toReturn = new List<StoredProcedure>();
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                DynamicParameters dParams = new DynamicParameters();
                dParams.Add("@I_SCHEMA", schemaName);
                toReturn = (await conn.QueryAsync<StoredProcedure>(selectSql, dParams, commandType: System.Data.CommandType.Text)).AsList();
            }

            return toReturn;
        }
    }
}
