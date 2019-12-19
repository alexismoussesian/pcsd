using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Reflection;
using System.Web.Script.Serialization;
using log4net;

namespace pcsd.plugin.sql.DataModel.GroupMember
{
    public class GroupMemberManager
    {
        public IList<GroupMember> groupMember { get; set; }
        private static readonly ILog Trace = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static IList<GroupMember> ParseGroupMember(string jsonText)
        {
            var result = new List<GroupMember>();
            try
            {
                var json = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
                var parsed = json.Deserialize<GroupMemberManager>(jsonText);
                return parsed.groupMember;
            }
            catch (Exception ex)
            {
                Trace.Fatal(ex);
            }
            return result;
        }


        public static void SaveGroupMembers(IList<GroupMember> groupMember, string connectionString)
        {
            try
            {
                var counter = new TransactionCounter();
                DatabaseContext dbCtx = null;
                foreach (var member in groupMember)
                {
                    ManageDatabaseContext(connectionString, counter, ref dbCtx);
                    if (dbCtx.GroupMembers.Any(x => x.id.Equals(member.id, StringComparison.OrdinalIgnoreCase) && x.groupId.Equals(member.groupId, StringComparison.OrdinalIgnoreCase) ))
                    {
                        counter.RowsIgnored++;
                        continue;
                    }
                    dbCtx.GroupMembers.Add(member);
                    counter.RowsAdded++;
                }
                Trace.Info($"Group Members added:{counter.RowsAdded}, ignored:{counter.RowsIgnored}");
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
