using System;
using ININ.PureCloudApi.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using pcsd;

namespace UnitTest.pcsd.connection
{
    [TestClass]
    public class Languages
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
        public void Should_Pass_When_Language_Count_Is_Not_Zero()
        {
            // Arrange

            // Act
            var results = _pureCloud.GetLanguages();

            // Assert
            Assert.AreNotEqual(0, results.Count);
        }

        #endregion
    }
}
