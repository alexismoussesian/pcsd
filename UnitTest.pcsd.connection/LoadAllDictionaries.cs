using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using pcsd;

namespace UnitTest.pcsd.connection
{
    [TestClass]
    public class LoadAllDictionaries
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
        public void Should_Pass_When_AllDictionaries_Are_Loaded()
        {
            // Arrange

            // Act
            _pureCloud.LoadAllDictionaries();

            // Assert
            Assert.IsTrue(true);
        }
    }
}
