using System;
using System.Collections.Generic;

namespace GetCounterInfoByAppPool
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Dictionary<string, string> appPoolNameByWorkerProcess = AppPoolInfoProvider.GetAppPoolNameByWorkerProcess();

            // Display the final mapping of process instances to app pool names
            Console.WriteLine("\nMapping of process instances to app pool names:");
            foreach (var kvp in appPoolNameByWorkerProcess)
            {
                Console.WriteLine($"Process Instance: {kvp.Key}, App Pool Name: {kvp.Value}");
            }
        }
    }
}
