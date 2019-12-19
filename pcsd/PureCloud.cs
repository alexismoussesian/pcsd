using PureCloudPlatform.Client.V2.Api;
using PureCloudPlatform.Client.V2.Client;
using PureCloudPlatform.Client.V2.Extensions;
using PureCloudPlatform.Client.V2.Model;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using pcsd.Model;
using pcstat.Model;


namespace pcsd
{
    public delegate void DictionariesLoadedEvent(object sender, EventArgs e);

    public class PureCloud
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const int PageSize = 100;
        private const string Granularity = "PT30M";     // "PT15M"

        #region Login/Logout

        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Environment { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? StartDateForAggregates => StartDate == null ? (DateTime?) null : new DateTime(StartDate.Value.Year, StartDate.Value.Month, StartDate.Value.Day, StartDate.Value.Hour, StartDate.Value.Minute >= 30 ? 30 : 0, 0);
      
        /// <summary>
        /// Login into PureCloud
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
        /// <param name="environment"></param>
        /// <returns></returns>
        public void Login(string clientId, string clientSecret, string environment)
        {
            Log.Debug("Setting Parameter");
            ClientId = clientId;
            Log.Debug($"ClientId = {clientId}");
            ClientSecret = clientSecret;
            Log.Debug($"ClientSecret = {clientSecret}");
            Environment = environment;
            Log.Debug($"Environment = {environment}");
            Login();
        }

        /// <summary>
        /// Login into PureCloud, Parameters need to be set first
        /// </summary>
        /// <returns></returns>
        public void Login()
        {
            if (string.IsNullOrEmpty(ClientId))
            {
                throw new ArgumentException("Argument ClientId is missing");
            }
            if (string.IsNullOrEmpty(ClientSecret))
            {
                throw new ArgumentException("Argument ClientSecret is missing");
            }
            if (string.IsNullOrEmpty(Environment))
            {
                throw new ArgumentException("Argument Environment is missing");
            }
            Log.Debug($"Login starting with Parameters ClientId = {ClientId}, ClientSecret = {ClientSecret}, Environment = {Environment}");
            Log.Debug("Setting Environment");
            Configuration.Default.ApiClient.RestClient.BaseUrl = new Uri($"https://api.{Environment}");

            Log.Debug("Retrieving Access Token");
            var accessTokenInfo = Configuration.Default.ApiClient.PostToken(ClientId, ClientSecret);

            Log.Debug($"Access Token retrieved: {accessTokenInfo.AccessToken}");
            Configuration.Default.AccessToken = accessTokenInfo.AccessToken;

            if (!string.IsNullOrEmpty(accessTokenInfo.AccessToken))
            {
                Log.Info("Login successful");

                // Trace org info
                var org = GetOrgData();                
                Log.Info($"Environment: {Environment}");
                Log.Info($"Org id: {org.Id}");
                Log.Info($"Org name: {org.Name}");
            }
        }

        /// <summary>
        /// Logout from PureCloud
        /// </summary>
        /// <returns></returns>
        public void Logout()
        {
            var api = new TokensApi();
            Log.Info("Logging out");
            api.DeleteTokensMe();
        }

        #endregion

        #region "Org"

        private Organization GetOrgData()
        {
            try
            {
                var api = new OrganizationApi();
                var org = api.GetOrganizationsMe();
                return org;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex);
            }
            return null;
        }

        #endregion

        #region Loading Dictionaries User, Queues, Languages, Skills, Wrap Up Codes, etc

        public Dictionary<string, string> ListOfQueues { get; } = new Dictionary<string, string>();
        public Dictionary<string, string> ListOfLanguages { get; } = new Dictionary<string, string>();
        public Dictionary<string, string> ListOfSkills { get; } = new Dictionary<string, string>();
        public Dictionary<string, string> ListOfUsers { get; } = new Dictionary<string, string>();
        public Dictionary<string, string> ListOfWrapUpCodes { get; } = new Dictionary<string, string>();
        public Dictionary<string, string> ListOfEdgeServers { get; } = new Dictionary<string, string>();
        public Dictionary<string, string> ListOfCampaigns { get; } = new Dictionary<string, string>();
        public Dictionary<string, string> ListOfContactLists { get; } = new Dictionary<string, string>();
        public Dictionary<string, string> ListOfPresences { get; } = new Dictionary<string, string>();
        public Dictionary<string, string> ListOfDivisions { get; } = new Dictionary<string, string>();
        public Dictionary<string, string> ListOfDataTables { get; } = new Dictionary<string, string>();
        public Dictionary<string, string> ListOfGroups { get; } = new Dictionary<string, string>();
        public Dictionary<string, string> ListOfRoles { get; } = new Dictionary<string, string>();
        public List<UserRole> ListOfUserRoles { get; } = new List<UserRole>();
        public List<UserSkill> ListOfUserSkills { get; } = new List<UserSkill>();
        public List<pcstat.Model.UserQueue> ListOfUserQueues { get; } = new List<pcstat.Model.UserQueue>();
        public List<UserInformation> ListOfUserInfos { get; } = new List<UserInformation>();
        public List<GroupMember> ListOfGroupMembers { get; } = new List<GroupMember>();
        public List<DataTableRows> ListOfDataTableRows { get; } = new List<DataTableRows>();

        private bool _userLoaded;        
        private bool _queuesLoaded;
        private bool _presencesLoaded;
        private bool _languagesLoaded;
        private bool _skillsLoaded;
        private bool _wrapUpCodesLoaded;
        private bool _edgeServersLoaded;
        private bool _campaignsLoaded;
        private bool _contactListsLoaded;
        private bool _divisionsLoaded;
        private bool _dataTablesLoaded;
        private bool _groupsLoaded;
        private bool _rolesLoaded;


        /// <summary>
        /// Used to initiate the GetUser Method, a dictionary with IDs and Names is returned
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetUsers()
        {
            Log.Debug("GetUsers started");
            var temp = new Dictionary<string, string>();

            temp = GetUsers(1, temp);

            foreach (var user in temp)
            {
                ListOfUsers[user.Key] = user.Value;
            }

            GetUserDetails();

            //GetUserRoles();

            //GetUserSkills();

            //GetUserQueues();

            _userLoaded = true;

            return ListOfUsers;
        }



        /// <summary>
        /// Used internaly to call GetUser Method until all users are delivered back, a dictionary with IDs and Names is returned
        /// </summary>
        /// <param name="page"></param>
        /// <param name="temp"></param>
        /// <returns></returns>
        private static Dictionary<string, string> GetUsers(int page, Dictionary<string, string> temp)
        {
            var api = new UsersApi();
            //todo added timeout
            api.Configuration.Timeout = 300000;
            var result = api.GetUsers(PageSize, page);

            if (result.Entities == null)
            {
                Log.Info("Empty User result");
                return temp;
            }

            Log.Debug($"Number of User results: {result.Entities.Count}");

            foreach (var user in result.Entities)
            {
                temp[user.Id] = user.Name;
                //Log.Debug($"Add User: {user.Name}");
            }
            if (temp.Count < result.Total)
            {
                Log.Debug($"More Users to retrieve {temp.Count} < {result.Total} ");
                return GetUsers(page + 1, temp);
            }
            Log.Info($"All Users retrieved {temp.Count} / {result.Total} ");

            return temp;
        }

        /// <summary>
        /// Get all queues
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetQueues()
        {
            Log.Debug("GetQueues started");
            var temp = new Dictionary<string, string>();
            temp = GetQueues(1, temp);
            foreach (var queue in temp)
            {
                ListOfQueues[queue.Key] = queue.Value;
            }

            _queuesLoaded = true;

            return ListOfQueues;
        }

        /// <summary>
        /// Get queues, page by page
        /// </summary>
        /// <param name="page"></param>
        /// <param name="temp"></param>
        /// <returns></returns>
        private static Dictionary<string, string> GetQueues(int page, Dictionary<string, string> temp)
        {
            var api = new RoutingApi();
            var result = api.GetRoutingQueues(PageSize, page);

            if (result.Entities == null)
            {
                Log.Info("Empty Queue result");
                return temp;
            }

            Log.Debug($"Number of Queue results: {result.Entities.Count}");

            foreach (var queue in result.Entities)
            {
                temp[queue.Id] = queue.Name;
                //Log.Debug($"Add Queue: {queue.Name}");
            }
            if (temp.Count < result.Total)
            {
                Log.Debug($"More Queues to retrieve {temp.Count} < {result.Total} ");
                return GetQueues(page + 1, temp);
            }
            Log.Info($"All Queues retrieved {temp.Count} / {result.Total} ");

            return temp;
        }


        /// <summary>
        /// Get the list of presence
        /// </summary>
        private void GetPresenceDefinitions()
        {
            Log.Debug("GetPresenceDefinitions started");
            ListOfPresences.Clear();
            var currentPage = 1;
            const string defaultLangLabel = "en_US";
            var api = new PresenceApi() { Configuration = { AccessToken = Configuration.Default.AccessToken } }; ;
            var pageResult = api.GetPresencedefinitions(currentPage, PageSize);
            while (pageResult.Total > ListOfPresences.Count)
            {
                foreach (var element in pageResult.Entities)
                {
                    var id = element.Id;
                    var labels = "label not found|label not found";
                    if (element.LanguageLabels != null && element.LanguageLabels.Any())
                    {
                        labels = element.LanguageLabels.ContainsKey(defaultLangLabel) ? $"{element.LanguageLabels[defaultLangLabel]}|{element.SystemPresence}" : $"{element.LanguageLabels.First().Value}|{element.SystemPresence}";
                    }                    
                    ListOfPresences.Add(id, labels);
                }
                pageResult = api.GetPresencedefinitions(++currentPage, PageSize);
            }
            _presencesLoaded = true;
            Log.Info($"All Presence Definitions retrieved {ListOfPresences.Count}");
        }

        /// <summary>
        /// Get all languages
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetLanguages()
        {
            Log.Debug("GetLanguages started");
            var temp = new Dictionary<string, string>();
            temp = GetLanguages(1, temp);
            foreach (var language in temp)
            {
                ListOfLanguages[language.Key] = language.Value;
            }
            _languagesLoaded = true;

            return ListOfLanguages;
        }

        /// <summary>
        /// Get languages, page by page
        /// </summary>
        /// <param name="page"></param>
        /// <param name="temp"></param>
        /// <returns></returns>
        private static Dictionary<string, string> GetLanguages(int page, Dictionary<string, string> temp)
        {
            var api = new LanguagesApi();
            var result = api.GetLanguages(PageSize, page);

            if (result.Entities == null)
            {
                Log.Info("Empty Language result");
                return temp;
            }

            Log.Debug($"Number of Language results: {result.Entities.Count}");

            foreach (var language in result.Entities)
            {
                temp[language.Id] = language.Name;
                //Log.Debug($"Add Language: {language.Name}");
            }
            if (temp.Count < result.Total)
            {
                Log.Debug($"More Languages to retrieve {temp.Count} < {result.Total} ");
                return GetQueues(page + 1, temp);
            }
            Log.Info($"All Languages retrieved {temp.Count} / {result.Total} ");

            return temp;
        }

        /// <summary>
        /// Get all Skills
        /// </summary>
        /// <returns>A dictionary of skills ids and names</returns>
        public Dictionary<string, string> GetSkills()
        {
            Log.Debug("GetSkills started");

            var pageNumber = 1;
            SkillEntityListing result;

            do
            {
                result = GetSkills(pageNumber);
                foreach (var skill in result.Entities)
                {
                    ListOfSkills.Add(skill.Id, skill.Name);
                    //Log.Debug($"Add skill: {skill.Name}");
                }

                pageNumber += 1;
            } while (result.Entities.Count == PageSize);
            Log.Info($"All Skills retrieved {ListOfSkills.Count} / {ListOfSkills.Count} ");

            _skillsLoaded = true;
            return ListOfSkills;
        }

        /// <summary>
        /// Retrieve list of Skills for the specific page number
        /// <param name="pageNumber"></param>
        /// </summary>
        private static SkillEntityListing GetSkills(int pageNumber)
        {
            var api = new RoutingApi();

            var result = api.GetRoutingSkills(PageSize, pageNumber);

            Log.Debug($"execute GetSkills for page {pageNumber} and found {result.Entities.Count} records.");

            return result;
        }

        /// <summary>
        /// Get all wrap up codes
        /// </summary>
        /// <returns>A dictionary of wrap up codes ids and names</returns>
        public Dictionary<string, string> GetWrapUpCodes()
        {
            Log.Debug("GetWrapUpCodes started");

            var pageNumber = 1;
            WrapupCodeEntityListing result;

            do
            {
                result = GetWrapUpCodes(pageNumber);
                foreach (var wrapUpCode in result.Entities)
                {
                    ListOfWrapUpCodes.Add(wrapUpCode.Id, wrapUpCode.Name);
                    //Log.Debug($"Add wrap up code: {wrapUpCode.Name}");
                }

                pageNumber += 1;
            } while (result.Entities.Count == PageSize);
            Log.Info($"All Wrap Up Codes retrieved {ListOfWrapUpCodes.Count} / {ListOfWrapUpCodes.Count} ");

            _wrapUpCodesLoaded = true;            
            return ListOfWrapUpCodes;
        }
        
        /// <summary>
        /// Retrieve list of Wrap Up Codes for the specific page number
        /// <param name="pageNumber"></param>
        /// </summary>
        private static WrapupCodeEntityListing GetWrapUpCodes(int pageNumber)
        {
            var api = new RoutingApi();

            var result = api.GetRoutingWrapupcodes(PageSize, pageNumber);            

            Log.Debug($"execute GetWrapUpCodes for page {pageNumber} and found {result.Entities.Count} records.");

            return result;
        }

        /// <summary>
        /// The method gets list of all Edge servers
        /// </summary>        
        private void GetEdgeDictionary()
        {
            Log.Debug("GetEdgeDictionary started");
            ListOfEdgeServers.Clear();
            var currentPage = 1;
      
            var api = new TelephonyProvidersEdgeApi() { Configuration = { AccessToken = Configuration.Default.AccessToken } }; ;
            var pageResult = api.GetTelephonyProvidersEdges(PageSize, currentPage);            
            while (pageResult != null && pageResult.Entities.Any())
            {
                foreach (var element in pageResult.Entities) { ListOfEdgeServers.Add(element.Id, element.Name); }                
                pageResult = api.GetTelephonyProvidersEdges(PageSize, ++currentPage);
            }
            _edgeServersLoaded = true;
            Log.Info($"All Edge Servers retrieved {ListOfEdgeServers.Count}");
        }

        /// <summary>
        /// The method gets list of all campaigns
        /// </summary>        
        private void GetCampaigns()
        {
            Log.Debug("GetCampaigns started");
            ListOfCampaigns.Clear();
            var currentPage = 1;

            var api = new OutboundApi() { Configuration = { AccessToken = Configuration.Default.AccessToken } }; ;

            // <proper code>
            //
            //var pageResult = api.GetCampaigns(pageSize, currentPage);
            //while (pageResult != null && pageResult.Entities.Any())
            //{
            //    foreach (var element in pageResult.Entities) { ListOfCampaigns.Add(element.Id, element.Name); }
            //    pageResult = api.GetCampaigns(pageSize, ++currentPage);
            //}
            //
            // </proper code>

            // <workaround for the paging bug in API>
            // details: https://inindca.atlassian.net/browse/API-2488

            var pageResult = api.GetOutboundCampaigns(PageSize, currentPage);            
            while (pageResult.Total > ListOfCampaigns.Count)
            {
                foreach (var element in pageResult.Entities) { ListOfCampaigns.Add(element.Id, element.Name); }
                pageResult = api.GetOutboundCampaigns(PageSize, ++currentPage);
            }

            // </workaround for the paging bug in API>

            _campaignsLoaded = true;
            Log.Info($"All Campaigns retrieved {ListOfCampaigns.Count}");
        }

        /// <summary>
        /// Get contact lists
        /// </summary>
        private void GetContactLists()
        {
            Log.Debug("GetContactLists started");
            ListOfContactLists.Clear();
            var currentPage = 1;
            var api = new OutboundApi() { Configuration = { AccessToken = Configuration.Default.AccessToken } }; ;

            var pageResult = api.GetOutboundContactlists(false, false, PageSize, currentPage);
            while (pageResult.Total > ListOfContactLists.Count)
            {
                foreach (var element in pageResult.Entities) { ListOfContactLists.Add(element.Id, element.Name); }
                pageResult = api.GetOutboundContactlists(false, false, PageSize, ++currentPage);
            }

            _contactListsLoaded = true;
            Log.Info($"All Contact Lists retrieved {ListOfContactLists.Count}");
        }

        /// <summary>
        /// Gat all divisions
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetDivisions()
        {
            Log.Debug("GetDivisions started");
            var temp = new Dictionary<string, string>();
            temp = GetDivisions(1, temp);
            foreach (var division in temp)
            {
                ListOfDivisions[division.Key] = division.Value;
            }

            _divisionsLoaded = true;

            return ListOfDivisions;
        }

        /// <summary>
        /// Get divisions, page by page
        /// </summary>
        /// <param name="page"></param>
        /// <param name="temp"></param>
        /// <returns></returns>
        private static Dictionary<string, string> GetDivisions(int page, Dictionary<string, string> temp)
        {
            var api = new AuthorizationApi();
            var result = api.GetAuthorizationDivisions(PageSize, page);

            if (result.Entities == null)
            {
                Log.Info("Empty Division result");
                return temp;
            }

            Log.Debug($"Number of Division results: {result.Entities.Count}");

            foreach (var division in result.Entities)
            {
                temp[division.Id] = division.Name;

            }
            if (temp.Count < result.Total)
            {
                Log.Debug($"More Divisions to retrieve {temp.Count} < {result.Total} ");
                return GetDivisions(page + 1, temp);
            }
            Log.Info($"All Division retrieved {temp.Count} / {result.Total} ");

            return temp;
        }


        /// <summary>
        /// Retrieve all data tables created in the system (max count is 25 in 2019)
        /// </summary>
        private void GetDataTables()
        {
            Log.Debug("GetAllDataTables started");
            ListOfDataTables.Clear();
            ListOfDataTableRows.Clear();
            var currentPage = 1;

            var api = new ArchitectApi();
            var result = api.GetFlowsDatatables("schema", 1, PageSize);

            foreach (var element in result.Entities)
            {
                ListOfDataTables.Add(element.Id, element.Name);
                GetDataTableRows(element.Id, 1);
            }

            _dataTablesLoaded = true;
            Log.Info($"All DataTables retrieved {ListOfDataTables.Count}");
            Log.Info($"All DataTableRows retrieved {ListOfDataTableRows.Count}");
        }



        /// <summary>
        /// Retrieve all information for users
        /// </summary>
        private void GetUserDetails()
        {
            Log.Debug("GetUserDetails started");
            ListOfUserRoles.Clear();
            ListOfUserSkills.Clear();
            ListOfUserQueues.Clear();
            ListOfUserInfos.Clear();

            var userApi = new UsersApi();
            var result = new UserEntityListing();
            var currentPage = 1;

            do
            {
                result = userApi.GetUsers(PageSize, currentPage++);

                foreach (var user in result.Entities)
                {
                    GetUserRoles(user.Id);
                    GetUserQueues(user.Id);
                    GetUserSkills(user.Id);
                    GetUserInfos(user.Id);
                }
            } while (result.Entities.Count == 100);

            Log.Info($"All User Details retrieved {ListOfUsers.Count}");
        }


        /// <summary>
        /// Retrieve all user roles per divisions
        /// </summary>
        //private void GetUserRoles()
        //{
        //    Log.Debug("GetUserRoles started");
        //    ListOfUserRoles.Clear();

        //    var userApi = new UsersApi();
        //    var result = new UserEntityListing();
        //    var currentPage = 1;

        //    do
        //    {
        //        result = userApi.GetUsers(PageSize, currentPage++);

        //        foreach (var user in result.Entities)
        //        {
        //            //Log.Info($">>>>>>>>>>> GetUserRoles {user.Name} {ListOfUserRoles.Count}");
        //            GetUserRoles(user.Id);
        //        }
        //    } while (result.Entities.Count == 100);

        //    Log.Info($"All User Roles retrieved {ListOfUserRoles.Count}");
        //}


        //private void GetUserSkills()
        //{
        //    Log.Debug("GetUserSkills started");
        //    ListOfUserSkills.Clear();

        //    var userApi = new UsersApi();
        //    var result = new UserEntityListing();
        //    var currentPage = 1;

        //    do
        //    {
        //        result = userApi.GetUsers(PageSize, currentPage++);

        //        foreach (var user in result.Entities)
        //        {
        //            //Log.Info($">>>>>>>>>>> GetUserSkills {user.Name} {ListOfUserRoles.Count}");
        //            GetUserSkills(user.Id);
        //            GetUserInfos(user.Id);
        //        }
        //    } while (result.Entities.Count == 100);

        //    Log.Info($"All User Skills retrieved {ListOfUserSkills.Count}");
        //}


        //private void GetUserQueues()
        //{
        //    Log.Debug("GetUserQueues started");
        //    ListOfUserQueues.Clear();

        //    var userApi = new UsersApi();
        //    var result = new UserEntityListing();
        //    var currentPage = 1;

        //    do
        //    {
        //        result = userApi.GetUsers(PageSize, currentPage++);

        //        foreach (var user in result.Entities)
        //        {
        //            //Log.Info($">>>>>>>>>>> GetUserSkills {user.Name} {ListOfUserRoles.Count}");
        //            GetUserQueues(user.Id);
        //        }
        //    } while (result.Entities.Count == 100);

        //    Log.Info($"All User Queues retrieved {ListOfUserQueues.Count}");
        //}

        /// <summary>
        /// Retrieve groups
        /// </summary>
        private void GetGroups()
        {
            Log.Debug("GetGroups started");
            ListOfGroups.Clear();
            ListOfGroupMembers.Clear();
            var currentPage = 1;

            var api = new GroupsApi();
            var result = api.GetGroups(PageSize, currentPage);

            while (result.Total > ListOfGroups.Count)
            {
                foreach (var element in result.Entities)
                {
                    ListOfGroups.Add(element.Id, element.Name);
                    GetGroupMembers(element.Id);
                }
                result = api.GetGroups(PageSize, ++currentPage);
            }

            _groupsLoaded = true;
            Log.Info($"All Groups retrieved {ListOfGroups.Count}");
            Log.Info($"All Group Members retrieved {ListOfGroupMembers.Count}");

        }

        /// <summary>
        /// Retrieve roles
        /// </summary>
        private void GetRoles()
        {
            Log.Debug("GetRoles started");
            ListOfRoles.Clear();
            var currentPage = 1;

            var api = new AuthorizationApi();
            var result = api.GetAuthorizationRoles(PageSize, currentPage);

            while (result.Total > ListOfRoles.Count)
            {
                foreach (var element in result.Entities)
                {
                    ListOfRoles.Add(element.Id, element.Name);
                }
                result = api.GetAuthorizationRoles(PageSize, ++currentPage);
            }

            _rolesLoaded = true;
            Log.Info($"All Roles retrieved {ListOfGroups.Count}");

        }

        /// <summary>
        /// Load all dictionaries in list of objects
        /// </summary>
        /// <returns></returns>
        public bool LoadAllDictionaries()
        {
            _userLoaded = false;            
            _queuesLoaded = false;
            _presencesLoaded = false;
            _languagesLoaded = false;
            _skillsLoaded = false;
            _wrapUpCodesLoaded = false;
            _edgeServersLoaded = false;
            _campaignsLoaded = false;
            _contactListsLoaded = false;
            _divisionsLoaded = false;
            _dataTablesLoaded = false;
            _groupsLoaded = false;
            _rolesLoaded = false;

            var loadUsers = new Task(() =>
            {
                GetUsers();
            });
            
            var loadQueues = new Task(() =>
            {
                GetQueues();
            });
            var loadLanguages = new Task(() =>
            {
                GetLanguages();
            });
            
            var loadSkills = new Task(() =>
            {
                GetSkills();
            });
            var loadWrapUpCodes = new Task(() =>
            {
                GetWrapUpCodes();
            });

            var loadDivisions = new Task(() =>
            {
                GetDivisions();
            });

            var loadEdgeServers = new Task(GetEdgeDictionary);
            var loadCampaigns = new Task(GetCampaigns);
            var loadContactLists = new Task(GetContactLists);
            var loadSystemPresence = new Task(GetPresenceDefinitions);
            var loadDataTables = new Task(GetDataTables);
            var loadGroups = new Task(GetGroups);
            var loadRoles = new Task(GetRoles);

            loadUsers.Start();
            loadQueues.Start();
            loadSystemPresence.Start();
            loadLanguages.Start();
            loadSkills.Start();
            loadWrapUpCodes.Start();
            loadEdgeServers.Start();
            loadCampaigns.Start();
            loadContactLists.Start();
            loadDivisions.Start();
            loadDataTables.Start();
            loadGroups.Start();
            loadRoles.Start();

            Task.WaitAll(loadUsers, loadQueues, loadSystemPresence,  loadLanguages, loadSkills, loadWrapUpCodes, loadEdgeServers, loadCampaigns, loadContactLists, loadDivisions, loadDataTables, loadGroups, loadRoles);
            return _userLoaded && _queuesLoaded && _presencesLoaded && _languagesLoaded && _skillsLoaded && _wrapUpCodesLoaded && _edgeServersLoaded && _campaignsLoaded && _contactListsLoaded && _divisionsLoaded && _dataTablesLoaded && _groupsLoaded && _rolesLoaded;
        }
        #endregion

        #region Pull Analytics

        /// <summary>
        /// Gets conversations for given interval from the Analytics API.
        /// </summary>
        /// <param name="interval">Start date of interval.</param>        
        /// <returns>List of conversations.</returns>
        public List<AnalyticsConversation> GetConversationData(Interval interval)
        {
            Log.Debug($"GetConversationData(), {nameof(interval)}:{interval}");
            var result = new List<AnalyticsConversation>();            
            try
            {
                var pageNumber = 1;
                var api = new AnalyticsApi();                
                var body = new ConversationQuery
                {
                    Interval = interval.ToString(),
                    Paging = new PagingSpec(PageSize, pageNumber)
                };
                Log.Info($"Getting conversations for interval: {body.Interval}");
                var pageResult = api.PostAnalyticsConversationsDetailsQuery(body);
                while (pageResult?.Conversations != null && pageResult.Conversations.Any())
                {
                    result.AddRange(pageResult.Conversations);
                    body.Paging.PageNumber++;
                    pageResult = api.PostAnalyticsConversationsDetailsQuery(body);                    
                }
                Log.Info($"Conversations for interval retrived: {result.Count}");
            }
            catch (Exception ex)
            {
                Log.Error("GetConversationData()", ex);
            }
            return result;
        }

        public List<AnalyticsConversation> GetConversationData(List<string> conversationIdList)
        {
            Log.Info("Getting conversation data by conversation ids from Analytics API.");
            var result = new List<AnalyticsConversation>();
            try
            {
                var analyticsApi = new AnalyticsApi();
                result.AddRange(conversationIdList.Select(conversationId => GetAnalyticsConversationById(conversationId, analyticsApi)));
                Log.Info($"Conversations by conversation ids requested:{conversationIdList.Count}, retrived:{result.Count}");
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
            return result;
        }

        public List<AnalyticsUserDetail> GetUserDetails(Interval interval)
        {
            Log.Debug($"GetUserDetails(), {nameof(interval)}:{interval}");
            var result = new List<AnalyticsUserDetail>();
            try
            {                         
                var api = new AnalyticsApi();
                var body = new UserDetailsQuery
                {
                    Interval = interval.ToString(),
                    Paging = new PagingSpec(PageSize, 1)
                };
                Log.Info($"Getting user details for interval: {body.Interval}");
                var pageResult = api.PostAnalyticsUsersDetailsQuery(body);
                while (pageResult?.UserDetails != null && pageResult.UserDetails.Any())
                {
                    result.AddRange(pageResult.UserDetails);
                    body.Paging.PageNumber++;
                    pageResult = api.PostAnalyticsUsersDetailsQuery(body);
                }
                Log.Info($"User details for interval retrived: {result.Count}");
            }
            catch (Exception ex)
            {
                Log.Error("GetUserDetails()", ex);
            }
            return result;
        }

        public AggregateQueryResponse GetConversationAggregates(Interval interval)
        {
            Log.Debug($"GetConversationAggregates(), {nameof(interval)}:{interval}");
            AggregateQueryResponse result = null;
            try
            {
                var api = new AnalyticsApi();
                var body = new AggregationQuery()
                {
                    Interval = interval.ToString(),
                    Granularity = Granularity,
                    GroupBy = new List<AggregationQuery.GroupByEnum>() { AggregationQuery.GroupByEnum.Queueid }
                };
                Log.Info($"Getting conversation aggregates for interval: {body.Interval}");
                result = api.PostAnalyticsConversationsAggregatesQuery(body);                
                Log.Info($"Conversation aggregates for interval retrived: {result?.Results?.Count}");
            }
            catch (Exception ex)
            {
                Log.Error("GetConversationAggregates()", ex);
            }
            return result;
        }

        public PresenceQueryResponse GetUserAggregates(Interval interval, int page)
        {
            Log.Debug($"GetUserAggregates(), {nameof(interval)}:{interval}, {nameof(page)}:{page}");
            PresenceQueryResponse result = null;
            try
            {                
                var usersPage = GetPageOfUsers(page);
                if (usersPage.Count < 1) { return null; }
                var groupby = new List<AggregationQuery.GroupByEnum> { AggregationQuery.GroupByEnum.Userid };
                var predicates = usersPage.Select(u => new AnalyticsQueryPredicate { Dimension = AnalyticsQueryPredicate.DimensionEnum.Userid, Value = u.Key }).ToList();                
                var filter = new AnalyticsQueryFilter(AnalyticsQueryFilter.TypeEnum.Or, null, predicates);
                var api = new AnalyticsApi();
                var body = new AggregationQuery()
                {
                    Interval = interval.ToString(),
                    Granularity = Granularity,
                    GroupBy = groupby,
                    Filter = filter
                };
                Log.Info($"Getting user aggregates. Interval: {body.Interval}, Page: {page}");
                result = api.PostAnalyticsUsersAggregatesQuery(body);
                Log.Info($"Got user aggregates. Interval: {body.Interval}, Page: {page}");                
            }
            catch (Exception ex)
            {
                Log.Error("GetUserAggregates()", ex);
            }
            return result;
        }

        private Dictionary<string, string> GetPageOfUsers(int page)
        {
            Log.Debug($"GetHundredOfUsers(), {nameof(page)}:{page}");            
            var skip = page * PageSize;            
            Log.Debug($"{nameof(skip)}:{skip}, {nameof(PageSize)}:{PageSize}");
            return ListOfUsers.OrderBy(o => o.Key).Skip(skip).Take(PageSize).ToDictionary(k => k.Key, v => v.Value);            
        }

        private AnalyticsConversation GetAnalyticsConversationById(string conversationId, IAnalyticsApi analyticsApi)
        {
            Log.Debug($"GetAnalyticsConversationById({conversationId})");
            if (string.IsNullOrWhiteSpace(conversationId)) throw new ArgumentNullException(nameof(conversationId));
            if (analyticsApi == null) throw new ArgumentNullException(nameof(analyticsApi));
            AnalyticsConversation result;
            try
            {
                result = analyticsApi.GetAnalyticsConversationDetails(conversationId);
            }
            catch (ApiException ex)
            {
                if (ex.ErrorCode != 429) throw;
				string ratelimitCount, ratelimitAllowed, ratelimitReset;
				ex.Headers.TryGetValue("inin-ratelimit-count", out ratelimitCount);
                ex.Headers.TryGetValue("inin-ratelimit-allowed", out ratelimitAllowed);
                ex.Headers.TryGetValue("inin-ratelimit-reset", out ratelimitReset);
                Log.Info($"API rate limit has been reached, {nameof(ratelimitCount)}:{ratelimitCount}, {nameof(ratelimitAllowed)}:{ratelimitAllowed}, {nameof(ratelimitReset)}:{ratelimitReset}");
                var resetTimeSeconds = 60; // default value in case that header parsing will go wrong                
                int.TryParse(ratelimitReset, out resetTimeSeconds);
                if (resetTimeSeconds > 60) throw new Exception("API rate limit reset > 60"); // if resetTimeSeconds is grather than 60 it means that something is wrong
                var resetTime = DateTime.Now.AddSeconds(resetTimeSeconds).AddMilliseconds(500); // adding a few milliseconds as a margin of error
                while (resetTime > DateTime.Now)
                {
                    Log.Debug($"Waiting, {nameof(resetTime)}:{resetTime:O}");
                    Thread.Sleep(200);
                }
                Log.Info($"Re-calling method {nameof(GetAnalyticsConversationById)}");
                result = GetAnalyticsConversationById(conversationId, analyticsApi);
            }
            return result;
        }


        #endregion

        #region "pull Conversations"

        public List<ParticipantAttr> GetParticipantAttrs(List<string> conversationIdList)
        {
            Log.Info("Getting conversation data by conversation ids from Conversations API.");
            var result = new List<ParticipantAttr>();
            try
            {
                var conversationsApi = new ConversationsApi();
                
                foreach (var conversationId in conversationIdList)
                {
                    var conversation = GetConversationById(conversationId, conversationsApi);
                    if (!conversation.Participants.Any()) continue;
                    foreach (var participant in conversation.Participants.Where(x => x.Attributes != null && x.Attributes.Any()).ToList())
                    {
                        foreach (var attr in participant.Attributes)
                        {
                            if (!result.Any(x => x.ConversationId.Equals(conversation.Id, StringComparison.OrdinalIgnoreCase) && x.ParticipantId.Equals(participant.Id, StringComparison.OrdinalIgnoreCase) && x.AttrName.Equals(attr.Key, StringComparison.OrdinalIgnoreCase)))
                            {
                                result.Add(new ParticipantAttr() { ConversationId = conversation.Id, ParticipantId = participant.Id, AttrName = attr.Key, AttrValue = attr.Value });
                            }
                        }                        
                    }
                }    
                Log.Info($"Participant attributes retrived:{result.Count}");
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
            return result;
        }



        private Conversation GetConversationById(string conversationId, IConversationsApi conversationsApi)
        {
            if (string.IsNullOrWhiteSpace(conversationId)) throw new ArgumentNullException(nameof(conversationId));
            if (conversationsApi == null) throw new ArgumentNullException(nameof(conversationsApi));
            Conversation result;
            try
            {
                result = conversationsApi.GetConversation(conversationId);
            }
            catch (ApiException ex)
            {
                if (ex.ErrorCode != 429) throw;
                string ratelimitCount;
                string ratelimitAllowed;
                string ratelimitReset;
                ex.Headers.TryGetValue("inin-ratelimit-count", out ratelimitCount);
                ex.Headers.TryGetValue("inin-ratelimit-allowed", out ratelimitAllowed);
                ex.Headers.TryGetValue("inin-ratelimit-reset", out ratelimitReset);
                Log.Info($"API rate limit has been reached, {nameof(ratelimitCount)}:{ratelimitCount}, {nameof(ratelimitAllowed)}:{ratelimitAllowed}, {nameof(ratelimitReset)}:{ratelimitReset}");
                var resetTimeSeconds = 60; // default value in case that header parsing will go wrong                
                int.TryParse(ratelimitReset, out resetTimeSeconds);
                if (resetTimeSeconds > 60) throw new Exception("API rate limit reset > 60"); // if resetTimeSeconds is grather than 60 it means that something is wrong
                var resetTime = DateTime.Now.AddSeconds(resetTimeSeconds).AddMilliseconds(500); // adding a few milliseconds as a margin of error
                while (resetTime > DateTime.Now)
                {
                    Log.Debug($"Waiting, {nameof(resetTime)}:{resetTime.ToString("O")}");
                    Thread.Sleep(200);
                }
                Log.Info($"Re-calling method {nameof(GetConversationById)}");
                result = GetConversationById(conversationId, conversationsApi);
            }
            return result;
        }


        public List<ConversationMetric> GetConversationMetrics(List<string> conversationIdList)
        {
            Log.Info("Getting conversation metrics by conversation ids from Conversations API.");
            var result = new List<ConversationMetric>();
            try
            {
                var analyticsApi = new AnalyticsApi();

                foreach (var conversationId in conversationIdList)
                {
                    var conversation = GetAnalyticsConversationById(conversationId, analyticsApi);
                    if (!conversation.Participants.Any()) continue;
                    foreach (var participant in conversation.Participants)
                    {
                        if (!participant.Sessions.Any()) continue;

                        foreach (var session in participant.Sessions)
                        {
                            //Log.Info($">>> session - {session.ToString()}");

                            var test = session.ToString();
                            if (!test.Contains("Metrics:")) continue;

                            if (session.Metrics == null) continue;

                            if (!session.Metrics.Any()) continue;

                            foreach (var metric in session.Metrics)
                            {
                                //Log.Info($">>> metrics retrieved:{result.Count} - {conversationId}");
                                if (metric == null) continue;
                                //Log.Info($"metrics retrieved:{result.Count} -  Name = {metric.Name}, Value = {metric.Value}, EmitDate = {metric.EmitDate}");
                                result.Add(new ConversationMetric() { SessionId = session.SessionId, ConversationId = conversationId, ParticipantId = participant.ParticipantId, attrName = metric.Name, attrValue = (long)metric.Value, emitDate = (DateTime)metric.EmitDate });
                            }
                        }

                    }
                }
                Log.Info($"Conversation metrics retrived:{result.Count}");
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
            return result;
        }


        /// <summary>
        /// Get group members from the groupid
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="currentPage"></param>
        private void GetGroupMembers(string groupId)
        {
            try
            {
                Log.Debug("GetGroupMembers started");

                var api = new GroupsApi();
                //var result = api.GetGroupMembers(groupId, PageSize, 1);
                var result = api.GetGroupIndividuals(groupId);
                //Log.Info($">>>>> api.GetGroupMembers #{ListOfGroups.Count}");

                foreach (var element in result.Entities)
                {
                    ListOfGroupMembers.Add(new GroupMember() { id = element.Id, name = element.Name, groupId = groupId });
                }

                Log.Debug($"All Groups retrieved {ListOfGroups.Count}");
            }
            catch (ApiException ex)
            {
                if (ex.ErrorCode != 429) throw;
                string ratelimitCount;
                string ratelimitAllowed;
                string ratelimitReset;
                ex.Headers.TryGetValue("inin-ratelimit-count", out ratelimitCount);
                ex.Headers.TryGetValue("inin-ratelimit-allowed", out ratelimitAllowed);
                ex.Headers.TryGetValue("inin-ratelimit-reset", out ratelimitReset);
                Log.Info($"API rate limit has been reached, {nameof(ratelimitCount)}:{ratelimitCount}, {nameof(ratelimitAllowed)}:{ratelimitAllowed}, {nameof(ratelimitReset)}:{ratelimitReset}");
                var resetTimeSeconds = 60; // default value in case that header parsing will go wrong                
                int.TryParse(ratelimitReset, out resetTimeSeconds);
                if (resetTimeSeconds > 60) throw new Exception("API rate limit reset > 60"); // if resetTimeSeconds is grather than 60 it means that something is wrong
                var resetTime = DateTime.Now.AddSeconds(resetTimeSeconds).AddMilliseconds(500); // adding a few milliseconds as a margin of error
                while (resetTime > DateTime.Now)
                {
                    Log.Debug($"Waiting, {nameof(resetTime)}:{resetTime.ToString("O")}");
                    Thread.Sleep(200);
                }
                Log.Info($"Re-calling method {nameof(GetGroupMembers)}");
                GetGroupMembers(groupId);
            }
        }


        /// <summary>
        /// Get user's role by divisions
        /// </summary>
        /// <param name="userId"></param>
        private void GetUserRoles(string userId)
        {
            try
            {
                //Log.Debug("GetUserRoles started");

                var userAuth = new AuthorizationApi();

                var auth = userAuth.GetAuthorizationSubject(userId);

                if (auth.Grants != null)
                {
                    foreach (var result in auth.Grants)
                    {
                        ListOfUserRoles.Add(new UserRole() { UserId = result.SubjectId, Roles = result.Role.Id, Division = result.Division.Id });
                        //Log.Info($">>>>>>>>>>> GetUserRoles {result.SubjectId} {result.Role.Id} {result.Division.Id}");
                    }
                }

                //Log.Debug($"All User roles retrieved {ListOfUserRoles.Count}");
            }
            catch (ApiException ex)
            {
                if (ex.ErrorCode != 429) throw;
                string ratelimitCount;
                string ratelimitAllowed;
                string ratelimitReset;
                ex.Headers.TryGetValue("inin-ratelimit-count", out ratelimitCount);
                ex.Headers.TryGetValue("inin-ratelimit-allowed", out ratelimitAllowed);
                ex.Headers.TryGetValue("inin-ratelimit-reset", out ratelimitReset);
                Log.Info($"API rate limit has been reached, {nameof(ratelimitCount)}:{ratelimitCount}, {nameof(ratelimitAllowed)}:{ratelimitAllowed}, {nameof(ratelimitReset)}:{ratelimitReset}");
                var resetTimeSeconds = 60; // default value in case that header parsing will go wrong                
                int.TryParse(ratelimitReset, out resetTimeSeconds);
                if (resetTimeSeconds > 60) throw new Exception("API rate limit reset > 60"); // if resetTimeSeconds is grather than 60 it means that something is wrong
                var resetTime = DateTime.Now.AddSeconds(resetTimeSeconds).AddMilliseconds(500); // adding a few milliseconds as a margin of error
                while (resetTime > DateTime.Now)
                {
                    Log.Debug($"Waiting, {nameof(resetTime)}:{resetTime.ToString("O")}");
                    Thread.Sleep(200);
                }
                Log.Info($"Re-calling method {nameof(GetUserRoles)}");
                GetUserRoles(userId);
            }
        }

        /// <summary>
        /// Get user's skill
        /// </summary>
        /// <param name="userId"></param>
        private void GetUserSkills(string userId)
        {
            try
            {
                Log.Debug("GetUserSkills started");

                var users = new UsersApi();

                var user = users.GetUserRoutingskills(userId);

                if (user.Entities != null)
                {
                    foreach (var result in user.Entities)
                    {
                        ListOfUserSkills.Add(new UserSkill() { UserId = userId, Skill = result.Id, Level = result.Proficiency.Value.ToString() });
                    }
                }

                Log.Debug($"All User roles retrieved {ListOfUserSkills.Count}");
            }
            catch (ApiException ex)
            {
                if (ex.ErrorCode != 429) throw;
                string ratelimitCount;
                string ratelimitAllowed;
                string ratelimitReset;
                ex.Headers.TryGetValue("inin-ratelimit-count", out ratelimitCount);
                ex.Headers.TryGetValue("inin-ratelimit-allowed", out ratelimitAllowed);
                ex.Headers.TryGetValue("inin-ratelimit-reset", out ratelimitReset);
                Log.Info($"API rate limit has been reached, {nameof(ratelimitCount)}:{ratelimitCount}, {nameof(ratelimitAllowed)}:{ratelimitAllowed}, {nameof(ratelimitReset)}:{ratelimitReset}");
                var resetTimeSeconds = 60; // default value in case that header parsing will go wrong                
                int.TryParse(ratelimitReset, out resetTimeSeconds);
                if (resetTimeSeconds > 60) throw new Exception("API rate limit reset > 60"); // if resetTimeSeconds is grather than 60 it means that something is wrong
                var resetTime = DateTime.Now.AddSeconds(resetTimeSeconds).AddMilliseconds(500); // adding a few milliseconds as a margin of error
                while (resetTime > DateTime.Now)
                {
                    Log.Debug($"Waiting, {nameof(resetTime)}:{resetTime.ToString("O")}");
                    Thread.Sleep(200);
                }
                Log.Info($"Re-calling method {nameof(GetUserSkills)}");
                GetUserSkills(userId);
            }
        }

        /// <summary>
        /// Get user's queues
        /// </summary>
        /// <param name="userId"></param>
        private void GetUserQueues(string userId)
        {
            try
            {
                Log.Debug("GetUserQueues started");

                var users = new UsersApi();

                var user = users.GetUserQueues(userId, PageSize, 1);

                if (user.Entities != null)
                {
                    foreach (var result in user.Entities)
                    {
                        ListOfUserQueues.Add(new pcstat.Model.UserQueue() { UserId = userId, Queue = result.Id });
                    }
                }

                Log.Debug($"All User queues retrieved {ListOfUserQueues.Count}");
            }
            catch (ApiException ex)
            {
                if (ex.ErrorCode != 429) throw;
                string ratelimitCount;
                string ratelimitAllowed;
                string ratelimitReset;
                ex.Headers.TryGetValue("inin-ratelimit-count", out ratelimitCount);
                ex.Headers.TryGetValue("inin-ratelimit-allowed", out ratelimitAllowed);
                ex.Headers.TryGetValue("inin-ratelimit-reset", out ratelimitReset);
                Log.Info($"API rate limit has been reached, {nameof(ratelimitCount)}:{ratelimitCount}, {nameof(ratelimitAllowed)}:{ratelimitAllowed}, {nameof(ratelimitReset)}:{ratelimitReset}");
                var resetTimeSeconds = 60; // default value in case that header parsing will go wrong                
                int.TryParse(ratelimitReset, out resetTimeSeconds);
                if (resetTimeSeconds > 60) throw new Exception("API rate limit reset > 60"); // if resetTimeSeconds is grather than 60 it means that something is wrong
                var resetTime = DateTime.Now.AddSeconds(resetTimeSeconds).AddMilliseconds(500); // adding a few milliseconds as a margin of error
                while (resetTime > DateTime.Now)
                {
                    Log.Debug($"Waiting, {nameof(resetTime)}:{resetTime.ToString("O")}");
                    Thread.Sleep(200);
                }
                Log.Info($"Re-calling method {nameof(GetUserQueues)}");
                GetUserQueues(userId);
            }
        }


        /// <summary>
        /// Get user's information from the user profile
        /// </summary>
        /// <param name="userId"></param>
        private void GetUserInfos(string userId)
        {
            try
            {
                Log.Debug("GetUserQueues started");

                List<string> criteria = new List<string>();
                criteria.Add("routingStatus");
                criteria.Add("presence");
                criteria.Add("conversationSummary");
                criteria.Add("outOfOffice");
                criteria.Add("geolocation");
                criteria.Add("station");
                criteria.Add("authorization");

                criteria.Add("certifications");
                criteria.Add("groups");
                criteria.Add("employerinfo");
                criteria.Add("locations");
                criteria.Add("profileskills");
                criteria.Add("skills");
                criteria.Add("languagepreference");

                var users = new UsersApi();

                var user = users.GetUser(userId, criteria);


                if (user != null)
                {
                    string location = "";
                    foreach (var loc in user.Locations)
                    {
                        location = loc.Id;
                    }

                    //var location2 = user.Locations.LastIndexOf(0);

                    ListOfUserInfos.Add(new UserInformation() { UserId = userId, Email = user.Email, Department = user.Department, Title = user.Title, Locations = location });
                }

                Log.Debug($"All User queues retrieved {ListOfUserQueues.Count}");
            }
            catch (ApiException ex)
            {
                if (ex.ErrorCode != 429) throw;
                string ratelimitCount;
                string ratelimitAllowed;
                string ratelimitReset;
                ex.Headers.TryGetValue("inin-ratelimit-count", out ratelimitCount);
                ex.Headers.TryGetValue("inin-ratelimit-allowed", out ratelimitAllowed);
                ex.Headers.TryGetValue("inin-ratelimit-reset", out ratelimitReset);
                Log.Info($"API rate limit has been reached, {nameof(ratelimitCount)}:{ratelimitCount}, {nameof(ratelimitAllowed)}:{ratelimitAllowed}, {nameof(ratelimitReset)}:{ratelimitReset}");
                var resetTimeSeconds = 60; // default value in case that header parsing will go wrong                
                int.TryParse(ratelimitReset, out resetTimeSeconds);
                if (resetTimeSeconds > 60) throw new Exception("API rate limit reset > 60"); // if resetTimeSeconds is grather than 60 it means that something is wrong
                var resetTime = DateTime.Now.AddSeconds(resetTimeSeconds).AddMilliseconds(500); // adding a few milliseconds as a margin of error
                while (resetTime > DateTime.Now)
                {
                    Log.Debug($"Waiting, {nameof(resetTime)}:{resetTime.ToString("O")}");
                    Thread.Sleep(200);
                }
                Log.Info($"Re-calling method {nameof(GetUserQueues)}");
                GetUserQueues(userId);
            }
        }


        /// <summary>
        /// Get datatables with rows
        /// </summary>
        /// <param name="dataTableId"></param>
        /// <param name="currentPage"></param>
        private void GetDataTableRows(string dataTableId, int currentPage)
        {
            try
            {
                Log.Debug("GetDataTableRows started");

                var api = new ArchitectApi();
                var result = api.GetFlowsDatatableRows(dataTableId, currentPage, PageSize, false);

                string row = "";

                foreach (var element in result.Entities)
                {
                    foreach (var column in element)
                    {
                        row = row + column.Key + "|" + column.Value + "$";
                    }

                    ListOfDataTableRows.Add(new DataTableRows() { dataTableId = dataTableId, dataTableRows = row });
                }
            }
            catch (ApiException ex)
            {
                if (ex.ErrorCode != 429) throw;
                string ratelimitCount;
                string ratelimitAllowed;
                string ratelimitReset;
                ex.Headers.TryGetValue("inin-ratelimit-count", out ratelimitCount);
                ex.Headers.TryGetValue("inin-ratelimit-allowed", out ratelimitAllowed);
                ex.Headers.TryGetValue("inin-ratelimit-reset", out ratelimitReset);
                Log.Info($"API rate limit has been reached, {nameof(ratelimitCount)}:{ratelimitCount}, {nameof(ratelimitAllowed)}:{ratelimitAllowed}, {nameof(ratelimitReset)}:{ratelimitReset}");
                var resetTimeSeconds = 60; // default value in case that header parsing will go wrong                
                int.TryParse(ratelimitReset, out resetTimeSeconds);
                if (resetTimeSeconds > 60) throw new Exception("API rate limit reset > 60"); // if resetTimeSeconds is grather than 60 it means that something is wrong
                var resetTime = DateTime.Now.AddSeconds(resetTimeSeconds).AddMilliseconds(500); // adding a few milliseconds as a margin of error
                while (resetTime > DateTime.Now)
                {
                    Log.Debug($"Waiting, {nameof(resetTime)}:{resetTime.ToString("O")}");
                    Thread.Sleep(200);
                }
                Log.Info($"Re-calling method {nameof(GetDataTableRows)}");
                GetDataTableRows(dataTableId, currentPage++);
            }
            Log.Debug($"All DataTableRows retrieved {ListOfDataTableRows.Count}");
        }

        #endregion
    }
}