using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Reflection;
using System.Web.Script.Serialization;
using log4net;

namespace pcsd.plugin.sql.DataModel.ConversationMetric
{
    public class ConversationMetricManager
    {
        public IList<ConversationMetric> conversationMetrics { get; set; }
        private static readonly ILog Trace = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static IList<ConversationMetric> ParseMetrics(string jsonText)
        {
            var result = new List<ConversationMetric>();
            try
            {
                var json = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
                var parsed = json.Deserialize<ConversationMetricManager>(jsonText);
                return parsed.conversationMetrics;
            }
            catch (Exception ex)
            {
                Trace.Fatal(ex);
            }
            return result;
        }

        public static void SaveMetrics(IList<ConversationMetric> metrics, string connectionString)
        {
            try
            {
                var counter = new TransactionCounter();
                DatabaseContext dbCtx = null;
                foreach (var metric in metrics)
                {
                    ManageDatabaseContext(connectionString, counter, ref dbCtx);
                    if (dbCtx.ConversationMetrics.Any(x => x.SessionId.Equals(metric.SessionId, StringComparison.OrdinalIgnoreCase) && x.attrName.Equals(metric.attrName, StringComparison.OrdinalIgnoreCase)))
                    {
                        counter.RowsIgnored++;
                        continue;
                    }
                    dbCtx.ConversationMetrics.Add(metric);
                    counter.RowsAdded++;
                }
                Trace.Info($"Conversation metrics added:{counter.RowsAdded}, ignored:{counter.RowsIgnored}");
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
            const long batchSize = 1000; // number of attributes in one inserting batch
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
