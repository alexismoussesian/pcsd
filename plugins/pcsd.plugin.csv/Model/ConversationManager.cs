using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using pcsd.plugin.csv.Continuity;
using pcsd.plugin.csv.Dictionary;
using pcsd.plugin.csv.Mapping;

namespace pcsd.plugin.csv.Model
{
    class ConversationManager
    {
        private static readonly ILog Trace = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public IList<Conversation> conversations { get; set; }

        public static ConversationManager Instance { get; set; }

        public static string GetCsvHeader()
        {            
            var csvLineList = MappingManager.MappingItemList.Select(item => ($"\"{item.Name}\"")).ToList();
            var csvLineString = string.Join(",", csvLineList);
            return csvLineString;
        }

        public static List<string> GetCsvConversations()
        {
            var result = new List<string>();
            try
            {
                foreach (var conversation in Instance.conversations)
                {
                    // <conversation>

                    if (!conversation.IsFinished())
                    {
                        Trace.Debug($"Skipping ongoing conversation: {conversation.conversationId}");                                                
                        OngoingConversationManager.AddOngoingConversationIfDoesntExist(conversation.conversationId);
                        continue;
                    }

                    var convItems = MappingManager.GetMappingItemSublist("conversations");
                    var convValues = GetValues(convItems, conversation);
                    if (conversation.participants != null && conversation.participants.Any())
                    {
                        foreach (var participant in conversation.participants)
                        {
                            // <participant>
                            var partItems = MappingManager.GetMappingItemSublist("participants");
                            var partValues = GetValues(partItems, participant);
                            if (participant.sessions != null && participant.sessions.Any())
                            {
                                foreach (var session in participant.sessions)
                                {
                                    // <session>
                                    var sessItems = MappingManager.GetMappingItemSublist("sessions");
                                    var sessValues = GetValues(sessItems, session);
                                    if (session.segments != null && session.segments.Any())
                                    {
                                        foreach (var segment in session.segments)
                                        {
                                            // <segment>
                                            var segItems = MappingManager.GetMappingItemSublist("segments");
                                            var segValues = GetValues(segItems, segment);
                                            var csvLine = ConvertValuesToCsvLine(new List<Dictionary<string, string>>() { convValues, partValues, sessValues, segValues });
                                            AddCsvLine(conversation.conversationId, csvLine, ref result);                                            
                                            // </segment>
                                        }
                                    }
                                    else
                                    {
                                        var csvLine = ConvertValuesToCsvLine(new List<Dictionary<string, string>>() { convValues, partValues, sessValues });
                                        AddCsvLine(conversation.conversationId, csvLine, ref result);
                                    }
                                    // </session>
                                }
                            }
                            else
                            {
                                var csvLine = ConvertValuesToCsvLine(new List<Dictionary<string, string>>() { convValues, partValues });
                                AddCsvLine(conversation.conversationId, csvLine, ref result);
                            }
                            // </participant>
                        }
                    }
                    else
                    {
                        var csvLine = ConvertValuesToCsvLine(new List<Dictionary<string, string>>() { convValues });
                        AddCsvLine(conversation.conversationId, csvLine, ref result);
                    }
                    // </conversation>
                }
            }
            catch (Exception ex)
            {
                Trace.Error("Error during transformation JSON to CSV", ex);
            }            
            return result;
        }

        private static Dictionary<string, string> GetValues(List<Item> mappingItemSublist, Reflectionable reflectionable)
        {
            var result = new Dictionary<string, string>();
            foreach (var item in mappingItemSublist)
            {
                string propertyValue;
                if (item.isArray)
                {
                    // <multi-value> (e.g. requestedRoutingSkillIds)
                    var propertyValueList = reflectionable.GetPropertyMultiValue(item.Name);
                    if (!string.IsNullOrWhiteSpace(item.dictionary))
                    {
                        for (var i = 0; i < propertyValueList.Count; i++)
                        {
                            propertyValueList[i] = GetValueFromDictionary(propertyValueList[i], item.dictionary);
                        }                                                
                    }
                    propertyValue = string.Join("|", propertyValueList);
                    // </multi-value>
                }
                else
                {
                    // <single-value>
                    propertyValue = reflectionable.GetPropertyValue(item.Name, item.dateTimeMask);
                    if (!string.IsNullOrWhiteSpace(item.dictionary))
                    {
                        propertyValue = GetValueFromDictionary(propertyValue, item.dictionary);
                    }
                    // </single-value>
                }
                
                if (item.clean) propertyValue = Clean(propertyValue); //(e.g. wrapUpNote)
                if (item.maxLen != null && item.maxLen > 0 && propertyValue.Length > item.maxLen)propertyValue = propertyValue.Substring(0, item.maxLen.Value); //(e.g. wrapUpNote)

                result.Add(item.Name, propertyValue);
            }        
            return result;
        }

        private static string Clean(string input)
        {
            var undesirableChars = new[] {'\t','\r','\n','"'};
            return undesirableChars.Aggregate(input, (current, undesirableChar) => current.Replace(undesirableChar, ' '));
        }

        private static string GetValueFromDictionary(string key, string dictionary)
        {
            var result = string.Empty;

            switch (dictionary.ToLower())
            {
                case "queues":
                    result = DictionaryManager.Queues.ContainsKey(key) ? DictionaryManager.Queues[key] : key;
                    break;
                case "languages":
                    result = DictionaryManager.Languages.ContainsKey(key) ? DictionaryManager.Languages[key] : key;
                    break;
                case "skills":
                    result = DictionaryManager.Skills.ContainsKey(key) ? DictionaryManager.Skills[key] : key;
                    break;
                case "users":
                    result = DictionaryManager.Users.ContainsKey(key) ? DictionaryManager.Users[key] : key;
                    break;
                case "wrapupcodes":
                    result = DictionaryManager.WrapUpCodes.ContainsKey(key) ? DictionaryManager.WrapUpCodes[key] : key;
                    break;
                case "edgeservers":
                    result = DictionaryManager.EdgeServers.ContainsKey(key) ? DictionaryManager.EdgeServers[key] : key;
                    break;
                case "campaigns":
                    result = DictionaryManager.Campaigns.ContainsKey(key) ? DictionaryManager.Campaigns[key] : key;
                    break;
                case "contactlists":
                    result = DictionaryManager.ContactLists.ContainsKey(key) ? DictionaryManager.ContactLists[key] : key;
                    break;
                default:
                    Trace.Error($"Unknown dictionary: {dictionary}");
                    result = key;
                    break;
            }

            return result;
        }

        private static string ConvertValuesToCsvLine(List<Dictionary<string, string>> valuesList)
        {                        
            var allValues = new Dictionary<string,string>();
            foreach (var values in valuesList) { values.ToList().ForEach(x => allValues.Add(x.Key, x.Value)); }
            var csvLineList = new List<string>();
            foreach (var item in MappingManager.MappingItemList)
            {
                if (allValues.ContainsKey(item.Name))
                {
                    // value found for configuration item
                    csvLineList.Add($"\"{allValues[item.Name]}\"");                    
                }
                else
                {
                    // value not found for configuration item
                    csvLineList.Add("\"\"");                    
                }
            }
            var csvLineString = string.Join(",", csvLineList);            
            return csvLineString;
        }

        private static void AddCsvLine(string conversationId, string csvLine, ref List<string> csvLineList)
        {
            OngoingConversationManager.RemoveOngoingConversationIfDoesExist(conversationId);
            csvLineList.Add(csvLine);
        }
    }
}
