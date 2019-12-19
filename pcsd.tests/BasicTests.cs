using Microsoft.VisualStudio.TestTools.UnitTesting;
using pcsd.plugins;
using System.Collections.Generic;

namespace unittests.consoleapp
{
    [TestClass]
    public class BasicTests
    {
        PluginLoader pluginLoader = new PluginLoader();

        [TestInitialize]
        public void Initialize()
        {

        }

        [TestMethod]
        public void NoPluginsTest()
        {
            List<string[]> pluginsCmdArgsHelp;
            Assert.IsTrue(pluginLoader.LoadPlugins("", new string[] { }, out pluginsCmdArgsHelp).Count == 0);
        }

        [TestMethod]
        public void BadPluginTest()
        {
            List<string[]> pluginsCmdArgsHelp;
            Assert.IsTrue(pluginLoader.LoadPlugins("I don't exist", new string[] { }, out pluginsCmdArgsHelp).Count == 0);
        }

        [TestMethod]
        public void LoadPluginThatExist()
        {

        }
    }
}
