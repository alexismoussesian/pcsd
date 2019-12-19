using System.IO;

namespace UnitTest.pcsd.connection.Helpers
{
   internal static  class FileHelper
    {
       internal static string GetPluginsFolder()
       {
            var parent = Directory.GetParent(Directory.GetCurrentDirectory()).Parent;
            var directoryInfo = parent?.Parent;
            if (directoryInfo != null)
            {
                return directoryInfo.FullName + @"\pcsd\bin\Debug\Plugins";
            }
           return "";
       }
    }
}
