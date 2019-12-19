using System;
using ININ.PureCloudApi.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using pcsd;

namespace UnitTest.pcsd.connection
{
    [TestClass]
    public class Login
    {
        #region Variables

        private PureCloud _pureCloud;

        #endregion

        #region TestInitialize

        [TestInitialize]
        public void TestInitialize()
        {
            _pureCloud = new PureCloud();
        }

        #endregion

        #region Should_Fail

        [TestMethod]
        public void Should_Fail_When_ClientId_Is_Empty()
        {
            // Arrange
            // Done in TestInitialize
            // Act
            // Assert
            Assert.IsTrue(string.IsNullOrEmpty(_pureCloud.ClientId));
        }

        [TestMethod]
        public void Should_Fail_When_ClientSecret_Is_Empty()
        {
            // Arrange
            // Done in TestInitialize
            // Act
            // Assert
            Assert.IsTrue(string.IsNullOrEmpty(_pureCloud.ClientSecret));
        }

        [TestMethod]
        public void Should_Fail_When_Environment_Is_Empty()
        {
            // Arrange
            // Done in TestInitialize
            // Act
            // Assert
            Assert.IsTrue(string.IsNullOrEmpty(_pureCloud.Environment));
        }

        [TestMethod]
        public void Should_Fail_When_No_ClientId_Were_Specified()
        {
            // Arrange

            // Act
            try
            {
                _pureCloud.Login();
            }
            catch (Exception ex)
            {
                // Assert
                Assert.AreEqual(typeof(ArgumentException), ex.GetType());
            }
        }

        [TestMethod]
        public void Should_Fail_When_No_ClientSecret_Were_Specified()
        {
            // Arrange
            _pureCloud.ClientId = "test";

            // Act
            try
            {
                _pureCloud.Login();
            }
            catch (Exception ex)
            {
                // Assert
                Assert.AreEqual(typeof(ArgumentException), ex.GetType());
            }
        }

        [TestMethod]
        public void Should_Fail_When_No_Environment_Were_Specified()
        {
            // Arrange
            _pureCloud.ClientId = "test";
            _pureCloud.ClientSecret = "test";

            // Act
            try
            {
                _pureCloud.Login();
            }
            catch (Exception ex)
            {
                // Assert
                Assert.AreEqual(typeof(ArgumentException), ex.GetType());
            }
        }

        [TestMethod]
        public void Should_Fail_When_Properties_Are_Unknown()
        {
            // Arrange
            Configuration.Default.AccessToken = null;
            _pureCloud.ClientId = "test";
            _pureCloud.ClientSecret = "test";
            _pureCloud.Environment = "test";

            // Act
            try
            {
                _pureCloud.Login();
            }
            catch (Exception)
            {
                // Assert
                Assert.AreEqual(null, Configuration.Default.AccessToken);
            }
        }

        #endregion

        #region Should_Pass

        [TestMethod]
        public void Should_Pass_When_Properties_Are_Not_NullOrEmpty()
        {
            // Arrange
            _pureCloud.ClientId = StaticConfig.ClientId;
            _pureCloud.ClientSecret = StaticConfig.ClientSecret;
            _pureCloud.Environment = StaticConfig.Environment;

            // Act
            _pureCloud.Login();

            // Assert
            Assert.IsNotNull(Configuration.Default.AccessToken);
        }

        [TestMethod]
        public void Should_Pass_When_Properties_Are_SentToTheMethod()
        {
            // Arrange

            // Act
            _pureCloud.Login(
                StaticConfig.ClientId,
                StaticConfig.ClientSecret,
                StaticConfig.Environment);

            // Assert
            Assert.IsNotNull(Configuration.Default.AccessToken);
        }

        #endregion
    }
}
