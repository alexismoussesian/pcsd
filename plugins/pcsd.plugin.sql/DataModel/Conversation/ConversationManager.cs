using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Script.Serialization;
using log4net;
using System.Data.Entity;

namespace pcsd.plugin.sql.DataModel.Conversation
{
    public class ConversationManager
    {
        public IList<Conversation> conversations { get; set; }
        private static readonly ILog Trace = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static IList<Conversation> ParseConversations(string jsonText)
        {
            IList<Conversation> result = new List<Conversation>();
            try
            {
                var json = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
                var parsed = json.Deserialize<ConversationManager>(jsonText);
                //Trace.Info("AMO - ParseConversations: " + jsonText);
                //Trace.Info("AMO - parsed: " + parsed.conversations.ElementAt(1).divisionIds);
                return parsed.conversations;
            }
            catch (Exception ex)
            {
                Trace.Fatal(ex);
            }
            return result;
        }

        public static void SaveConversations(IList<Conversation> conversations, string connectionString)
        {
            try
            {
                var counter = new TransactionCounter();
                DatabaseContext dbCtx = null;
                foreach (var c in conversations)
                {
                    ManageDatabaseContext(connectionString, counter, ref dbCtx);

                    if (c.IsFinished())
                    {
                        if (!dbCtx.Conversations.Any(x => x.conversationId == c.conversationId)) // if conversation does not exist in conversations
                        {
                            // add to conversations a new finished conversation...
                            dbCtx.Conversations.Add(c);
                        }
                        else
                        {
                            // update the existing conversation when it is finished again...
                            // (it handles email replies that have the same conversation id)
                            var exisitngEntity = dbCtx.Conversations.Include(x => x.participants.Select(y => y.sessions.Select(z => z.segments))).FirstOrDefault(a => a.conversationId == c.conversationId);
                            if (exisitngEntity != null)
                            {
                                dbCtx.Entry(exisitngEntity).Entity.conversationStart = c.conversationStart;
                                dbCtx.Entry(exisitngEntity).Entity.conversationEnd = c.conversationEnd;
                                dbCtx.Entry(exisitngEntity).Entity.participants = c.participants;
                                dbCtx.Entry(exisitngEntity).Entity.divisionIds = c.divisionIds;
                                counter.RowsUpdated++;
                            }
                        }
                        // ...and eventually remove it from ongoing conversation bag
                        var oc = dbCtx.OngoingConversation.FirstOrDefault(x => x.conversationId == c.conversationId);
                        if (oc == null) continue;
                        dbCtx.OngoingConversation.Remove(oc);
                        counter.RowsRemovedFromTheBag++;
                    }
                    else if (!dbCtx.OngoingConversation.Any(x => x.conversationId == c.conversationId)) // if conversation does not exist in ongoing conversations bag
                    {
                        // add to ongoing conversations bag a new ongoing conversation
                        var oc = new OngoingConversation() {conversationId = c.conversationId};
                        dbCtx.OngoingConversation.Add(oc);
                        counter.RowsPutIntoTheBag++;
                    }
                    else
                    {
                        // ignore duplicate                            
                        Trace.Debug($"Ignoring conversation for conversationId:{c.conversationId}.");
                        counter.RowsIgnored++;
                    }
                }
                Trace.Info($"Conversations added:{counter.RowsAdded}, updated:{counter.RowsUpdated}, ignored:{counter.RowsIgnored}, added ongoing:{counter.RowsPutIntoTheBag}, removed ongoing:{counter.RowsRemovedFromTheBag}");
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
            const long batchSize = 200; // number of conversations in one inserting batch
            if (databaseContext == null)
            {
                Trace.Debug("Db context is null, creating a new one.");
                databaseContext = new DatabaseContext(connectionString);
                return;                
            }
            if (counter.RowsAdded == 0 || counter.RowsAdded%batchSize != 0) return;
            Trace.Debug($"Batch size reached {batchSize}, saving changes and recreating db context.");
            databaseContext.SaveChanges();
            databaseContext.Dispose();
            databaseContext = new DatabaseContext(connectionString);
        }

        public static List<string> GetOngoingConversationIdList(string connectionString)
        {
            var result = new List<string>();
            using (var dbCtx = new DatabaseContext(connectionString))
            {                
                try
                {
                    result.AddRange(dbCtx.OngoingConversation.Select(oc => oc.conversationId));
                    Trace.Info($"Previously reported ongoing conversations: {result.Count}");
                }
                catch (Exception ex)
                {
                    Trace.Error(ex);
                }
                return result;
            }
        }
    }
}
