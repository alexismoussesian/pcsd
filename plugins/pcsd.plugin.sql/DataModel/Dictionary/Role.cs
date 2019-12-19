using System.ComponentModel.DataAnnotations;

namespace pcsd.plugin.sql.DataModel.Dictionary
{
    public class Role
    {
        [Key]
        public string id { get; set; }
        public string name { get; set; }
    }
}
