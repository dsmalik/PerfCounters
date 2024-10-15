using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace IISManagementClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var processInfo = GetProcessCommandLines();
            foreach (var entry in processInfo)
            {
                Console.WriteLine($"Process ID: {entry.Key.Item1}, Process Name: {entry.Key.Item2}, Command Line: {entry.Value}");
            }

            Console.WriteLine(); Console.WriteLine();

            // Create an instance of ServerManager to manage IIS
            using (ServerManager serverManager = new ServerManager())
            {
                // Iterate through each site in IIS
                foreach (Site site in serverManager.Sites)
                {
                    Console.WriteLine($"Site Name: {site.Name}, Site ID: W3SVC{site.Id}");

                    // Iterate through each application in the site
                    foreach (Application app in site.Applications)
                    {
                        Console.WriteLine($"\tApplication Path: {app.Path}, PoolName: {app.ApplicationPoolName}, App Counter: _LM_W3SVC_{site.Id}_ROOT{(app.Path.Length > 1 ? "_" + app.Path.Substring(1) : "")}");

                        //// Iterate through each virtual directory in the application
                        //foreach (VirtualDirectory vdir in app.VirtualDirectories)
                        //{
                        //    Console.WriteLine($"\t\tVirtual Directory: {vdir.Path}, Physical Path: {vdir.PhysicalPath}");
                        //}
                    }

                    Console.WriteLine();
                }
            }
        }

        private static Dictionary<(int, string), string> GetProcessCommandLines()
        {
            var processCommandLines = new Dictionary<(int, string), string>();

            var searcher = new ManagementObjectSearcher("SELECT ProcessId, Name, CommandLine FROM Win32_Process");
            foreach (ManagementObject obj in searcher.Get())
            {
                var processId = Convert.ToInt32(obj["ProcessId"]);
                var processName = obj["Name"].ToString();
                var commandLine = obj["CommandLine"]?.ToString() ?? string.Empty;

                if (processName.Equals("iisexpress.exe", StringComparison.OrdinalIgnoreCase))
                {
                    processCommandLines[(processId, processName)] = GetConfigPath(commandLine);
                }
            }

            return processCommandLines;
        }

        private static string GetConfigPath(string commandLine)
        {
            var configStringToMatch = @"/config:";
            var configPathRaw = commandLine.Split(' ').Where(x => x.StartsWith(configStringToMatch)).FirstOrDefault();

            if (!string.IsNullOrEmpty(configPathRaw))
            {
                return new FileInfo(configPathRaw.Substring(configStringToMatch.Length + 1, configPathRaw.Length - configStringToMatch.Length - 2)).FullName;
            }

            return "";
        }
    }
}
