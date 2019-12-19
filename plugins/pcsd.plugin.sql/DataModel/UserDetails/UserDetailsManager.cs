using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Script.Serialization;
using log4net;

namespace pcsd.plugin.sql.DataModel.UserDetails
{
    class UserDetailsManager
    {
        public IList<UserDetail> userdetailsdata { get; set; }
        private static readonly ILog Trace = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static IList<UserDetail> ParseUserDetails(string jsonText)
        {
            IList<UserDetail> result = new List<UserDetail>();
            try
            {
                var json = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
                var parsed = json.Deserialize<UserDetailsManager>(jsonText);                
                return parsed.userdetailsdata;
            }
            catch (Exception ex)
            {
                Trace.Fatal(ex);
            }
            return result;
        }

        public static void SaveUserDetails(IList<UserDetail> userDetails, string connectionString)
        {
            try
            {
                var counterPrimaryPresence = new TransactionCounter();
                var counterRoutingStatus = new TransactionCounter();
                DatabaseContext dbCtx = null;
                foreach (var ud in userDetails)
                {
                    ManageDatabaseContext(connectionString, counterPrimaryPresence, ref dbCtx);
                    // <primary presence>
                    if (ud.primaryPresence != null && ud.primaryPresence.Any())
                    {
                        foreach (var pp in ud.primaryPresence)
                        {
                            if (pp.endTime == DateTime.MinValue) continue;
                            if (!dbCtx.PrimaryPresence.Any(x => x.userId.Equals(ud.userId) && x.startTime.Equals(pp.startTime) && x.systemPresence.Equals(pp.systemPresence, StringComparison.OrdinalIgnoreCase)))
                            {
                                pp.userId = ud.userId;  // as per API response the user id is parsed on UserDetai level so it must be assigned here before pushing row to the database
                                dbCtx.PrimaryPresence.Add(pp);
                                counterPrimaryPresence.RowsAdded++;
                            }
                            else
                            {
                                counterPrimaryPresence.RowsIgnored++;
                            }
                        }
                    }
                    // </primary presence>

                    // <routing status>
                    if (ud.routingStatus != null && ud.routingStatus.Any())
                    {
                        foreach (var rs in ud.routingStatus)
                        {
                            if (rs.endTime == DateTime.MinValue) continue;
                            if (!dbCtx.RoutingStatus.Any(x => x.userId.Equals(ud.userId) && x.startTime.Equals(rs.startTime) && x.routingStatus.Equals(rs.routingStatus, StringComparison.OrdinalIgnoreCase)))
                            {
                                rs.userId = ud.userId; // as per API response the user id is parsed on UserDetai level so it must be assigned here before pushing row to the database
                                dbCtx.RoutingStatus.Add(rs);
                                counterRoutingStatus.RowsAdded++;
                            }
                            else
                            {
                                counterRoutingStatus.RowsIgnored++;
                            }
                        }
                    }
                    // </routing status>
                }
                Trace.Info($"Primary presences added:{counterPrimaryPresence.RowsAdded}, ignored:{counterPrimaryPresence.RowsIgnored}");
                Trace.Info($"Routing statuses added:{counterRoutingStatus.RowsAdded}, ignored:{counterRoutingStatus.RowsIgnored}");
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
            //
            // Dividing inserts for smaller batches.
            // It is fix for the issue:
            // https://bitbucket.org/eccemea/purecloud-stats-dispatcher/issues/20/outofmemoryexception
            //
            const long batchSize = 200; // number of userDetails in one inserting batch
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
