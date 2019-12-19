using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Script.Serialization;
using log4net;

namespace pcsd.plugin.sql.DataModel.ConversationAggregates
{
    class ConversationAggregatesManager
    {
        public IList<Queuedata> queuedata { get; set; }
        private static readonly ILog Trace = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static IList<ConversationAggregate> ParseConversationAggregates(string jsonText)
        {
            var result = new List<ConversationAggregate>();
            try
            {
                var json = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
                var queueDataList = json.Deserialize<ConversationAggregatesManager>(jsonText);
                result = GetFlattenConversationAggregates(queueDataList.queuedata).ToList();
            }
            catch (Exception ex)
            {
                Trace.Fatal(ex);
            }
            return result;
        }

        private static IEnumerable<ConversationAggregate> GetFlattenConversationAggregates(IEnumerable<Queuedata> queueDataList)
        {
            var result = new List<ConversationAggregate>();
            try
            {                
                result.AddRange(queueDataList.SelectMany(queueData => queueData.data, (queueData, data) => new {queueData, data}).SelectMany(@t => @t.data.metrics, (@t, metric) => new ConversationAggregate{mediaType = @t.queueData.@group.mediaType, queueId = @t.queueData.@group.queueId ?? string.Empty, intervalUtc = @t.data.interval, metric = metric.metric, count = metric.stats.count, min = metric.stats.min, max = metric.stats.max, sum = metric.stats.sum}));
            }
            catch (Exception ex)
            {
                Trace.Fatal(ex);
            }
            return result;
        }

        public static void SaveConversationAggregates(IEnumerable<ConversationAggregate> conversationAggregateList, string connectionString)
        {
            try
            {
                var counter = new TransactionCounter();
                DatabaseContext dbCtx = null;
                foreach (var ca in conversationAggregateList)
                {
                    ManageDatabaseContext(connectionString, counter, ref dbCtx);
                    if (!dbCtx.ConversationAggregates.Any(x =>
                        x.queueId.Equals(ca.queueId, StringComparison.CurrentCultureIgnoreCase)
                        && x.mediaType.Equals(ca.mediaType, StringComparison.CurrentCultureIgnoreCase)
                        && x.intervalUtc.Equals(ca.intervalUtc, StringComparison.CurrentCultureIgnoreCase)
                        && x.metric.Equals(ca.metric, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        // add conversation aggregate
                        dbCtx.ConversationAggregates.Add(ca);
                        counter.RowsAdded++;
                    }
                    else
                    {
                        // conversation aggregate does exist
                        counter.RowsIgnored++;
                    }
                }
                Trace.Info($"Conversations aggregates added:{counter.RowsAdded}, ignored:{counter.RowsIgnored}");
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
