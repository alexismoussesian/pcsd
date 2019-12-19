using System.Data.Entity;
using pcsd.plugin.sql.DataModel.Conversation;
using pcsd.plugin.sql.DataModel.ConversationAggregates;
using pcsd.plugin.sql.DataModel.Dictionary;
using pcsd.plugin.sql.DataModel.UserAggregates;
using pcsd.plugin.sql.DataModel.UserDetails;
using pcsd.plugin.sql.DataModel.GroupMember;
using pcsd.plugin.sql.DataModel.UserReference;

namespace pcsd.plugin.sql.DataModel
{
    public class DatabaseContext: DbContext
    {
        public DatabaseContext()
        {
            // empty constructor for db migrations
        }

        public DatabaseContext(string connectionString) { Database.Connection.ConnectionString = connectionString;}

        public DbSet<Interval.Interval> Intervals { get; set; }

        public DbSet<Conversation.Conversation> Conversations { get; set; }

        public DbSet<OngoingConversation> OngoingConversation { get; set; }

        public DbSet<PrimaryPresence> PrimaryPresence { get; set; }

        public DbSet<RoutingStatus> RoutingStatus { get; set; }

        public DbSet<Queue> Queues { get; set; }

        public DbSet<Division> Divisions { get; set; }

        public DbSet<Language> Languages { get; set; }

        public DbSet<Skill> Skills { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<WrapUpCode> WrapUpCodes { get; set; }

        public DbSet<EdgeServer> EdgeServers { get; set; }

        public DbSet<Campaign> Campaigns { get; set; }

        public DbSet<ContactList> ContactLists { get; set; }

        public DbSet<PresenceDefinitions> PresenceDefinitions { get; set; }

        public DbSet<ParticipantAttr.ParticipantAttr> ParticipantAttrs { get; set; }

        public DbSet<ConversationAggregate> ConversationAggregates { get; set; }

        public DbSet<UserAggregate> UserAggregates { get; set; }

        public DbSet<DataTable> DataTables { get; set; }

        public DbSet<DataTableRow.DataTableRows> DataTableRows { get; set; }

        public DbSet<Dictionary.Group> Groups { get; set; }

        public DbSet<GroupMember.GroupMember> GroupMembers { get; set; }

        public DbSet<ConversationMetric.ConversationMetric> ConversationMetrics { get; set; }

        public DbSet<UserRole> userRoles { get; set; }

        public DbSet<UserSkill> UserSkills { get; set; }

        public DbSet<UserQueue> UserQueues { get; set; }

        public DbSet<UserInformation> UserInformations { get; set; }

        public DbSet<Role> Roles { get; set; }


    }
}
