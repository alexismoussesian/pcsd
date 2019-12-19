using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using pcsd;
using pcsd.plugins;
using UnitTest.pcsd.connection.Helpers;

namespace UnitTest.pcsd.connection.Plugins
{
    [TestClass]
    public class ConversationData
    {
        #region Variables

        private PureCloud _pureCloud;
        private string _pluginsFolder;
        private string[] _args;

        /// <summary>
        /// Contains the list of plugins that were successfully loaded from the Plugins folder
        /// </summary>
        private static ICollection<IPlugin> _loadedPlugins;
        private static List<string[]> _pluginsCmdArgsHelp;
        #endregion

        #region TestInitialize

        [TestInitialize]
        public void TestInitialize()
        {
            _pluginsFolder = FileHelper.GetPluginsFolder();

            _args = new string[] { $"/clientid={StaticConfig.ClientId}",
                $"/clientsecret={StaticConfig.ClientSecret}",
                $"/environment={StaticConfig.Environment}",
                //"/stats=user",
                $"/target-sql={StaticConfig.Targetsql}",
                "/music"};

            // Load plugins
            _loadedPlugins = new PluginLoader().LoadPlugins(_pluginsFolder, _args, out _pluginsCmdArgsHelp);
            _pureCloud = new PureCloud
            {
                ClientId = StaticConfig.ClientId,
                ClientSecret = StaticConfig.ClientSecret,
                Environment = StaticConfig.Environment
            };

            _pureCloud.Login();
            _pureCloud.GetUsers();
        }

        #endregion
    }
}
