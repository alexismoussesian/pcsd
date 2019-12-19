using log4net;
using log4net.Config;
using pcsd.plugins;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using pcsd.Model;

namespace pcsd
{
    public class Program
    {
        /// <summary>
        /// Plugins folder
        /// </summary>
        private const string PluginsFolder = "Plugins";
        private const int StatisticsInterval = 30;
        private const int MaxIntervalDurationDays = 7;

        /// <summary>
        /// Contains the list of plugins that were successfully loaded from the Plugins folder
        /// </summary>
        private static ICollection<IPlugin> _loadedPlugins;
        private static List<string[]> _pluginsCmdArgsHelp;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly PureCloud PureCloudObj = new PureCloud();

        /// <summary>
        /// List of stats to retrieve (i.e. Conversations, Queues, Users)
        /// </summary>
        private static List<string> _stats = new List<string>();

        private static readonly DateTime To = DateTime.UtcNow;
        private static DateTime ToForAggregates => new DateTime(To.Year, To.Month, To.Day, To.Hour, To.Minute >= 30 ? 30 : 0, 0);
        private static readonly string[] DateTimeFormats = { "yyyy-MM-dd", "yyyy-MM-dd HH:mm:ss" }; // - formats from documentation: /startdate=2015-12-31 or /startdate="2015-12-31 14:00:00"

        /// <summary>
        /// Main entry
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            AppTitle.StartProgress();
            PrintWelcomeScreen();

            // initiate log
            BasicConfigurator.Configure();

            try
            {
                Log.Debug("Application is started");

                // Load plugins
                _loadedPlugins = new PluginLoader().LoadPlugins(PluginsFolder, args, out _pluginsCmdArgsHelp);

                // check parameters
                if (args.Length == 0 || args[0].Equals("/help"))
                {
                    ShowUsage();
                    ShowPressAnyKey();
                    return;
                }

                // Retrieve arguments from command line
                Log.Debug($"Number of command line parameters = {args.Length}");
                for (var i = 0; i < args.Length; i++)
                {
                    Log.Debug($"Arg[{i}] = [{args[i]}]");
                    var splittedArg = args[i].Replace("/", "").Split('=');
                    var key = splittedArg[0];
                    var value = "";
                    if (splittedArg.Length == 2)
                    {
                        value = splittedArg[1];
                    }
                    switch (key.ToLower())
                    {
                        case "clientid":
                            PureCloudObj.ClientId = value;
                            break;
                        case "clientsecret":
                            PureCloudObj.ClientSecret = value;
                            break;
                        case "environment":
                            PureCloudObj.Environment = value;
                            break;
                        case "stats":
                            _stats = value.ToLower().Split(',').ToList();
                            break;                        
                        case "startdate":                            
                            DateTime parsedValue;
                            if (DateTime.TryParseExact(value, DateTimeFormats, CultureInfo.CurrentCulture, DateTimeStyles.None, out parsedValue ))
                            {
                                PureCloudObj.StartDate = parsedValue;
                                Log.Info($"Start date parameter: {PureCloudObj.StartDate?.ToString("o")}");
                            }
                            else
                            {
                                Log.Error($"Couldn't parse startdate. Delivered value:{value}, expected formats:{string.Join(", ", DateTimeFormats)}");
                            }                            
                            break;                        
                    }
                }

                // Set stats to default if it wasn't specified
                if (_stats.Count == 0)
                {
                    _stats = new List<string>()
                    {
                        "conversations",
                        "queues",
                        "users",
                        "userdetails"
                    };
                }

                // Login
                PureCloudObj.Login();
                
                // Load dictionaries & pull all analytics
                if (PureCloudObj.LoadAllDictionaries())
                {
                    Log.Info("All Dictionaries loaded.");
                    BeginProcess();
                }

                // And... we're done!
                PureCloudObj.Logout();
            }
            catch (ArgumentException ex)
            {
                Log.Fatal($"Application error {ex.Message}");
            }
            catch (NotImplementedException ex)
            {
                Log.Fatal($"Application error {ex.Message}");
            }
            catch (Exception ex)
            {
                Log.Fatal("Application error", ex);
                ShowUsage();
            }
            finally
            {
                if (_loadedPlugins != null)
                {
                    // Dispose all plugins

                    foreach (var loadedPlugin in _loadedPlugins)
                    {
                        loadedPlugin.Dispose();
                    }
                }
            }

            AppTitle.StopProgress();
            ShowPressAnyKey();
        }

        static void BeginProcess()
        {                        
            foreach (var loadedPlugin in _loadedPlugins)
            {
                try
                {
                    Log.Info($">>>>> PROCESS STARTED for plugin {loadedPlugin.GetType()}");
                    
                    // <calling InitializeDictionaries() for plugin>
                    loadedPlugin.InitializeDictionaries(PureCloudObj.ListOfQueues, PureCloudObj.ListOfLanguages, PureCloudObj.ListOfSkills, PureCloudObj.ListOfUsers, PureCloudObj.ListOfWrapUpCodes, PureCloudObj.ListOfEdgeServers, PureCloudObj.ListOfCampaigns, PureCloudObj.ListOfContactLists, PureCloudObj.ListOfPresences, PureCloudObj.ListOfDivisions, PureCloudObj.ListOfDataTables, PureCloudObj.ListOfGroups, PureCloudObj.ListOfRoles);
                    // </calling InitializeDictionaries() for plugin>

                    // <Add group members>
                    string result = "";
                    foreach (var member in PureCloudObj.ListOfGroupMembers)
                    {   result = result + member.groupId + "|" + member.id + "|" + member.name + "$";   }
                    result = result.Substring(0, result.Length - 1);
                    loadedPlugin.PushGroupMembers(result);
                    // </Add group members>


                    // <Add datatable rows>
                    result = "";
                    foreach (var row in PureCloudObj.ListOfDataTableRows)
                    {   result = result + row.dataTableId + "@" + row.dataTableRows + "£";  }
                    result = result.Substring(0, result.Length - 1);
                    loadedPlugin.PushDataTableRows(result);
                    // </Add datatable rows>


                    // <Add user roles>
                    result = "";
                    foreach (var row in PureCloudObj.ListOfUserRoles)
                    {   result = result + row.UserId + "|" + row.Roles + "|" + row.Division + "$";  }
                    result = result.Substring(0, result.Length - 1);
                    loadedPlugin.PushUserRoles(result);
                    // </Add user roles>


                    // <Add user skills>
                    result = "";
                    foreach (var row in PureCloudObj.ListOfUserSkills)
                    {   result = result + row.UserId + "|" + row.Skill + "|" + row.Level + "$"; }
                    result = result.Substring(0, result.Length - 1);
                    loadedPlugin.PushUserSkills(result);
                    // </Add user skills>


                    // <Add user queues>
                    result = "";
                    foreach (var row in PureCloudObj.ListOfUserQueues)
                    { result = result + row.UserId + "|" + row.Queue + "$"; }
                    result = result.Substring(0, result.Length - 1);
                    loadedPlugin.PushUserQueues(result);
                    // </Add user queues>


                    // <Add user details>
                    result = "";
                    foreach (var row in PureCloudObj.ListOfUserInfos)
                    { result = result + row.UserId + "|" + row.Email + "|" + row.Department + "|" + row.Title + "|" + row.Locations + "$"; }
                    result = result.Substring(0, result.Length - 1);
                    loadedPlugin.PushUserInformations(result);
                    // </Add user queues>


                    // <determining 'from' values>
                    // https://bitbucket.org/eccemea/purecloud-stats-dispatcher/issues/11/startdate-should-override-last-interval
                    var from = PureCloudObj.StartDate ?? loadedPlugin.GetLatestInterval();
                    var fromForAggregates = PureCloudObj.StartDateForAggregates ?? loadedPlugin.GetLatestIntervalForAggregates();
                    // </determining 'from' values>

                    // <getting the data from PC> 
                    if (_stats.Contains("conversations") && loadedPlugin.SupportedStats().Contains("conversations")) BeginProcessingConversationDetails(from, loadedPlugin);
                    if (_stats.Contains("userdetails") && loadedPlugin.SupportedStats().Contains("userdetails")) BeginProcessingUserDetails(from, loadedPlugin);
                    if (_stats.Contains("queues") && loadedPlugin.SupportedStats().Contains("queues")) BeginProcessingConversationAggregates(fromForAggregates, loadedPlugin);
                    if (_stats.Contains("users") && loadedPlugin.SupportedStats().Contains("users")) BeginProcessingUserAggregates(fromForAggregates, loadedPlugin);                    
                    // </getting the data from PC>

                    // <saving intervals for plugin>
                    loadedPlugin.SetLatestInterval(To);
                    loadedPlugin.SetLatestIntervalForAggregates(ToForAggregates);
                    // </saving intervals for plugin>

                    Log.Info($"<<<<< PROCESS FINISHED for plugin {loadedPlugin.GetType()}");
                }
                catch (Exception ex)
                {
                    Log.Fatal($"{loadedPlugin.GetType()} error", ex);
                }
            }
        }

        private static void BeginProcessingConversationDetails(DateTime from, IPlugin loadedPlugin)
        {            
            // Get conversation data based on requested period:
            PullConversationData(from, loadedPlugin);
            // Get conversation data based on saved ongoing conversation list:
            PullConversationDataByIds(loadedPlugin);                                          
        }

        private static void PullConversationData(DateTime from, IPlugin loadedPlugin)
        {
            Log.Debug($"PullConversationData(), {nameof(from)}:{from.ToString("O")}, {nameof(To)}:{To.ToString("O")}");
            try
            {         
                if (from.Equals(DateTime.MinValue))  { throw new ArgumentException("Equals(DateTime.MinValue)", nameof(from)); } // a legacy condition, not sure is it necessary at all
                
                // <calculating intervals>
                var intervalList = new List<Interval>();
                var tempFrom = from;
                while (To.Subtract(tempFrom).TotalDays >= MaxIntervalDurationDays)
                {
                    var tempTo = tempFrom.AddDays(MaxIntervalDurationDays).Date.AddTicks(-1);
                    intervalList.Add(new Interval(tempFrom, tempTo));
                    tempFrom = tempTo.AddDays(1).Date;
                }
               intervalList.Add(new Interval(tempFrom, To));
                // </calculating intervals>
                
                // <getting the data>
                foreach (var interval in intervalList)
                {
                    var conversationData = PureCloudObj.GetConversationData(interval);
                    if (conversationData == null || conversationData.Count < 1) continue;
                    var conversationDataJson = "{\"conversations\":" + JsonConvert.SerializeObject(conversationData) + "}";
                    loadedPlugin.PushData(conversationDataJson);
                    Log.Info($"{nameof(loadedPlugin.AttachParticipantAttrs)}:{loadedPlugin.AttachParticipantAttrs}");
                    if (!loadedPlugin.AttachParticipantAttrs) continue;
                    var finishedConversations = conversationData.Where(conversation => conversation.ConversationEnd != null).Select(conversation => conversation.ConversationId).ToList();
                    var participantAttrs = PureCloudObj.GetParticipantAttrs(finishedConversations);
                    if (participantAttrs == null || participantAttrs.Count < 1) continue;
                    var participantAttrsJson = "{\"participantattrs\":" + JsonConvert.SerializeObject(participantAttrs) + "}";
                    loadedPlugin.PushData(participantAttrsJson);

                    var conversationMetrics = PureCloudObj.GetConversationMetrics(finishedConversations);
                    if (conversationMetrics == null || conversationMetrics.Count < 1) continue;
                    var conversationMetricsJson = "{\"conversationmetrics\":" + JsonConvert.SerializeObject(conversationMetrics) + "}";
                    loadedPlugin.PushData(conversationMetricsJson);


                }
                // </getting the data>
            }
            catch (Exception ex)
            {
                Log.Error("PullConversationData()", ex);
            }
        }

        private static void PullConversationDataByIds(IPlugin loadedPlugin)
        {
            Log.Debug("PullConversationDataByIds()");
            try
            {
                var ongoingConversationIdList = loadedPlugin.OngoingConversationIdList();
                if (ongoingConversationIdList == null || ongoingConversationIdList.Count < 1) return;
                var conversationData = PureCloudObj.GetConversationData(ongoingConversationIdList);
                if (conversationData == null || conversationData.Count < 1) return;
                var conversationDataJson = "{\"conversations\":" + JsonConvert.SerializeObject(conversationData) + "}";
                loadedPlugin.PushData(conversationDataJson);
                Log.Info($"{nameof(loadedPlugin.AttachParticipantAttrs)}:{loadedPlugin.AttachParticipantAttrs}");
                if (!loadedPlugin.AttachParticipantAttrs) return;
                var finishedConversations = conversationData.Where(conversation => conversation.ConversationEnd != null).Select(conversation => conversation.ConversationId).ToList();
                var participantAttrs = PureCloudObj.GetParticipantAttrs(finishedConversations);
                if (participantAttrs == null || participantAttrs.Count < 1) return;
                var participantAttrsJson = "{\"participantattrs\":" + JsonConvert.SerializeObject(participantAttrs) + "}";
                loadedPlugin.PushData(participantAttrsJson);

                var conversationMetrics = PureCloudObj.GetConversationMetrics(finishedConversations);
                if (conversationMetrics == null || conversationMetrics.Count < 1) return;
                var conversationMetricsJson = "{\"conversationmetrics\":" + JsonConvert.SerializeObject(conversationMetrics) + "}";
                loadedPlugin.PushData(conversationMetricsJson);

            }
            catch (Exception ex)
            {
                Log.Error("PullConversationDataByIds()", ex);
            }
        }

        private static void BeginProcessingUserDetails(DateTime from, IPlugin loadedPlugin)
        {
            Log.Debug($"BeginProcessingUserDetails(), {nameof(from)}:{@from:O}, {nameof(To)}:{To:O}");
            try
            {
                if (from.Equals(DateTime.MinValue)) { throw new ArgumentException("Equals(DateTime.MinValue)", nameof(from)); } // a legacy condition, not sure is it necessary at all

                // <calculating intervals>
                var intervalList = new List<Interval>();
                var tempFrom = from;
                while (To.Subtract(tempFrom).TotalDays >= MaxIntervalDurationDays)
                {
                    var tempTo = tempFrom.AddDays(MaxIntervalDurationDays).Date.AddTicks(-1);
                    intervalList.Add(new Interval(tempFrom, tempTo));
                    tempFrom = tempTo.AddDays(1).Date;
                }
                intervalList.Add(new Interval(tempFrom, To));
                // </calculating intervals>

                // <getting the data>
                foreach (var interval in intervalList)
                {
                    var userDetailsData = PureCloudObj.GetUserDetails(interval);
                    if (userDetailsData == null || userDetailsData.Count < 1) continue;
                    var conversationDataJson = "{\"userdetailsdata\":" + JsonConvert.SerializeObject(userDetailsData) + "}";
                    loadedPlugin.PushData(conversationDataJson);                    
                }
                // </getting the data>
            }
            catch (Exception ex)
            {
                Log.Error("BeginProcessingUserDetails()", ex);
            }
        }

        private static void BeginProcessingConversationAggregates(DateTime fromForAggregates, IPlugin loadedPlugin)
        {
            Log.Debug($"BeginProcessingConversationAggregates(), {nameof(fromForAggregates)}:{fromForAggregates.ToString("O")}, {nameof(To)}:{To.ToString("O")}");
            try
            {
                if (fromForAggregates.Equals(DateTime.MinValue)) { throw new ArgumentException("Equals(DateTime.MinValue)", nameof(fromForAggregates)); } // a legacy condition, not sure is it necessary at all

                // <calculating intervals>
                if (ToForAggregates.Subtract(fromForAggregates).TotalMinutes < 30 ) return; // minimum time frame is 30 minutes (a duration of one grnuality interval)
                var intervalList = new List<Interval>();                
                var tempFrom = fromForAggregates;
                while (ToForAggregates.Subtract(tempFrom).TotalDays > MaxIntervalDurationDays)
                {
                    var tempTo = tempFrom.AddDays(MaxIntervalDurationDays).Date;
                    intervalList.Add(new Interval(tempFrom, tempTo));
                    tempFrom = tempTo.Date;
                }
                intervalList.Add(new Interval(tempFrom, ToForAggregates));
                // </calculating intervals>

                // <getting the data>
                foreach (var interval in intervalList)
                {
                    var conversationAggregates = PureCloudObj.GetConversationAggregates(interval);
                    if (conversationAggregates?.Results == null || conversationAggregates.Results.Count < 1) continue;                    
                    var conversationAggregatesJson = "{\"queuedata\":" + JsonConvert.SerializeObject(conversationAggregates.Results) + "}";
                    loadedPlugin.PushData(conversationAggregatesJson);
                }
                // </getting the data>
            }
            catch (Exception ex)
            {
                Log.Error("BeginProcessingConversationAggregates()", ex);
            }
        }

        private static void BeginProcessingUserAggregates(DateTime fromForAggregates, IPlugin loadedPlugin)
        {
            Log.Debug($"BeginProcessingUserAggregates(), {nameof(fromForAggregates)}:{fromForAggregates.ToString("O")}, {nameof(To)}:{To.ToString("O")}");
            try
            {
                if (fromForAggregates.Equals(DateTime.MinValue)) { throw new ArgumentException("Equals(DateTime.MinValue)", nameof(fromForAggregates)); } // a legacy condition, not sure is it necessary at all

                // <calculating intervals>
                if (ToForAggregates.Subtract(fromForAggregates).TotalMinutes < 30) return; // minimum time frame is 30 minutes (a duration of one grnuality interval)
                var intervalList = new List<Interval>();
                var tempFrom = fromForAggregates;

                // <issue API-2719 - orginal code - 7 days query intervals>
                //while (ToForAggregates.Subtract(tempFrom).TotalDays >= MaxIntervalDuration)
                //{
                //    var tempTo = tempFrom.AddDays(MaxIntervalDuration).Date;
                //    intervalList.Add(new Interval(tempFrom, tempTo));
                //    tempFrom = tempTo.Date;
                //}
                //intervalList.Add(new Interval(tempFrom, ToForAggregates));
                // </issue API-2719 - orginal code - 7 days query intervals>

                // <issue API-2719 - workaround - 12 hrs query intervals>
                const int maxIntervalDurationHours = 12;
                while (ToForAggregates.Subtract(tempFrom).TotalHours > maxIntervalDurationHours)
                {
                    var tempTo = tempFrom.AddHours(maxIntervalDurationHours);
                    intervalList.Add(new Interval(tempFrom, tempTo));
                    tempFrom = tempTo;
                }
                intervalList.Add(new Interval(tempFrom, ToForAggregates));
                // </issue API-2719 - workaround - 12 hrs query intervals>

                // </calculating intervals>

                // <getting the data>
                foreach (var interval in intervalList)
                {
                    var page = 0;
                    var userAggregates = PureCloudObj.GetUserAggregates(interval, page);
                    while (userAggregates != null && userAggregates.Results.Any())
                    {
                        var userAggregatesJson = "{\"userdata\":" + JsonConvert.SerializeObject(userAggregates.Results) + "}";
                        loadedPlugin.PushData(userAggregatesJson);
                        page++;
                        userAggregates = PureCloudObj.GetUserAggregates(interval, page);
                    }                    
                }
                // </getting the data>
            }
            catch (Exception ex)
            {
                Log.Error("BeginProcessingUserAggregates()", ex);
            }
        }

        /// <summary>
        /// Shows how to use this program
        /// </summary>
        static void ShowUsage()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Usage of pcsd.exe");
            sb.AppendLine("\t/clientid=XXXX");
            sb.AppendLine("\t/clientsecret=XXXX");
            sb.AppendLine("\t/environment=XXXX");            
            sb.AppendLine($"\t/startdate=(allowed formats: {string.Join(" or ", DateTimeFormats)})");
            if (_pluginsCmdArgsHelp != null)
            {
                foreach (var cmd in _pluginsCmdArgsHelp.SelectMany(pluginCmdArgs => pluginCmdArgs))
                {
                    sb.AppendLine($"\t{cmd}");
                }
            }
            sb.AppendLine("\t/help");
            Log.Info(sb.ToString());
        }

        private static void ShowPressAnyKey()
        {
            if (!Environment.UserInteractive) return;
            // wait for a key pressing if the app is running in the user mode
            Console.BackgroundColor = ConsoleColor.DarkYellow;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\r\n      Press any key to continue...     ");
            Console.ResetColor();
            Console.ReadKey();
        }


        private static void PrintWelcomeScreen()

        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("");
            Console.WriteLine(" #########################################");
            Console.WriteLine(" #                                       #");
            Console.WriteLine(" #      ╔═╗┬ ┬┬─┐┌─┐╔═╗┬  ┌─┐┬ ┬┌┬┐      #");
            Console.WriteLine(" #      ╠═╝│ │├┬┘├┤ ║  │  │ ││ │ ││      #");
            Console.WriteLine(" #      ╩  └─┘┴└─└─┘╚═╝┴─┘└─┘└─┘─┴┘      #");
            Console.WriteLine(" #            ╔═╗┌┬┐┌─┐┌┬┐┌─┐            #");
            Console.WriteLine(" #            ╚═╗ │ ├─┤ │ └─┐            #");
            Console.WriteLine(" #            ╚═╝ ┴ ┴ ┴ ┴ └─┘            #");
            Console.WriteLine(" #                                       #");
            Console.WriteLine(" #########################################");
            Console.WriteLine("");
            Console.WriteLine($"AssemblyVersion: {Assembly.GetEntryAssembly().GetName().Version}");
            Console.WriteLine($"OSVersion: {Environment.OSVersion}");
            Console.WriteLine($"MachineName: {Environment.MachineName}");
            Console.WriteLine($"UserName: {Environment.UserDomainName}\\{Environment.UserName}");
            Console.WriteLine("");
            Console.ResetColor();
        }

    }
}
