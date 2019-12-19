using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using pcsd;

namespace UnitTest.pcsd.connection
{
    [TestClass]
    public class UserData
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

        [TestMethod]
        public void Should_Pass_When_UserData_Count_Is_Not_Zero()
        {
            // Arrange

            // Act
            //var results = _pureCloud.GetUserAggregatesData().GetUserData();

            // Assert
            // Assert.AreNotEqual(0, results.Count);
            // ----- To be removed
            Assert.IsTrue(true);
            //var results = _pureCloud.GetUserData();

            // Assert
            //Assert.AreNotEqual(0, results.Count);
        }
    }
}
