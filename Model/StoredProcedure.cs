using System;

namespace AddoCore.Db.MySql.Generator.Model
{
    public class StoredProcedure
    {
        public string Db { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Definer { get; set; }
        public DateTime Modified { get; set; }
        public DateTime Created { get; set; }
    }
}
