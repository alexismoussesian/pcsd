using System;
using System.ComponentModel.DataAnnotations;

namespace pcsd.plugin.sql.DataModel.Interval
{
    public class Interval
    {
        [Key]
        public IntervalTypes IntervalType { get; set; }
        public DateTime LastIntervalUtc { get; set; }
        public enum IntervalTypes { Lastinterval, LastIntervalForAggregates }
    }
}
