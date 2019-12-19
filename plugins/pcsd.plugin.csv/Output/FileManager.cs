using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using log4net;

namespace pcsd.plugin.csv.Output
{
    class FileManager
    {
        private static readonly ILog Trace = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static string _filePath;
        
        private static void CreateFile(string filePath)
        {
            var baseFileName = Path.GetFileNameWithoutExtension(filePath);
            var baseExtension = Path.GetExtension(filePath);
            var baseDir = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(baseDir)) { Directory.CreateDirectory(baseDir); }
            if (File.Exists(filePath))
            {
                var newName = $"{baseDir}\\{baseFileName} (renamed {DateTime.Now.ToString("yyyyMMddHHmmssfff")}){baseExtension}";
                File.Move(filePath, newName);
                Trace.Info($"File with this name did exist, renamed it to: {newName}");
            }
            var file = File.Create(filePath);
            file.Close();
            _filePath = filePath;
            Trace.Info($"Output file has been created: {_filePath}");
        }

        public static void SaveToFile(List<string> csvLines, string filePath)
        {
            if (string.IsNullOrWhiteSpace(_filePath)) CreateFile(filePath);
            File.AppendAllLines(_filePath, csvLines);
        }
    }
}