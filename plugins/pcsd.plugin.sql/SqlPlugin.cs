using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Script.Serialization;
using log4net;
using pcsd.plugin.sql.DataModel.Conversation;
using pcsd.plugin.sql.DataModel.ConversationAggregates;
using pcsd.plugin.sql.DataModel.Dictionary;
using pcsd.plugin.sql.DataModel.Interval;
using pcsd.plugin.sql.DataModel.ParticipantAttr;
using pcsd.plugin.sql.DataModel.UserAggregates;
using pcsd.plugin.sql.DataModel.UserDetails;
using pcsd.plugin.sql.DataModel.GroupMember;
using pcsd.plugin.sql.DataModel.ConversationMetric;
using pcsd.plugin.sql.DataModel.DataTableRow;
using pcsd.plugins;
using pcsd.plugin.sql.DataModel.UserReference;

namespace pcsd.plugin.sql
{
    public class SqlPlugin : IPlugin
    {
        private const string ConnectionStringArgName = "target-sql";
        private const string AttachParticipantAttrsArgName = "participant-attrs";
        private string ConnectionString => _cmdArgs[ConnectionStringArgName];

        public bool AttachParticipantAttrs
        {
            get
            {
                var result = false;
                if (_cmdArgs.ContainsKey(AttachParticipantAttrsArgName))
                {
                    bool.TryParse(_cmdArgs[AttachParticipantAttrsArgName], out result);
                }                
                return result;
            }
        }
        private Dictionary<string, string> _cmdArgs;
        private static readonly ILog Trace = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void Initialize(string[] cmdArgs)
        {
            Trace.Info("Initializing");
            _cmdArgs = ParseCmdArgs(cmdArgs);
        }

        public void FixEfProviderServicesProblem()
        {
            var instance = System.Data.Entity.SqlServer.SqlProviderServices.Instance;
        }

        public string[] GetCommandLineParameters() => new[] {$"/{ConnectionStringArgName}", $"/{AttachParticipantAttrsArgName}" };

        public string[] GetCommandLineParametersHelp() => new[] { $"/{ConnectionStringArgName}={{SQL Server connection string}}", $"/{AttachParticipantAttrsArgName}={{A type of boolean, optional parameter. It controls whether participant attributes have to be exported.}}" };

        public List<string> SupportedStats() => new List<string>() { "conversations", "queues", "users", "userdetails" };

        public DateTime GetLatestInterval()
        {
            return IntervalManager.GetIntervalUtc(Interval.IntervalTypes.Lastinterval, ConnectionString);
        }

        public DateTime GetLatestIntervalForAggregates()
        {
            return IntervalManager.GetIntervalUtc(Interval.IntervalTypes.LastIntervalForAggregates, ConnectionString);
        }

        public void SetLatestInterval(DateTime dateTime)
        {
            IntervalManager.SetIntervalUtc(Interval.IntervalTypes.Lastinterval, dateTime, ConnectionString);
        }

        public void SetLatestIntervalForAggregates(DateTime dateTime)
        {
            IntervalManager.SetIntervalUtc(Interval.IntervalTypes.LastIntervalForAggregates, dateTime, ConnectionString);
        }

        public void ResetInterval(DateTime dateTime)
        {
            // what to do here?
        }

        public void InitializeDictionaries(Dictionary<string, string> queues, Dictionary<string, string> languages, Dictionary<string, string> skills, Dictionary<string, string> users, Dictionary<string, string> wrapUpCodes, Dictionary<string, string> edgeServers, Dictionary<string, string> campaigns, Dictionary<string, string> contactLists, Dictionary<string, string> presences, Dictionary<string, string> divisions, Dictionary<string, string> dataTables, Dictionary<string, string> groups, Dictionary<string, string> roles)
        {
            Trace.Info($"InitializeDictionaries(), queues:{queues?.Count}, languages:{languages?.Count}, skills:{skills?.Count}, users:{users?.Count}, wrap up codes: {wrapUpCodes?.Count}, edge servers: {edgeServers?.Count}, campaigns: {campaigns?.Count}, contactLists: {contactLists?.Count}, presenceDefinitions: {presences?.Count}, divisions:{divisions?.Count}, dataTables:{dataTables?.Count}, groups:{groups?.Count}, roles:{roles?.Count}");
            Trace.Info($"Initialize ConnectionString:{ConnectionString}");
            DictionaryManager.SaveQueues(queues, ConnectionString);
            DictionaryManager.SaveLanguages(languages, ConnectionString);
            DictionaryManager.SaveSkills(skills, ConnectionString);
            DictionaryManager.SaveUsers(users, ConnectionString);
            DictionaryManager.SaveWrapUpCodes(wrapUpCodes, ConnectionString);
            DictionaryManager.SaveEdgeServers(edgeServers, ConnectionString);
            DictionaryManager.SaveCampaigns(campaigns, ConnectionString);
            DictionaryManager.SaveContactLists(contactLists, ConnectionString);
            DictionaryManager.SavePresenceDefinitions(presences, ConnectionString);
            DictionaryManager.SaveDivisions(divisions, ConnectionString);
            DictionaryManager.SaveDataTables(dataTables, ConnectionString);
            DictionaryManager.SaveGroups(groups, ConnectionString);
            DictionaryManager.SaveRoles(roles, ConnectionString);
        }

        public bool PushData(string data)
        {
            // data = System.IO.File.ReadAllText("C:\\temp\\TemProjectFormPcsd\\conv.txt"); // <- loading JSON from file for tests
            Trace.Info($"PushData(), data length: {data.Length}");
            if (string.IsNullOrEmpty(data))
            {
                Trace.Error("PushData() was executed with empty content, exiting... :-(");
                return false;
            }
            // http://stackoverflow.com/questions/1151987/can-i-set-an-unlimited-length-for-maxjsonlength-in-web-config
            var serializer = new JavaScriptSerializer {MaxJsonLength = int.MaxValue};

            var dataType = ((serializer).Deserialize<Dictionary<string, object>>(data)).Keys.FirstOrDefault();
            Trace.Info($"Detected data type: {dataType}");
            switch (dataType)
            {
                case "conversations":                    
                    ConversationManager.SaveConversations(ConversationManager.ParseConversations(data), ConnectionString);
                    break;
                case "participantattrs":
                    ParticipantAttrManager.SaveAttrs(ParticipantAttrManager.ParseAttrs(data), ConnectionString);
                    break;
                case "conversationmetrics":
                    ConversationMetricManager.SaveMetrics(ConversationMetricManager.ParseMetrics(data), ConnectionString);
                    break;
                case "userdetailsdata":
                    UserDetailsManager.SaveUserDetails(UserDetailsManager.ParseUserDetails(data), ConnectionString);
                    break;
                case "userdata":
                    UserAggregatesManager.SaveUserAggregates(UserAggregatesManager.ParseUserAggregates(data), ConnectionString);
                    break;
                case "queuedata":
                    ConversationAggregatesManager.SaveConversationAggregates(ConversationAggregatesManager.ParseConversationAggregates(data), ConnectionString);
                    break;
                default:
                    Trace.Error("Unsupported data type. :-(");
                    break;
            }
            return true;
        }


        public bool PushGroupMembers(string groupMember)
        {
            List<GroupMember> ListOfGroupMember = new List<GroupMember>();

            var groupMembers = groupMember.Split('$');

            foreach (var members in groupMembers)
            {
                var items = members.Split('|');
                ListOfGroupMember.Add(new GroupMember { groupId = items[0], id = items[1], name = items[2] });
            }

            GroupMemberManager.SaveGroupMembers(ListOfGroupMember, ConnectionString);

            return true;
        }

        public bool PushUserRoles(string userRole)
        {
            List<UserRole> ListOfUserRole = new List<UserRole>();

            var userRoles = userRole.Split('$');

            foreach (var user in userRoles)
            {
                var items = user.Split('|');
                ListOfUserRole.Add(new UserRole { UserId = items[0], Roles = items[1], Division = items[2] });
            }

            UserReferenceManager.SaveUserRoles(ListOfUserRole, ConnectionString);

            return true;
        }

        public bool PushUserSkills(string userSkill)
        {
            List<UserSkill> ListOfUserSkill = new List<UserSkill>();

            var userSkills = userSkill.Split('$');

            foreach (var user in userSkills)
            {
                var items = user.Split('|');
                ListOfUserSkill.Add(new UserSkill { UserId = items[0], Skill = items[1], Level = items[2] });
            }

            UserReferenceManager.SaveUserSkills(ListOfUserSkill, ConnectionString);

            return true;
        }


        public bool PushUserQueues(string userQueue)
        {
            List<UserQueue> ListOfUserQueue = new List<UserQueue>();

            var userQueues = userQueue.Split('$');

            foreach (var user in userQueues)
            {
                var items = user.Split('|');
                ListOfUserQueue.Add(new UserQueue { UserId = items[0], Queue = items[1] });
            }

            UserReferenceManager.SaveUserQueues(ListOfUserQueue, ConnectionString);

            return true;
        }

        public bool PushUserInformations(string userInfo)
        {
            List<UserInformation> ListOfUserInformation = new List<UserInformation>();

            var userInformations = userInfo.Split('$');

            foreach (var user in userInformations)
            {
                var items = user.Split('|');
                ListOfUserInformation.Add(new UserInformation { UserId = items[0], Email = items[1], Department = items[2], Title = items[3], Locations = items[4] });
            }

            UserReferenceManager.SaveUserInformations(ListOfUserInformation, ConnectionString);

            return true;
        }


        public bool PushDataTableRows(string dataTableRowsComplete)
        {
            List<DataTableRows> ListOfDTRows = new List<DataTableRows>();

            var dataTableRow = dataTableRowsComplete.Split('£');

            foreach (var row in dataTableRow)
            {
                var items = row.Split('@');
                ListOfDTRows.Add(new DataTableRows { dataTableId = items[0],  dataTableRows = items[1]});
            }

            DataTableRowManager.SaveDataTablesRows(ListOfDTRows, ConnectionString);

            return true;
        }

        public void Dispose()
        {
            Trace.Info("Dispose()");
            // todo: not sure do I have to do anything here
        }

        public List<string> OngoingConversationIdList() { return ConversationManager.GetOngoingConversationIdList(ConnectionString); }

        private Dictionary<string, string> ParseCmdArgs(IEnumerable<string> cmdArgs) { return cmdArgs.ToDictionary( cmdArg => cmdArg.Substring(1, cmdArg.IndexOf("=", StringComparison.Ordinal) - 1), cmdArg => cmdArg.Substring(cmdArg.IndexOf("=", StringComparison.Ordinal) + 1, cmdArg.Length - cmdArg.IndexOf("=", StringComparison.Ordinal) - 1)); }

    }
}
