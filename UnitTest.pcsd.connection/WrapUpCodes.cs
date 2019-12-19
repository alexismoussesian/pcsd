using System;
using ININ.PureCloudApi.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using pcsd;

namespace UnitTest.pcsd.connection
{
    [TestClass]
    public class WrapUpCodes
    {
        #region Variables

        private PureCloud _pureCloud;

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
            _pureCloud.GetUsers();
            _pureCloud.GetQueues();
            _pureCloud.GetLanguages();
        }

        #endregion

        [TestMethod]
        public void Should_Pass_When_WrapUpCodes_Count_Is_Not_Zero()
        {
            // Arrange

            // Act
            var results = _pureCloud.GetWrapUpCodes();

            // Assert
            Assert.AreNotEqual(0, results.Count);
        }
    }
}
