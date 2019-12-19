using System;
using System.Configuration;
using System.Reflection;
using log4net;

namespace pcsd.plugin.csv
{
    class IntervalManager
    {
        private static readonly ILog Trace = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const int DefaultNumberOfDays = 7;

        public static DateTime GetIntervalUtc(IntervalTypes intervalType)
        {
            try
            {
                Trace.Debug($"GetIntervalUtc(), intervalType:{intervalType}");
                var config = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);
                var valueAsString = config.AppSettings.Settings[intervalType.ToString()].Value;
                var valueAsDate = DateTime.Parse(valueAsString);
                Trace.Info($"{intervalType} loaded: {valueAsDate.ToString("o")}");
                return valueAsDate;
            }
            catch 
            {                
                // <return a default value if interval does not exist>
                var dt = DateTime.Today.ToUniversalTime().AddDays(-DefaultNumberOfDays);
                Trace.Warn($"{intervalType} not found, taking the default one: {dt.ToString("o")}");
                return dt;
                // <return a default value if interval does not exist>
            }
        }

        public static void SetIntervalUtc(IntervalTypes intervalType, DateTime interval)
        {
            try
            {
                Trace.Debug($"SetIntervalUtc(), intervalType:{intervalType}, interval:{interval.ToString("o")}");
                var config = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);
                if (config.AppSettings.Settings[intervalType.ToString()] == null) { config.AppSettings.Settings.Add(intervalType.ToString(), ""); }
                config.AppSettings.Settings[intervalType.ToString()].Value = interval.ToString("o");
                config.Save(ConfigurationSaveMode.Modified);
                Trace.Info($"{intervalType} saved: {interval.ToString("o")}");
            }
            catch (Exception ex)
            {
                Trace.Fatal(ex);
            }

        }

        public enum IntervalTypes { Lastinterval, LastIntervalForAggregates }
    }
}
