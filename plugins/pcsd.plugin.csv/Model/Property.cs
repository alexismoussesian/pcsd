using pcsd.plugin.csv.Mapping;

namespace pcsd.plugin.csv.Model
{
    public class Property : Reflectionable
    {        
        public string propertyType { get; set; }
        public string property { get; set; }
        public string value { get; set; }
    }
}
