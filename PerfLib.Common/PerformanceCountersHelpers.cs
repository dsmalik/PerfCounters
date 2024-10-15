using PerfLib.Common.Models;
using PerfLib.Common.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace PerfLib.Common
{
    public class PerformanceCountersHelpers
    {
        private List<CounterValueResponse> GetCountersForApplication(CounterRequest counterRequest)
        {
            var appInfo = counterRequest.AppInfo;

            string instanceName = $"_LM_W3SVC_{appInfo.SiteId}_ROOT{(appInfo.AppPath.Length > 1 ? "_" + appInfo.AppPath.Substring(1) : "")}";

            var getProcessId = AppPoolInfoProvider.GetProcessIdByAppPoolName().Where(x => x.Key == appInfo.AppPoolName).Select(x => x.Value).FirstOrDefault();

            var odpInstance = $"{instanceName.ToLowerInvariant()}-1...[{getProcessId},2]";

            var requests = new HashSet<CounterValueRequest>();

            foreach (var item in counterRequest.Counters)
            {
                foreach (var counter in item.SelectedCounters)
                {
                    requests.Add(new CounterValueRequest
                    {
                        CategoryName = item.SelectedCategory,
                        CounterName = counter,
                        InstanceName = GetInstanceNameFor(item, instanceName, getProcessId)
                    });
                }
            }

            var responses = new List<CounterValueResponse>();

            foreach (var request in requests)
            {
                try
                {
                    var counter = new PerformanceCounter(request.CategoryName, request.CounterName, request.InstanceName, true);

                    var value = counter.NextValue();

                    responses.Add(new CounterValueResponse
                    {
                        CategoryName = request.CategoryName,
                        CounterName = request.CounterName,
                        InstanceName = request.InstanceName,
                        Value = value
                    });
                }
                catch (Exception ex)
                {
                    responses.Add(new CounterValueResponse
                    {
                        CategoryName = request.CategoryName,
                        CounterName = request.CounterName,
                        InstanceName = request.InstanceName,
                        ErrorMessage = ex.Message,
                        Value = 0
                    });
                }
            }

            return responses;
        }


        private string GetInstanceNameFor(CounterRequestData item, string instanceName, int getProcessId)
        {
            if (item.SelectedCategory.StartsWith("ASP"))
            {
                return instanceName;
            }

            if (item.SelectedCategory.Contains("ODP"))
            {
                return $"{instanceName.ToLowerInvariant()}-1...[{getProcessId},2]";
            }

            return GetWorkerInstanceNameByProcessId(getProcessId);
        }

        private string GetWorkerInstanceNameByProcessId(int getProcessId)
        {
            var categoryName = "Process";
            var counterName = "";

            var category = new PerformanceCounterCategory(categoryName);

            var instances = category.GetInstanceNames().Where(x => x.StartsWith("w3wp"));

            foreach (var instanceName in instances)
            {
                PerformanceCounter counter;

                if (!string.IsNullOrEmpty(instanceName))
                {
                    counter = new PerformanceCounter(categoryName, counterName, instanceName, true);
                }
                else
                {
                    counter = new PerformanceCounter(categoryName, counterName);
                }

                var counterValue = counter.NextValue();
                Console.WriteLine($"{instanceName} - {counterValue}");

                if ((int)counterValue == getProcessId)
                {
                    return instanceName;
                }
            }

            return "w3wp";
        }

        public static Dictionary<(int, string), string> GetProcessInfoForIisExpress()
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
