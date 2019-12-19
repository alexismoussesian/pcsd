using Microsoft.VisualStudio.TestTools.UnitTesting;
using pcsd;

namespace unittests.sql
{
	[TestClass]
    public class SqlPlugin
    {
        #region Variables

        private string _queueData = string.Empty;
        private PureCloud _pureCloud;
        private readonly pcsd.plugin.sql.SqlPlugin _sql = new pcsd.plugin.sql.SqlPlugin();

        #endregion

        #region TestInitialize

        [TestInitialize]
		public void TestInitialize()
        {
            _pureCloud = new PureCloud
            {
                ClientId = StaticConfig.ClientId,
                ClientSecret = StaticConfig.ClientSecret,
                Environment = StaticConfig.Environment
            };
            _pureCloud.Login();

            var args = new string[] { $"/clientid={StaticConfig.ClientId}",
                $"/clientsecret={StaticConfig.ClientSecret}",
                $"/environment={StaticConfig.Environment}",
                $"/target-sql={StaticConfig.Targetsql}"
            };

            _pureCloud.GetQueues();
            _pureCloud.GetUsers();
            _pureCloud.GetWrapUpCodes();

            _sql.Initialize(args);
            _sql.InitializeDictionaries(_pureCloud.ListOfQueues, _pureCloud.ListOfLanguages, _pureCloud.ListOfSkills, _pureCloud.ListOfUsers, _pureCloud.ListOfWrapUpCodes, _pureCloud.ListOfEdgeServers, _pureCloud.ListOfCampaigns, _pureCloud.ListOfContactLists, _pureCloud.ListOfPresences, _pureCloud.ListOfDivisions, _pureCloud.ListOfDataTables, _pureCloud.ListOfGroups, _pureCloud.ListOfRoles);

        }

        #endregion

        #region Should_Fail

        [TestMethod]
        public void Should_Fail_When_Initialize_With_No_Args()
        {
        }

        #endregion

    }
}
