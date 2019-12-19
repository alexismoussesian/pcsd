using System.ComponentModel.DataAnnotations;

namespace pcsd.plugin.sql.DataModel.Dictionary
{
    public class PresenceDefinitions : IDictionaryItem
    {
        [Key]
        public string id { get; set; }
        public string name { get; set; }
        public string systemPresence { get; set; }
    }
}
