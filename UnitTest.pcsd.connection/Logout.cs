using System;
using ININ.PureCloudApi.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using pcsd;

namespace UnitTest.pcsd.connection
{
    [TestClass]
    public class Logout
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
        }

        #endregion

        #region Should_Fail

        #endregion

        #region Should_Pass

        [TestMethod]
        public void Should_Pass_When_Properties_Are_Correct()
        {
            // Arrange
            // Done in TestInitialize

            try
            {
                // Act
                _pureCloud.Logout();

                // Assert
                Assert.IsNotNull(Configuration.Default.AccessToken);
            }
            catch
            {
                // Assert
                Assert.Fail();
            }
        }

        #endregion
    }
}
