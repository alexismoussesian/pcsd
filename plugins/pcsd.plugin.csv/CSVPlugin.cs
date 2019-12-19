using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using Newtonsoft.Json;
using pcsd.plugin.csv.Continuity;
using pcsd.plugin.csv.Dictionary;
using pcsd.plugin.csv.Mapping;
using pcsd.plugin.csv.Model;
using pcsd.plugin.csv.Output;
using pcsd.plugins;

namespace pcsd.plugin.csv
{
    public class CsvPlugin : IPlugin
    {
        private static readonly ILog Trace = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static Dictionary<string, string> _cmdArgs;
        
        public List<string> OngoingConversationIdList() => OngoingConversationManager.GetOngoingConversationList();

        public string[] GetCommandLineParameters() => new[] { "/target-csv" };

        public string[] GetCommandLineParametersHelp() => new[] { "/target-csv={path where the CSV file has to be created}" };

        public List<string> SupportedStats() => new List<string>() { "conversations" };

        public DateTime GetLatestInterval()
        {
            return IntervalManager.GetIntervalUtc(IntervalManager.IntervalTypes.Lastinterval);
        }

        public void SetLatestInterval(DateTime dateTime)
        {
            IntervalManager.SetIntervalUtc(IntervalManager.IntervalTypes.Lastinterval, dateTime);
        }

        public void Initialize(string[] cmdArgs)
        {
            Trace.Info("Initializing");
            _cmdArgs = ParseCmdArgs(cmdArgs);
            OngoingConversationManager.Initialize();
            MappingManager.Initialize();            
            var csvHeader = ConversationManager.GetCsvHeader();
            FileManager.SaveToFile(new List<string>() {csvHeader}, _cmdArgs["target-csv"]);
        }

        public void InitializeDictionaries(Dictionary<string, string> queues, Dictionary<string, string> languages, Dictionary<string, string> skills, Dictionary<string, string> users, Dictionary<string, string> wrapUpCodes, Dictionary<string, string> edgeServers, Dictionary<string, string> campaigns, Dictionary<string, string> contactLists, Dictionary<string, string> presences, Dictionary<string, string> divisions, Dictionary<string, string> dataTables, Dictionary<string, string> groups, Dictionary<string, string> roles)
        {
            Trace.Info($"InitializeDictionaries(), queues:{queues?.Count}, languages:{languages?.Count}, skills:{skills?.Count}, users:{users?.Count}, wrap up codes: {wrapUpCodes?.Count}, edge servers: {edgeServers?.Count}, campaigns: {campaigns?.Count}, contactLists: {contactLists?.Count}, presenceDefinitions: {presences?.Count}, dataTables: {dataTables?.Count}, groups: {groups?.Count}, roles: {roles?.Count}");
            DictionaryManager.Queues = queues;
            DictionaryManager.Languages = languages;
            DictionaryManager.Skills = skills;
            DictionaryManager.Users = users;
            DictionaryManager.WrapUpCodes = wrapUpCodes;
            DictionaryManager.EdgeServers = edgeServers;
            DictionaryManager.Campaigns = campaigns;
            DictionaryManager.ContactLists = contactLists;
        }

        public bool PushData(string data)
        {
            Trace.Info($"PushData(), data length: {data.Length}");
            if (string.IsNullOrWhiteSpace(data))
            {
                Trace.Error("PushData() was executed with empty content, exiting...");
                return false;
            }
            if (!data.StartsWith("{\"conversations\":"))
            {
                Trace.Error("Incorrect data format, exiting...");
                return false;
            }

            Trace.Info("Deserializing data");
            ConversationManager.Instance = JsonConvert.DeserializeObject<ConversationManager>(data);
            
            Trace.Info("Processing data");            
            var csvLines = ConversationManager.GetCsvConversations();

            Trace.Info("Saving data");
            FileManager.SaveToFile(csvLines, _cmdArgs["target-csv"]);
            
            return true;
        }
        public bool PushGroupMembers(string groupMember)
        {
            return true;
        }

        public bool PushDataTableRows(string dataTableRowsComplete)
        {
            return true;
        }

        public bool PushUserRoles(string userRolesComplete)
        {
            return true;
        }
        public bool PushUserSkills(string userSkillComplete)
        {
            return true;
        }

        public bool PushUserQueues(string userQueueComplete)
        {
            return true;
        }

        public bool PushUserInformations(string userInfoComplete)
        {
            return true;
        }

        private Dictionary<string, string> ParseCmdArgs(IEnumerable<string> cmdArgs) { return cmdArgs.ToDictionary(cmdArg => cmdArg.Substring(1, cmdArg.IndexOf("=", StringComparison.Ordinal) - 1), cmdArg => cmdArg.Substring(cmdArg.IndexOf("=", StringComparison.Ordinal) + 1, cmdArg.Length - cmdArg.IndexOf("=", StringComparison.Ordinal) - 1)); }

        #region "not used or not supported"

        public DateTime GetLatestIntervalForAggregates()
        {
            Trace.Debug("The plugin doesn't support aggregated data.");
            return DateTime.Now;
        }

        public void SetLatestIntervalForAggregates(DateTime dateTime)
        {
            Trace.Debug("The plugin doesn't support aggregated data.");
        }

        public void Dispose()
        {
            // Nothing to do here...
        }

        public void ResetInterval(DateTime dateTime)
        {
            // What is the purpose of this method? Is it necessery at all?
        }

        public bool AttachParticipantAttrs => false;

        #endregion
    }
}
