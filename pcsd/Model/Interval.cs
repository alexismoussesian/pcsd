using System;
using System.Reflection;
using log4net;

namespace pcsd.Model
{
    public class Interval
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        public Interval(DateTime from, DateTime to)
        {
            Log.Debug($"Interval(), {nameof(from)}:{from:O}, {nameof(to)}:{to:O}");
            From = from;
            To = to;            
        }

        public override string ToString()
        {
            if (From == null) throw new NullReferenceException($"{nameof(From)} field can't be NULL");
            if (To == null) throw new NullReferenceException($"{nameof(To)} field can't be NULL");            
            var result = $"{From.Value:O}/{To.Value:O}";
            Log.Debug($"{nameof(result)}:{result}");            
            if (From >= To) throw new Exception("Date TO must be grather than date FROM.");
            return result;
        }
    }
}
