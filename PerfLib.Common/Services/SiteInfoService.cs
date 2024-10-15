using Microsoft.Web.Administration;
using PerfLib.Common.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerfLib.Common.Services
{
    public class SiteInfoService
    {
        public static List<IisAppInfo> GetSiteInfoFor(string path, string processName)
        {
            // Path to the applicationHost.config file for full IIS
            string applicationHostConfigPath = path;
            var serverManager = new ServerManager(applicationHostConfigPath);
            var iisApps = new List<IisAppInfo>();

            foreach (var site in serverManager.Sites)
            {
                foreach (var app in site.Applications)
                {
                    iisApps.Add(new IisAppInfo
                    {
                        SiteId = site.Id,
                        SiteName = site.Name,
                        AppPoolName = app.ApplicationPoolName,
                        AppPath = app.Path.TrimEnd('/').Replace("/", ""),
                        ProcessName = processName
                    });
                }
            }

            return iisApps;
        }


        public static List<IisAppInfo> GetIisExpressSites()
        {
            // Get the list of applications for all iisexpress processes
            var iisexpresses = PerformanceCountersHelpers.GetProcessInfoForIisExpress();

            var iisExpressSites = new List<IisAppInfo>();
            // Get the list of applications using the default iis config
            foreach (var iisExpressConfigPath in iisexpresses.Values)
            {
                var sites = GetSiteInfoFor(iisExpressConfigPath, "iisexpress");

                foreach (var site in sites)
                {
                    iisExpressSites.Add(new IisAppInfo
                    {
                        SiteId = site.SiteId,
                        SiteName = site.SiteName,
                        AppPoolName = site.AppPoolName,
                        AppPath = site.AppPath.TrimEnd('/').Replace("/", ""),
                        ProcessName = site.ProcessName
                    });

                }
            }

            return iisExpressSites;
        }

        public static List<IisAppInfo> GetIisSites()
        {
            return GetSiteInfoFor(@"C:\Windows\System32\inetsrv\config\applicationHost.config", "w3wp");
        }
    }
}
