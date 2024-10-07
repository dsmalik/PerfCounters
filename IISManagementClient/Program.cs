using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IISManagementClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
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
                        Console.WriteLine($"\tApplication Path: {app.Path}");

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
    }
}
