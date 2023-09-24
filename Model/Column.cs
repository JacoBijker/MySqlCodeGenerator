using System;
using System.Collections.Generic;
using System.Text;

namespace AddoCore.Db.MySql.Generator.Model
{
    public class Column
    {
        public string TABLE_SCHEMA { get; set; }
        public string TABLE_NAME { get; set; }
        public string COLUMN_NAME { get; set; }
        public string ORDINAL_POSITION { get; set; }
        public string COLUMN_DEFAULT { get; set; }
        public string IS_NULLABLE { get; set; }
        public string DATA_TYPE { get; set; }
        public string COLUMN_TYPE { get; set; }
        public string COLUMN_KEY { get; set; }
        public string EXTRA { get; set; }
    }
}
