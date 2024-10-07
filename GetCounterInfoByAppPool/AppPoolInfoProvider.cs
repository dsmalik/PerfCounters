using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GetCounterInfoByAppPool
{
    public class AppPoolInfoProvider
    {
        public static Dictionary<string, string> GetAppPoolNameByWorkerProcess()
        {
            Dictionary<int, string> appPoolByProcessId = new Dictionary<int, string>();
            Dictionary<string, string> appPoolNameByWorkerProcess = new Dictionary<string, string>();

            try
            {
                // Define the performance counter category for W3SVC_W3WP
                PerformanceCounterCategory category = new PerformanceCounterCategory("W3SVC_W3WP");

                // Get all instances for the category
                string[] instances = category.GetInstanceNames();

                // Populate the appPoolByProcessId dictionary
                foreach (string instance in instances)
                {
                    if (instance != "__Total")
                    {
                        // Split the instance name to get process ID and app pool name
                        string[] parts = instance.Split('_');
                        if (parts.Length == 2 && int.TryParse(parts[0], out int processId))
                        {
                            appPoolByProcessId[processId] = parts[1];
                        }
                    }
                }

                // Define the performance counter category for Process
                PerformanceCounterCategory processCategory = new PerformanceCounterCategory("Process");

                // Get all instances for the Process category
                string[] processInstances = processCategory.GetInstanceNames();

                // Populate the appPoolNameByWorkerProcess dictionary
                foreach (string processInstance in processInstances)
                {
                    if (processInstance.StartsWith("w3wp"))
                    {
                        using (PerformanceCounter processIdCounter = new PerformanceCounter("Process", "ID Process", processInstance, true))
                        {
                            int processId = (int)processIdCounter.RawValue;

                            // Lookup the app pool name using the process ID
                            if (appPoolByProcessId.TryGetValue(processId, out string appPoolName))
                            {
                                appPoolNameByWorkerProcess[processInstance] = appPoolName;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            return appPoolNameByWorkerProcess;
        }
    }
}
