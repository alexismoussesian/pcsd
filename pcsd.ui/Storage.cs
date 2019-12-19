using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace pcsd.ui
{
    class Storage
    {
        private static string ConfigurationFolderPath => $"{Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)}\\pcsd.ui";
        private static string ConfigurationFilePath => $"{ConfigurationFolderPath}\\pcsd.ui.config";

        public static void Save(List<Config> configList)
        {
            if (!Directory.Exists(ConfigurationFolderPath)) Directory.CreateDirectory(ConfigurationFolderPath);
            var serializer = new JsonSerializer();
            var stringWriter = new StringWriter();
            serializer.Serialize(stringWriter, configList);            
            File.WriteAllText(ConfigurationFilePath, stringWriter.ToString());
        }

        public static List<Config> Load()
        {
            if (!File.Exists(ConfigurationFilePath)) return new List<Config>();
            var jsonString = File.ReadAllText(ConfigurationFilePath);            
            var serializer = new JsonSerializer();
            var configuration = (List<Config>)serializer.Deserialize(new StringReader(jsonString), typeof(List<Config>));
            return configuration;
        }
    }
}
