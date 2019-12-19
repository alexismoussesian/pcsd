using log4net;
using pcsd.plugins;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;

namespace pcsd.plugin.file
{
    public class FilePlugin : IPlugin
    {
        private readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private string _filename = "output.txt";
        private const int MaxTimeSpan = 7;
        private DateTime _lastExecution;
        private DateTime _lastAggregateExecution;

        public void Dispose()
        {
            // Nothing to do here...
        }

        public List<string> OngoingConversationIdList()
        {
            throw new NotImplementedException();
        }

        public string[] GetCommandLineParameters()
        {
            return new[]
            {
                "/target-file"
            };
        }

        public string[] GetCommandLineParametersHelp()
        {
            return new[]
            {
                "/target-file=XXXX"
            };
        }

        public List<string> SupportedStats() => new List<string>() { "conversations", "queues", "users" };

        public void ResetInterval(DateTime dateTime)
        {
            try
            {
                var temp = GetAppSetting("LastExecution");
                if (!string.IsNullOrEmpty(temp))
                {
                    _lastExecution = DateTime.Parse(temp);
                    _log.Debug($"Last Exectution loaded: {_lastExecution}");
                }
                else
                {
                    _lastExecution = dateTime.Subtract(TimeSpan.FromDays(MaxTimeSpan));
                    SetLatestInterval(_lastExecution);
                    _log.Debug($"Last Exectution set: {_lastExecution}");
                }

                temp = GetAppSetting("LastAggregateExecution");
                if (!string.IsNullOrEmpty(temp))
                {
                    _lastAggregateExecution = DateTime.Parse(temp);
                    _log.Debug($"Last Aggregate Exectution loaded: {_lastAggregateExecution}");
                }
                else
                {
                    _lastAggregateExecution = new DateTime(_lastExecution.Year, _lastExecution.Month, _lastExecution.Day, _lastExecution.Hour, _lastExecution.Minute > 30 ? 30 : 0, 0);
                    SetLatestIntervalForAggregates(_lastAggregateExecution);
                    _log.Debug($"Last Aggregate Exectution set: {_lastAggregateExecution}");
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Error in SetLatestInterval {ex.Message}");
            }
        }

        public DateTime GetLatestInterval()
        {
            return _lastExecution;
        }

        public DateTime GetLatestIntervalForAggregates()
        {
            return _lastAggregateExecution;
        }

        public void SetLatestInterval(DateTime dateTime)
        {
            _lastExecution = dateTime;
            SetAppSetting("LastExecution", dateTime.ToString("o"));
        }

        public void SetLatestIntervalForAggregates(DateTime dateTime)
        {
            try
            {
                _lastAggregateExecution = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute >= 30 ? 30 : 0, 0);
                SetAppSetting("LastAggregateExecution", dateTime.ToString("o"));
            }
            catch (Exception ex)
            {
                _log.Error($"Error in SetLatestInterval {ex.Message}");
            }
        }

        public void Initialize(string[] cmdArg)
        {
            if (cmdArg.Length <= 0) return;
            var temp = cmdArg[0].Split('=');
            if (temp.Length >= 2)
            {
                _filename = temp[1];
            }
        }

        public void InitializeDictionaries(Dictionary<string, string> queues, Dictionary<string, string> languages, Dictionary<string, string> skills, Dictionary<string, string> users, Dictionary<string, string> wrapUpCodes, Dictionary<string, string> edgeServers, Dictionary<string, string> campaigns, Dictionary<string, string> contactLists, Dictionary<string, string> systemPresence, Dictionary<string, string> divisions, Dictionary<string, string> dataTables, Dictionary<string, string> groups, Dictionary<string, string> roles)
        {
            // There are no dictionaries to initialize. We will just write whatever we get to the file.
        }

        public bool PushData(string data)
        {
            _log.Debug($"File Plugin received data {data.Length} - writing to file {_filename}");
            using (var outputFile = new StreamWriter(_filename, true))
            {
                outputFile.WriteLine(data);
            }
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

        private static string GetAppSetting(string key)
        {
            var config = ConfigurationManager.OpenExeConfiguration(
                                    Assembly.GetExecutingAssembly().Location);
            return config.AppSettings.Settings[key] != null ? config.AppSettings.Settings[key].Value : "";
        }

        private static void SetAppSetting(string key, string value)
        {
            var config = ConfigurationManager.OpenExeConfiguration(
                                    Assembly.GetExecutingAssembly().Location);
            if (config.AppSettings.Settings[key] != null)
            {
                config.AppSettings.Settings.Remove(key);
            }
            config.AppSettings.Settings.Add(key, value);
            config.Save(ConfigurationSaveMode.Modified);
        }

        public bool AttachParticipantAttrs => false;
    }
}
