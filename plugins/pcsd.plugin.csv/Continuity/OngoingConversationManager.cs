using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using log4net;

namespace pcsd.plugin.csv.Continuity
{
    class OngoingConversationManager
    {
        private const string OngoingConversationListSettingName = "OngoingConversationList";
        private static readonly ILog Trace = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static List<string> _ongoingConversationList; 

        public static void Initialize()
        {
            try
            {
                Trace.Debug("Loading ongoing conversation list");
                var config = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);
                if (!config.AppSettings.Settings.AllKeys.Contains(OngoingConversationListSettingName))
                {
                    Trace.Debug($"Settings key {OngoingConversationListSettingName} doesn't exist, adding it");                    
                    config.AppSettings.Settings.Add(OngoingConversationListSettingName, ""); 
                    config.AppSettings.Settings[OngoingConversationListSettingName].Value = "";
                    config.Save(ConfigurationSaveMode.Modified);
                    _ongoingConversationList = new List<string>();
                    Trace.Info("Ongoing coversation list created");
                    return;
                }
                var ongoingConversationListAsString = config.AppSettings.Settings[OngoingConversationListSettingName].Value;
                _ongoingConversationList = ongoingConversationListAsString.Contains("|") ? ongoingConversationListAsString.Split('|').ToList() : new List<string>();
                Trace.Info($"Ongoing coversation list loaded: {_ongoingConversationList.Count()} items");
            }
            catch (Exception ex)
            {
                Trace.Fatal(ex);                
            }
        }

        private static void Save()
        {
            try
            {
                Trace.Debug("Saving ongoing conversation list");
                var config = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);
                if (!config.AppSettings.Settings.AllKeys.Contains(OngoingConversationListSettingName))
                {
                    Trace.Debug($"Settings key {OngoingConversationListSettingName} doesn't exist, adding it");
                    config.AppSettings.Settings.Add(OngoingConversationListSettingName, string.Empty);                                        
                    Trace.Info("Ongoing coversation list created");                    
                }
                var ongoingConversationListAsString = string.Join("|", _ongoingConversationList);
                config.AppSettings.Settings[OngoingConversationListSettingName].Value = ongoingConversationListAsString;
                config.Save(ConfigurationSaveMode.Modified);
                Trace.Debug($"Ongoing coversation list saved: {_ongoingConversationList.Count()} items");
            }
            catch (Exception ex)
            {
                Trace.Fatal(ex);
            }
        }

        public static void AddOngoingConversationIfDoesntExist(string conversationId)
        {
            if (_ongoingConversationList.Contains(conversationId)) return;
            _ongoingConversationList.Add(conversationId);
            Save();
            Trace.Debug($"Ongoing conversation has been added: {conversationId}");
        }

        public static void RemoveOngoingConversationIfDoesExist(string conversationId)
        {
            if (!_ongoingConversationList.Contains(conversationId)) return;
            _ongoingConversationList.Remove(conversationId);
            Save();
            Trace.Debug($"Ongoing conversation has been removved: {conversationId}");
        }

        public static List<string> GetOngoingConversationList() => _ongoingConversationList ?? new List<string>();
    }
}
