using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Reflection;
using System.Web.Script.Serialization;
using log4net;

namespace pcsd.plugin.sql.DataModel.DataTableRow
{
    public class DataTableRowManager
    {
        public IList<DataTableRows> entities { get; set; }
        private static readonly ILog Trace = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static IList<DataTableRows> ParseDataTableRows(string jsonText)
        {
            var result = new List<DataTableRows>();
            try
            {
                var json = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
                var parsed = json.Deserialize<DataTableRowManager>(jsonText);
                return parsed.entities;
            }
            catch (Exception ex)
            {
                Trace.Fatal(ex);
            }
            return result;
        }


        public static void SaveDataTablesRows(IList<DataTableRows> dataTableId, string connectionString)
        {
            try
            {
                var counter = new TransactionCounter();
                DatabaseContext dbCtx = null;
                foreach (var row in dataTableId)
                {
                    ManageDatabaseContext(connectionString, counter, ref dbCtx);
                    if (dbCtx.DataTableRows.Any(x => x.dataTableId.Equals( row.dataTableId, StringComparison.OrdinalIgnoreCase) ))
                    {
                        counter.RowsIgnored++;
                        continue;
                    }
                    dbCtx.DataTableRows.Add(row);
                    counter.RowsAdded++;
                }
                Trace.Info($"DataTables Rows added:{counter.RowsAdded}, ignored:{counter.RowsIgnored}");
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
