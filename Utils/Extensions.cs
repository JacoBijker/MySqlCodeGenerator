using System.IO;

namespace AddoCore.Db.MySql.Generator.Utils
{
    public static class Extensions
    {
        public static string ToCamelCase(this string @this)
        {
            return char.ToLowerInvariant(@this[0]) + @this.Substring(1);
        }

        public static void SaveTo(this string fileData, string path)
        {
            var directory = Path.GetDirectoryName(path);
            Directory.CreateDirectory(directory);
            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.WriteLine(fileData);
                sw.Close();
            }
        }

    }
}
