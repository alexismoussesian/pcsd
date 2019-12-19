using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using pcsd;
using pcsd.plugin.sql;
using pcsd.plugins;

namespace UnitTest.pcsd.connection
{
    [TestClass]
    public class SqlPluginTests
    {
        #region Variables

        private SqlPlugin _sqlPlugin;

        #endregion

        #region TestInitialize

        [TestInitialize]
        public void TestInitialize()
        {
        _sqlPlugin = new SqlPlugin();
        }

        #endregion

        #region Should_Pass

        [TestMethod]
        public void Should_Fail_When_Initialize_Agrs_Is_Empty()
        {
            // Arrange

            // Act
            try
            {
                // Assert
                _sqlPlugin.Initialize(new string[] { "" });
                Assert.Fail();
            }
            catch (Exception ex)
            {
                // Assert
                Assert.IsNotNull(ex);
            }
        }

        [TestMethod]
        public void Should_Pass_When_Initialize_Agrs_Is_Ok()
        {
            // Arrange
            var pluginArgs = new List<string>() {"sql"};

            // Act
            try
            {
                // Assert
                _sqlPlugin.Initialize(pluginArgs.ToArray());
                Assert.Fail();
            }
            catch (Exception ex)
            {
                // Assert
                Assert.IsNotNull(ex);
            }
        }

        #endregion
    }
}
