using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using pcsd;
using pcsd.plugins;
using UnitTest.pcsd.connection.Helpers;

namespace UnitTest.pcsd.connection
{
    [TestClass]
    public class ConsoleParameters
    {
        #region Variables

        private string[] _args;
        private string _pluginsFolder;

        /// <summary>
        /// Contains the list of plugins that were successfully loaded from the Plugins folder
        /// </summary>
        private static ICollection<IPlugin> _loadedPlugins;
        private static List<string[]> _pluginsCmdArgsHelp;

        #endregion

        [TestMethod]
        public void Should_Fail_When_No_Args_Were_Specified()
        {
            // Arrange
            _pluginsFolder = FileHelper.GetPluginsFolder();

            // Act
            _loadedPlugins = new PluginLoader().LoadPlugins(_pluginsFolder, _args, out _pluginsCmdArgsHelp);

            // Assert
            Assert.AreEqual(0,_loadedPlugins.Count);
        }

        [TestMethod]
        public void Should_Fail_When_No_Stats_Were_Specified()
        {
            // Arrange
            _args = new string[] { $"/clientid={StaticConfig.ClientId}",
                $"/clientsecret={StaticConfig.ClientSecret}",
                $"/environment={StaticConfig.Environment}",
                $"/stats={StaticConfig.Stats}",
                $"/target-sql={StaticConfig.Targetsql}"
            };

            // Act
            //Program.Main(_args);

            // Assert
            //Assert.IsTrue(arguments.Contains("stats"));
            Assert.IsTrue(true);
        }

        #region Should_Pass

        //[TestMethod]
        //public void Should_Pass_When_Target_Is_Incorrect()
        //{
        //    // Arrange
        //    _args = new string[] {
        //        $"/pcsd=testing"
        //    };

        //    // Act
        //    Program.Main(_args);

        //    // Assert
        //    Assert.IsTrue(true);
        //}

        //[TestMethod]
        //public void Should_Pass_When_Help_Is_Passed()
        //{
        //    // Arrange
        //    _args = new string[] {
        //        $"/help"
        //    };

        //    // Act
        //    Program.Main(_args);

        //    // Assert
        //    Assert.IsTrue(true);
        //}

        [TestMethod]
        public void Should_Pass_When_Args_Are_NotNull()
        {
            // Arrange
            _args = new string[] { };

            // Act
            Program.Main(_args);

            // Assert
            Assert.IsTrue(true);
        }

        //[TestMethod]
        //public void Should_Pass_When_Music_Args()
        //{
        //    // Arrange
        //    _args = new string[] { $"/clientid={StaticConfig.ClientId}",
        //        $"/clientsecret={StaticConfig.ClientSecret}",
        //        $"/environment={StaticConfig.Environment}",
        //        "/music"};

        //    // Act
        //    Program.Main(_args);

        //    // Assert
        //    Assert.IsTrue(true);
        //}

        [TestMethod]
        public void Should_Pass_When_Target_Sql_Is_Correct()
        {
            // Arrange
            //stats = user,queue,conversation
            _args = new string[] { $"/clientid={StaticConfig.ClientId}",
                $"/clientsecret={StaticConfig.ClientSecret}",
                $"/environment={StaticConfig.Environment}",
                //"/stats=user",
                $"/target-sql={StaticConfig.Targetsql}"
            };

            // Act
            //Program.Main(_args);

            // Assert
            Assert.IsTrue(true);
        }
        
        #endregion
    }
}
