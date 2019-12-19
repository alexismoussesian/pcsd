using System.Linq;

namespace pcsd.plugin.csv.Mapping
{
    public class Item
    {
        public string path { get; set; }
        public string dictionary { get; set; }
        public bool isArray { get; set; }
        public bool clean { get; set; }
        public int? maxLen { get; set; }
        /// <summary>
        /// For date time mask use .NET formats:
        /// https://msdn.microsoft.com/en-us/library/az4se3k1(v=vs.110).aspx
        /// https://msdn.microsoft.com/en-us/library/8kb3ddd4(v=vs.110).aspx
        /// </summary>
        public string dateTimeMask { get; set; }
        public string Name => path.Split('.').Last();
        public string Parent => path.Split('.').Count() < 2 ? string.Empty : path.Split('.')[path.Split('.').Length-2];
    }
}
