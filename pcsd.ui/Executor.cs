using System.Collections.Generic;
using System.Diagnostics;

namespace pcsd.ui
{
    class Executor
    {
        public static string GetArguments(Config config)
        {            
            var argumentList = new List<string>();
            if (!string.IsNullOrWhiteSpace(config.ClientId)) argumentList.Add($"/clientid=\"{config.ClientId}\"");
            if (!string.IsNullOrWhiteSpace(config.ClientSecret)) argumentList.Add($"/clientsecret=\"{config.ClientSecret}\"");
            if (!string.IsNullOrWhiteSpace(config.Environment)) argumentList.Add($"/environment=\"{config.Environment}\"");
            if (!string.IsNullOrWhiteSpace(config.TargetSql)) argumentList.Add($"/target-sql=\"{config.TargetSql}\"");
            if (!string.IsNullOrWhiteSpace(config.TargetCsv)) argumentList.Add($"/target-csv=\"{config.TargetCsv}\"");
            if (!string.IsNullOrWhiteSpace(config.Stats)) argumentList.Add($"/stats=\"{config.Stats}\"");
            if (config.StartDate != null) argumentList.Add($"/startdate=\"{config.StartDate?.ToString("yyyy-MM-dd HH:mm:ss")}\"");
            return string.Join(" ", argumentList);
        }

        public static void Execute(string arguments)
        {
            var p = new Process
            {
                StartInfo =
                {
                    FileName = "..\\pcsd.exe",
                    Arguments = arguments
                }
            };
            p.Start();
        }
    }
}
