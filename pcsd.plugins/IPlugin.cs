using System;
using System.Collections.Generic;


namespace pcsd.plugins
{
    public interface IPlugin
    {
        /// <summary>
        /// Called when the plugin is first loaded
        /// </summary>
        void Initialize(string[] cmdArgs);

        /// <summary>
        /// Gets the command line parameters used by this plugin
        /// </summary>
        /// <returns>A list of command line parameters that this plugin is used for</returns>
        string[] GetCommandLineParameters();

        /// <summary>
        /// Gets the command line parameters help used by this plugin
        /// </summary>
        /// <returns>A list of command line parameters help that this plugin is used for</returns>
        string[] GetCommandLineParametersHelp();

        /// <summary>
        /// Reset the interval to a timespan maxSpan days in the past.
        /// </summary>
        void ResetInterval(DateTime dateTimeNow);

        /// <summary>
        /// Get the interval when the plugin was last executed
        /// </summary>
        /// <returns>A DateTime with the interval that was used during the last execution of ths plugin</returns>
        DateTime GetLatestInterval();

        /// <summary>
        /// Get the interval for aggregates when the plugin was last executed
        /// </summary>
        /// <returns>A DateTime with the aggregate interval that was used during the last execution of ths plugin</returns>
        DateTime GetLatestIntervalForAggregates();

        /// <summary>
        /// Set the interval when the plugin was last executed
        /// </summary>
        void SetLatestInterval(DateTime dateTime);

        /// <summary>
        /// Set the interval for aggregates when the plugin was last executed
        /// </summary>
        void SetLatestIntervalForAggregates(DateTime dateTime);

        /// <summary>
        /// Called when dictionaries should be initialized (i.e. Conversations, Queues, Users)
        /// </summary>
        void InitializeDictionaries(Dictionary<string, string> queues, Dictionary<string, string> anguages, Dictionary<string, string> skills, Dictionary<string, string> users, Dictionary<string, string> wrapUpCodes, Dictionary<string, string> edgeServers, Dictionary<string, string> campaigns, Dictionary<string, string> contactLists, Dictionary<string, string> systemPresence, Dictionary<string, string> divisions, Dictionary<string, string> dataTables, Dictionary<string, string> groups, Dictionary<string, string> roles);

        /// <summary>
        /// Called when data needs to be pushed to the 3rd party system (target)
        /// </summary>
        /// <param name="data">data (json-formatted) to push</param>
        /// <returns>True if successful, false otherwise</returns>
        bool PushData(string data);

        /// <summary>
        /// Called when group members needs to be pushed to the 3rd party system (target)
        /// </summary>
        /// <param name="ListOfMembers"></param>
        /// <returns></returns>
        bool PushGroupMembers(string ListOfMembers);

        /// <summary>
        /// Called when user roles needs to be pushed to the 3rd party system (target)
        /// </summary>
        /// <param name="ListOfUserRoles"></param>
        /// <returns></returns>
        bool PushUserRoles(string ListOfUserRoles);

        /// <summary>
        /// Called when user skills needs to be pushed to the 3rd party system (target)
        /// </summary>
        /// <param name="ListOfUserSkills"></param>
        /// <returns></returns>
        bool PushUserSkills(string ListOfUserSkills);

        /// <summary>
        /// Called when user queues needs to be pushed to the 3rd party system (target)
        /// </summary>
        /// <param name="ListOfUserQueues"></param>
        /// <returns></returns>
        bool PushUserQueues(string ListOfUserQueues);

        /// <summary>
        /// Called when user informations needs to be pushed to the 3rd party system (target)
        /// </summary>
        /// <param name="ListOfUserInformations"></param>
        /// <returns></returns>
        bool PushUserInformations(string ListOfUserInformations);

        /// <summary>
        /// Called when datatable rows needs to be pushed to the 3rd party system (target)
        /// </summary>
        /// <param name="dataTableRowsComplete"></param>
        /// <returns></returns>
        bool PushDataTableRows(string dataTableRowsComplete);

        /// <summary>
        /// Called when the main applications exits
        /// </summary>
        void Dispose();

        /// <summary>
        /// This method returns a list of identifiers of reported previously ongoing conversations. They have to be retrived from the PC API and pushed into the target if already finished.
        /// </summary>
        /// <returns>List of conversation identifiers.</returns>
        List<string> OngoingConversationIdList();

        /// <summary>
        /// Returns the list of stats that are supported by the plugin.
        /// </summary>
        /// <returns>The list of stats</returns>
        List<string> SupportedStats();

        bool AttachParticipantAttrs { get; }
    }

}
