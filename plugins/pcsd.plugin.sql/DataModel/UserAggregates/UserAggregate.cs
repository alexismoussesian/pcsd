using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pcsd.plugin.sql.DataModel.UserAggregates
{
    /// <summary>
    /// Flatten representation of Userdata
    /// </summary>
    public class UserAggregate
    {
        [Key]
        [Column(Order = 1)]
        public string userId { get; set; }
        [Key]
        [Column(Order = 2)]
        public string intervalUtc { get; set; }
        public DateTime intervalStartLocal
        {
            get
            {
                if (!intervalUtc.Contains("/")) return DateTime.MinValue;
                var dateAsString = intervalUtc.Split('/')[0];
                DateTime date;
                return DateTime.TryParse(dateAsString, out date) ? date : DateTime.MinValue; ;
            }
            set
            {
                // DO NOT DELETE THE SETTER!
                // IT FORCES EF TO CREATE A DB FIELD FOR THIS PROPERTY.
            }
        }
        public DateTime intervalEndLocal
        {
            get
            {
                if (!intervalUtc.Contains("/")) return DateTime.MinValue; ;
                var dateAsString = intervalUtc.Split('/')[1];
                DateTime date;
                return DateTime.TryParse(dateAsString, out date) ? date : DateTime.MinValue;
            }
            set
            {
                // DO NOT DELETE THE SETTER!
                // IT FORCES EF TO CREATE A DB FIELD FOR THIS PROPERTY.
            }
        }
        [Key]
        [Column(Order = 3)]
        public string metric { get; set; }
        [Key]
        [Column(Order = 4)]
        public string qualifier { get; set; }
        public double? sum { get; set; }
    }
}
