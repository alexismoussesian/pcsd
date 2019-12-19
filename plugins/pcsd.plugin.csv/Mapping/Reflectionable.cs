using System;
using System.Collections.Generic;
using System.Linq;

namespace pcsd.plugin.csv.Mapping
{
    public class Reflectionable
    {
        public string GetPropertyValue(string propertyName, string dateTimeMask = "")
        {
            var result = string.Empty;
            try
            {
                var value = GetType().GetProperties().Single(x => x.Name == propertyName).GetValue(this, null);
                if (!string.IsNullOrWhiteSpace(dateTimeMask) && value.GetType().FullName == "System.DateTime")
                {                    
                    result = ((DateTime)value).ToString(dateTimeMask);
                }
                else
                {
                    result = value.ToString();
                }
            }
            catch {}
            return result;
        }

        public List<string> GetPropertyMultiValue(string propertyName)
        {
            var result = new List<string>();
            try
            {
               var value = GetType().GetProperties().Single(x => x.Name == propertyName).GetValue(this, null);
               if (value != null) result.AddRange((List<string>)value);
            }
            catch { }
            return result;
        }
    }
}
