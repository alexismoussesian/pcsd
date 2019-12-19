using System;
using System.Linq;
using System.Reflection;
using log4net;

namespace pcsd.plugin.sql.DataModel.Interval
{
    class IntervalManager
    {
        private static readonly ILog Trace = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const int DefaultNumberOfDays = 6;

        public static void SetIntervalUtc(DataModel.Interval.Interval.IntervalTypes intervalType, DateTime lastIntervalUtc, string connectionString)
        {
            Trace.Debug($"SetInterval(), intervalType:{intervalType}, lastIntervalUtc:{lastIntervalUtc.ToString("o")}");
            using (var dbCtx = new DatabaseContext(connectionString))
            {
                try
                {
                    // <update interval if does exist>
                    var interval = (from i in dbCtx.Intervals
                        where i.IntervalType == intervalType
                        select i).FirstOrDefault();
                    if (interval != null)
                    {
                        Trace.Debug("Updating the existing interval.");
                        interval.LastIntervalUtc = lastIntervalUtc;
                        dbCtx.SaveChanges();
                        return;
                    }
                    // </update interval if does exist>

                    // <create an interval if doesn't exist>
                    Trace.Debug("Creating a new interval.");
                    dbCtx.Intervals.Add(new DataModel.Interval.Interval { IntervalType = intervalType, LastIntervalUtc = lastIntervalUtc });
                    dbCtx.SaveChanges();
                    // <create an interval if doesn't exist>
                }
                catch (Exception ex)
                {
                    Trace.Fatal(ex);
                }
            }
        }

        public static DateTime GetIntervalUtc(Interval.IntervalTypes intervalType, string connectionString)
        {
            Trace.Info($"GetInterval(), intervalType:{intervalType}");
            using (var dbCtx = new DatabaseContext(connectionString))
            {
                try
                {
                    // <get interval>
                    var interval = (from i in dbCtx.Intervals
                                    where i.IntervalType == intervalType
                                    select i).FirstOrDefault();
                    if (interval != null)
                    {
                        Trace.Info($"{interval.IntervalType} was found: {interval.LastIntervalUtc.ToString("o")}");
                        return interval.LastIntervalUtc;
                    }
                    // </get interval>
                }
                catch (Exception ex)
                {
                    Trace.Fatal(ex);
                }
                // <return a default value if interval does not exist>
                var dt = DateTime.Today.ToUniversalTime().AddDays(-DefaultNumberOfDays);
                Trace.Info($"A new value for {intervalType} was created: {dt.ToString("o")}");
                return dt;
                // </return a default value if interval does not exist>
            }
        }
    }
}

