using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Script.Serialization;
using log4net;
using pcsd.plugin.sql.DataModel.ConversationAggregates;

namespace pcsd.plugin.sql.DataModel.UserAggregates
{
    class UserAggregatesManager
    {
        public IList<Userdata> userdata { get; set; }
        private static readonly ILog Trace = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static IList<UserAggregate> ParseUserAggregates(string jsonText)
        {
            var result = new List<UserAggregate>();
            try
            {
                var json = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
                var userDataList = json.Deserialize<UserAggregatesManager>(jsonText);
                result = GetFlattenUserAggregates(userDataList.userdata).ToList();
            }
            catch (Exception ex)
            {
                Trace.Fatal(ex);
            }
            return result;
        }

        private static IEnumerable<UserAggregate> GetFlattenUserAggregates(IEnumerable<Userdata> userDataList)
        {
            var result = new List<UserAggregate>();
            try
            {
                result.AddRange(userDataList.SelectMany(userData => userData.data, (userData, data) => new { userData, data }).SelectMany(@t => @t.data.metrics, (@t, metric) => new UserAggregate { userId = @t.userData.@group.userId ?? string.Empty, intervalUtc = @t.data.interval, metric = metric.metric, qualifier = metric.qualifier, sum = metric.stats.sum }));
            }
            catch (Exception ex)
            {
                Trace.Fatal(ex);
            }
            return result;
        }

        public static void SaveUserAggregates(IEnumerable<UserAggregate> userAggregateList, string connectionString)
        {
            try
            {
                var counter = new TransactionCounter();
                DatabaseContext dbCtx = null;
                foreach (var ua in userAggregateList)
                {
                    ManageDatabaseContext(connectionString, counter, ref dbCtx);
                    if (!dbCtx.UserAggregates.Any(x =>
                        x.userId.Equals(ua.userId, StringComparison.CurrentCultureIgnoreCase)                        
                        && x.intervalUtc.Equals(ua.intervalUtc, StringComparison.CurrentCultureIgnoreCase)
                        && x.metric.Equals(ua.metric, StringComparison.CurrentCultureIgnoreCase)
                        && x.qualifier.Equals(ua.qualifier, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        // add conversation aggregate
                        dbCtx.UserAggregates.Add(ua);
                        counter.RowsAdded++;
                    }
                    else
                    {
                        // conversation aggregate does exist
                        counter.RowsIgnored++;
                    }
                }
                Trace.Info($"User aggregates added:{counter.RowsAdded}, ignored:{counter.RowsIgnored}");
                if (dbCtx == null) return;
                dbCtx.SaveChanges();
                dbCtx.Dispose();
            }
            catch (Exception ex)
            {
                Trace.Error(ex);
            }
        }

        private static void ManageDatabaseContext(string connectionString, TransactionCounter counter, ref DatabaseContext databaseContext)
        {
            const long batchSize = 1000; // number of rows in one inserting batch
            if (databaseContext == null)
            {
                Trace.Debug("Db context is null, creating a new one.");
                databaseContext = new DatabaseContext(connectionString);
                return;
            }
            if (counter.RowsAdded == 0 || counter.RowsAdded % batchSize != 0) return;
            Trace.Debug($"Batch size reached {batchSize}, saving changes and recreating db context.");
            databaseContext.SaveChanges();
            databaseContext.Dispose();
            databaseContext = new DatabaseContext(connectionString);
        }
    }
}
