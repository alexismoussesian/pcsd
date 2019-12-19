using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;
using Newtonsoft.Json;

namespace pcsd.plugin.csv.Mapping
{
    class MappingManager
    {
        private const string MappingFileName = "pcsd.plugin.csv.mapping.json";
        private static readonly ILog Trace = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly string AssemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        
        private static readonly string MappingFilePath = $"{AssemblyDir}\\{MappingFileName}";

        public static List<Item> MappingItemList { get; set; }
    
        public static void Initialize()
        {
            try
            {
                Trace.Debug($"Loading mapping file: {MappingFilePath}");
                var mappingFile = File.ReadAllText(MappingFilePath);
                Trace.Debug("Deserializing mapping file");
                MappingItemList = JsonConvert.DeserializeObject<List<Item>>(mappingFile);
            }
            catch (Exception ex)
            {
                Trace.Error("", ex);
            }
        }

        public static List<Item> GetMappingItemSublist(string parent) => MappingItemList.Where(x => x.Parent == parent).ToList();        
    }
}
