using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using pcsd.plugin.sql.DataModel.UserReference;

namespace pcsd.plugin.sql.DataModel.Dictionary
{
    public class DictionaryManager
    {
        private static readonly ILog Trace = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void SaveQueues(Dictionary<string, string> queues, string connectionString)
        {
            using (var dbCtx = new DatabaseContext(connectionString))
            {
                try
                {
                    var counter = new TransactionCounter();
                    foreach (var q in queues)
                    {
                        if (dbCtx.Queues.Any(x => x.id == q.Key)) // does id exist
                        {
                            // update name if id does exist
                            if (q.Value == null) continue;
                            var existingItem = dbCtx.Queues.FirstOrDefault(x => x.id == q.Key);
                            if (existingItem == null || existingItem.name == q.Value) continue;
                            existingItem.name = q.Value;
                            counter.RowsUpdated++;
                        }
                        else
                        {
                            // create an item if id doesn't exist
                            dbCtx.Queues.Add(new Queue() {id = q.Key, name = q.Value});
                            counter.RowsAdded ++;
                        }
                    }
                    dbCtx.SaveChanges();
                    Trace.Info($"Queues added:{counter.RowsAdded}, updated:{counter.RowsUpdated}");
                }
                catch (Exception ex)
                {
                    Trace.Fatal(ex);
                }
            }
        }


        public static void SaveLanguages(Dictionary<string, string> languages, string connectionString)
        {
            using (var dbCtx = new DatabaseContext(connectionString))
            {
                try
                {
                    var counter = new TransactionCounter();
                    foreach (var q in languages)
                    {
                        if (dbCtx.Languages.Any(x => x.id == q.Key)) // does id exist
                        {
                            // update name if id does exist
                            if (q.Value == null) continue;
                            var existingItem = dbCtx.Languages.FirstOrDefault(x => x.id == q.Key);
                            if (existingItem == null || existingItem.name == q.Value) continue;
                            existingItem.name = q.Value;
                            counter.RowsUpdated++;
                        }
                        else
                        {
                            // create an item if id doesn't exist
                            dbCtx.Languages.Add(new Language() { id = q.Key, name = q.Value });
                            counter.RowsAdded++;
                        }
                    }
                    dbCtx.SaveChanges();
                    Trace.Info($"Languages added:{counter.RowsAdded}, updated:{counter.RowsUpdated}");
                }
                catch (Exception ex)
                {
                    Trace.Fatal(ex);
                }
            }
        }

        public static void SaveSkills(Dictionary<string, string> skills, string connectionString)
        {
            using (var dbCtx = new DatabaseContext(connectionString))
            {
                try
                {
                    var counter = new TransactionCounter();
                    foreach (var q in skills)
                    {
                        if (dbCtx.Skills.Any(x => x.id == q.Key)) // does id exist
                        {
                            // update name if id does exist
                            if (q.Value == null) continue;
                            var existingItem = dbCtx.Skills.FirstOrDefault(x => x.id == q.Key);
                            if (existingItem == null || existingItem.name == q.Value) continue;
                            existingItem.name = q.Value;
                            counter.RowsUpdated++;
                        }
                        else
                        {
                            // create an item if id doesn't exist
                            dbCtx.Skills.Add(new Skill() { id = q.Key, name = q.Value });
                            counter.RowsAdded++;
                        }
                    }
                    dbCtx.SaveChanges();
                    Trace.Info($"Skills added:{counter.RowsAdded}, updated:{counter.RowsUpdated}");
                }
                catch (Exception ex)
                {
                    Trace.Fatal(ex);
                }
            }
        }

        public static void SaveUsers(Dictionary<string, string> users, string connectionString)
        {
            using (var dbCtx = new DatabaseContext(connectionString))
            {
                try
                {
                    var counter = new TransactionCounter();
                    foreach (var q in users)
                    {
                        if (dbCtx.Users.Any(x => x.id == q.Key)) // does id exist
                        {
                            // update name if id does exist
                            if (q.Value == null) continue;
                            var existingItem = dbCtx.Users.FirstOrDefault(x => x.id == q.Key);
                            if (existingItem == null || existingItem.name == q.Value) continue;
                            existingItem.name = q.Value;
                            counter.RowsUpdated++;
                        }
                        else
                        {
                            // create an item if id doesn't exist
                            dbCtx.Users.Add(new User() { id = q.Key, name = q.Value });
                            counter.RowsAdded++;
                        }
                    }
                    dbCtx.SaveChanges();
                    Trace.Info($"Users added:{counter.RowsAdded}, updated:{counter.RowsUpdated}");
                }
                catch (Exception ex)
                {
                    Trace.Fatal(ex);
                }
            }
        }

        public static void SaveWrapUpCodes(Dictionary<string, string> wrapUpCodes, string connectionString)
        {
            using (var dbCtx = new DatabaseContext(connectionString))
            {
                try
                {
                    var counter = new TransactionCounter();
                    foreach (var q in wrapUpCodes)
                    {
                        if (dbCtx.WrapUpCodes.Any(x => x.id == q.Key)) // does id exist
                        {
                            // update name if id does exist
                            if (q.Value == null) continue;
                            var existingItem = dbCtx.WrapUpCodes.FirstOrDefault(x => x.id == q.Key);
                            if (existingItem == null || existingItem.name == q.Value) continue;
                            existingItem.name = q.Value;
                            counter.RowsUpdated++;
                        }
                        else
                        {
                            // create an item if id doesn't exist
                            dbCtx.WrapUpCodes.Add(new WrapUpCode() { id = q.Key, name = q.Value });
                            counter.RowsAdded++;
                        }
                    }
                    dbCtx.SaveChanges();
                    Trace.Info($"Wrap Up Codes added:{counter.RowsAdded}, updated:{counter.RowsUpdated}");
                }
                catch (Exception ex)
                {
                    Trace.Fatal(ex);
                }
            }
        }

        public static void SaveEdgeServers(Dictionary<string, string> edgeServers, string connectionString)
        {
            using (var dbCtx = new DatabaseContext(connectionString))
            {
                try
                {
                    var counter = new TransactionCounter();
                    foreach (var q in edgeServers)
                    {
                        if (dbCtx.EdgeServers.Any(x => x.id == q.Key)) // does id exist
                        {
                            // update name if id does exist
                            if (q.Value == null) continue;
                            var existingItem = dbCtx.EdgeServers.FirstOrDefault(x => x.id == q.Key);
                            if (existingItem == null || existingItem.name == q.Value) continue;
                            existingItem.name = q.Value;
                            counter.RowsUpdated++;
                        }
                        else
                        {
                            // create an item if id doesn't exist
                            dbCtx.EdgeServers.Add(new EdgeServer() { id = q.Key, name = q.Value });
                            counter.RowsAdded++;
                        }
                    }
                    dbCtx.SaveChanges();
                    Trace.Info($"Edge Servers added:{counter.RowsAdded}, updated:{counter.RowsUpdated}");
                }
                catch (Exception ex)
                {
                    Trace.Fatal(ex);
                }
            }
        }

        public static void SaveCampaigns(Dictionary<string, string> campaigns, string connectionString)
        {
            using (var dbCtx = new DatabaseContext(connectionString))
            {
                try
                {
                    var counter = new TransactionCounter();
                    foreach (var q in campaigns)
                    {
                        if (dbCtx.Campaigns.Any(x => x.id == q.Key)) // does id exist
                        {
                            // update name if id does exist
                            if (q.Value == null) continue;
                            var existingItem = dbCtx.Campaigns.FirstOrDefault(x => x.id == q.Key);
                            if (existingItem == null || existingItem.name == q.Value) continue;
                            existingItem.name = q.Value;
                            counter.RowsUpdated++;
                        }
                        else
                        {
                            // create an item if id doesn't exist
                            dbCtx.Campaigns.Add(new Campaign() { id = q.Key, name = q.Value });
                            counter.RowsAdded++;
                        }
                    }
                    dbCtx.SaveChanges();
                    Trace.Info($"Campaigns added:{counter.RowsAdded}, updated:{counter.RowsUpdated}");
                }
                catch (Exception ex)
                {
                    Trace.Fatal(ex);
                }
            }
        }

        public static void SaveContactLists(Dictionary<string, string> contactLists, string connectionString)
        {
            using (var dbCtx = new DatabaseContext(connectionString))
            {
                try
                {
                    var counter = new TransactionCounter();
                    foreach (var q in contactLists)
                    {
                        if (dbCtx.ContactLists.Any(x => x.id == q.Key)) // does id exist
                        {
                            // update name if id does exist
                            if (q.Value == null) continue;
                            var existingItem = dbCtx.ContactLists.FirstOrDefault(x => x.id == q.Key);
                            if (existingItem == null || existingItem.name == q.Value) continue;
                            existingItem.name = q.Value;
                            counter.RowsUpdated++;
                        }
                        else
                        {
                            // create an item if id doesn't exist
                            dbCtx.ContactLists.Add(new ContactList() { id = q.Key, name = q.Value });
                            counter.RowsAdded++;
                        }
                    }
                    dbCtx.SaveChanges();
                    Trace.Info($"Contact lists added:{counter.RowsAdded}, updated:{counter.RowsUpdated}");
                }
                catch (Exception ex)
                {
                    Trace.Fatal(ex);
                }
            }
        }

        public static void SavePresenceDefinitions(Dictionary<string, string> presence, string connectionString)
        {
            using (var dbCtx = new DatabaseContext(connectionString))
            {
                try
                {
                    var counter = new TransactionCounter();
                    foreach (var q in presence)
                    {
                        var pDef = q.Value.Split('|');

                        if (dbCtx.PresenceDefinitions.Any(x => x.id == q.Key)) // does id exist
                        {
                            // update name if id does exist
                            if (pDef[0] == null) continue;
                            var existingItem = dbCtx.PresenceDefinitions.FirstOrDefault(x => x.id == q.Key);
                            if (existingItem == null || existingItem.name == pDef[0] || existingItem.systemPresence == pDef[1]) continue;
                            existingItem.name = pDef[0];
                            existingItem.systemPresence = pDef[1];
                            counter.RowsUpdated++;
                        }
                        else
                        {
                            // create an item if id doesn't exist
                            dbCtx.PresenceDefinitions.Add(new PresenceDefinitions() { id = q.Key, name = pDef[0], systemPresence = pDef[1] });
                            counter.RowsAdded++;
                        }
                    }
                    dbCtx.SaveChanges();
                    Trace.Info($"PresenceDefinitions added:{counter.RowsAdded}, updated:{counter.RowsUpdated}");
                }
                catch (Exception ex)
                {
                    Trace.Fatal(ex);
                }
            }
        }

        public static void SaveDivisions(Dictionary<string, string> divisions, string connectionString)
        {
            using (var dbCtx = new DatabaseContext(connectionString))
            {
                try
                {
                    var counter = new TransactionCounter();
                    foreach (var q in divisions)
                    {
                        if (dbCtx.Divisions.Any(x => x.id == q.Key)) // does id exist
                        {
                            // update name if id does exist
                            if (q.Value == null) continue;
                            var existingItem = dbCtx.Divisions.FirstOrDefault(x => x.id == q.Key);
                            if (existingItem == null || existingItem.name == q.Value) continue;
                            existingItem.name = q.Value;
                            counter.RowsUpdated++;
                        }
                        else
                        {
                            // create an item if id doesn't exist
                            dbCtx.Divisions.Add(new Division() { id = q.Key, name = q.Value });
                            counter.RowsAdded++;
                        }
                    }
                    dbCtx.SaveChanges();
                    Trace.Info($"Divisions added:{counter.RowsAdded}, updated:{counter.RowsUpdated}");
                }
                catch (Exception ex)
                {
                    Trace.Fatal(ex);
                }
            }
        }


        public static void SaveRoles(Dictionary<string, string> roles, string connectionString)
        {
            using (var dbCtx = new DatabaseContext(connectionString))
            {
                try
                {
                    var counter = new TransactionCounter();
                    foreach (var q in roles)
                    {
                        if (dbCtx.Roles.Any(x => x.id == q.Key)) // does id exist
                        {
                            // update name if id does exist
                            if (q.Value == null) continue;
                            var existingItem = dbCtx.Roles.FirstOrDefault(x => x.id == q.Key);
                            if (existingItem == null || existingItem.name == q.Value) continue;
                            existingItem.name = q.Value;
                            counter.RowsUpdated++;
                        }
                        else
                        {
                            // create an item if id doesn't exist
                            dbCtx.Roles.Add(new Role() { id = q.Key, name = q.Value });
                            counter.RowsAdded++;
                        }
                    }
                    dbCtx.SaveChanges();
                    Trace.Info($"Roles added:{counter.RowsAdded}, updated:{counter.RowsUpdated}");
                }
                catch (Exception ex)
                {
                    Trace.Fatal(ex);
                }
            }
        }

        public static void SaveDataTables(Dictionary<string, string> dataTable, string connectionString)
        {
            using (var dbCtx = new DatabaseContext(connectionString))
            {
                try
                {
                    var counter = new TransactionCounter();
                    foreach (var q in dataTable)
                    {
                        if (dbCtx.DataTables.Any(x => x.id == q.Key)) // does id exist
                        {
                            // update name if id does exist
                            if (q.Value == null) continue;
                            var existingItem = dbCtx.DataTables.FirstOrDefault(x => x.id == q.Key);
                            if (existingItem == null || existingItem.name == q.Value) continue;
                            existingItem.name = q.Value;
                            counter.RowsUpdated++;
                        }
                        else
                        {
                            // create an item if id doesn't exist
                            dbCtx.DataTables.Add(new DataTable() { id = q.Key, name = q.Value });
                            counter.RowsAdded++;
                        }
                    }
                    dbCtx.SaveChanges();
                    Trace.Info($"Data Tables added:{counter.RowsAdded}, updated:{counter.RowsUpdated}");
                }
                catch (Exception ex)
                {
                    Trace.Fatal(ex);
                }
            }
        }

        public static void SaveGroups(Dictionary<string, string> groups, string connectionString)
        {
            using (var dbCtx = new DatabaseContext(connectionString))
            {
                try
                {
                    var counter = new TransactionCounter();
                    foreach (var q in groups)
                    {
                        if (dbCtx.Groups.Any(x => x.id == q.Key)) // does id exist
                        {
                            // update name if id does exist
                            if (q.Value == null) continue;
                            var existingItem = dbCtx.Groups.FirstOrDefault(x => x.id == q.Key);
                            if (existingItem == null || existingItem.name == q.Value) continue;
                            existingItem.name = q.Value;
                            counter.RowsUpdated++;
                        }
                        else
                        {
                            // create an item if id doesn't exist
                            dbCtx.Groups.Add(new Group() { id = q.Key, name = q.Value });
                            counter.RowsAdded++;
                        }
                    }
                    dbCtx.SaveChanges();
                    Trace.Info($"Groups added:{counter.RowsAdded}, updated:{counter.RowsUpdated}");
                }
                catch (Exception ex)
                {
                    Trace.Fatal(ex);
                }
            }
        }

    }
}
