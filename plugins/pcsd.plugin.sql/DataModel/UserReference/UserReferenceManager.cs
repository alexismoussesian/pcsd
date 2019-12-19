using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Reflection;
using System.Web.Script.Serialization;
using log4net;

namespace pcsd.plugin.sql.DataModel.UserReference
{
    public class UserReferenceManager
    {
        //public IList<UserRole> userRole { get; set; }
        private static readonly ILog Trace = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        //public static IList<UserRole> ParseUserRole(string jsonText)
        //{
        //    var result = new List<UserRole>();
        //    try
        //    {
        //        var json = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
        //        var parsed = json.Deserialize<UserReferenceManager>(jsonText);
        //        return parsed.userRole;
        //    }
        //    catch (Exception ex)
        //    {
        //        Trace.Fatal(ex);
        //    }
        //    return result;
        //}


        public static void SaveUserRoles(IList<UserRole> userRole, string connectionString)
        {
            try
            {
                var counter = new TransactionCounter();
                DatabaseContext dbCtx = null;
                foreach (var user in userRole)
                {
                    ManageDatabaseContext(connectionString, counter, ref dbCtx);
                    if (dbCtx.userRoles.Any(x => x.UserId.Equals(user.UserId, StringComparison.OrdinalIgnoreCase) && x.Roles.Equals(user.Roles, StringComparison.OrdinalIgnoreCase)))
                    {
                        counter.RowsIgnored++;
                        continue;
                    }
                    dbCtx.userRoles.Add(user);
                    counter.RowsAdded++;
                }
                Trace.Info($"User Roles added:{counter.RowsAdded}, ignored:{counter.RowsIgnored}");
                if (dbCtx == null) return;
                dbCtx.SaveChanges();
                dbCtx.Dispose();
            }
            catch (Exception ex)
            {
                Trace.Error(ex);
            }
        }

        public static void SaveUserSkills(IList<UserSkill> userSkill, string connectionString)
        {
            try
            {
                var counter = new TransactionCounter();
                DatabaseContext dbCtx = null;
                foreach (var user in userSkill)
                {
                    ManageDatabaseContext(connectionString, counter, ref dbCtx);
                    if (dbCtx.UserSkills.Any(x => x.UserId.Equals(user.UserId, StringComparison.OrdinalIgnoreCase) && x.Skill.Equals(user.Skill, StringComparison.OrdinalIgnoreCase)))
                    {
                        counter.RowsIgnored++;
                        continue;
                    }
                    dbCtx.UserSkills.Add(user);
                    counter.RowsAdded++;
                }
                Trace.Info($"User Skills added:{counter.RowsAdded}, ignored:{counter.RowsIgnored}");
                if (dbCtx == null) return;
                dbCtx.SaveChanges();
                dbCtx.Dispose();
            }
            catch (Exception ex)
            {
                Trace.Error(ex);
            }
        }

        public static void SaveUserQueues(IList<UserQueue> userQueue, string connectionString)
        {
            try
            {
                var counter = new TransactionCounter();
                DatabaseContext dbCtx = null;
                foreach (var user in userQueue)
                {
                    ManageDatabaseContext(connectionString, counter, ref dbCtx);
                    if (dbCtx.UserQueues.Any(x => x.UserId.Equals(user.UserId, StringComparison.OrdinalIgnoreCase) && x.Queue.Equals(user.Queue, StringComparison.OrdinalIgnoreCase)))
                    {
                        counter.RowsIgnored++;
                        continue;
                    }
                    dbCtx.UserQueues.Add(user);
                    counter.RowsAdded++;
                }
                Trace.Info($"User Queues added:{counter.RowsAdded}, ignored:{counter.RowsIgnored}");
                if (dbCtx == null) return;
                dbCtx.SaveChanges();
                dbCtx.Dispose();
            }
            catch (Exception ex)
            {
                Trace.Error(ex);
            }
        }

        public static void SaveUserInformations(IList<UserInformation> userInfo, string connectionString)
        {
            try
            {
                var counter = new TransactionCounter();
                DatabaseContext dbCtx = null;
                foreach (var user in userInfo)
                {
                    ManageDatabaseContext(connectionString, counter, ref dbCtx);
                    if (dbCtx.UserInformations.Any(x => x.UserId.Equals(user.UserId, StringComparison.OrdinalIgnoreCase)))
                    {
                        counter.RowsIgnored++;
                        continue;
                    }
                    dbCtx.UserInformations.Add(user);
                    counter.RowsAdded++;
                }
                Trace.Info($"User Informations added:{counter.RowsAdded}, ignored:{counter.RowsIgnored}");
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
