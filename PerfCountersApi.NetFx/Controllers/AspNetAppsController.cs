using Microsoft.Web.Administration;
using PerfLib.Common;
using PerfLib.Common.Models;
using PerfLib.Common.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.PerformanceData;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Web.Http;
using System.Web.Http.Cors;

namespace PerfCountersApi.NetFx.Controllers
{
    [EnableCors("*", "*", "*")]
    public class AspNetAppsController : ApiController
    {
        // Endpoint to retrieve information about applications hosted in IIS
        [HttpGet]
        [Route("api/aspnetapps/iis-apps")]
        // IEnumerable<IisAppInfo>
        public IHttpActionResult GetIisApps([FromUri] string path = @"C:\Windows\System32\inetsrv\config\applicationHost.config")
        {
            try
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
                            ProcessName = "w3wp"
                        });
                    }
                }

                return Ok(iisApps);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("api/aspnetapps/aspnet-apps")]
        // IEnumerable<IisAppInfo>
        public IHttpActionResult GetAllAspNetApps()
        {
            try
            {
                var responses = new List<IisAppInfo>();
                var iisExpressSites = SiteInfoService.GetIisExpressSites();

                responses.AddRange(iisExpressSites);


                var w3wpSites = SiteInfoService.GetIisSites();
                responses.AddRange(w3wpSites);

                return Ok(responses);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("api/aspnetapps/performance-counters")]
        // IEnumerable<CounterValueResponse>
        public IHttpActionResult GetPerformanceCounters([FromBody] CounterRequest counterRequest)
        {
            try
            {
                List<CounterValueResponse> responses = GetCountersForApplication(counterRequest);

                return Ok(responses);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private List<CounterValueResponse> GetCountersForApplication(CounterRequest counterRequest)
        {
            var appInfo = counterRequest.AppInfo;
            var appName = appInfo.AppPath.Length > 1 ? "_" + appInfo.AppPath.Substring(1) : "";
            string instanceName = $"_LM_W3SVC_{appInfo.SiteId}_ROOT{appName}";

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
                        Value = value,
                        SiteName = $"{counterRequest.AppInfo.ProcessName}_{counterRequest.AppInfo.ProcessName}{(appName.Length > 1 ? $"{appName}" : "")}"
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
                        Value = 0,
                        SiteName = $"{counterRequest.AppInfo.ProcessName}_{counterRequest.AppInfo.SiteName}{(appName.Length > 1 ? $"{appName}" : "")}"
                    });
                }
            }

            return responses;
        }

        [HttpPost]
        [Route("api/aspnetapps/aspnet-counters")]
        public IHttpActionResult GetAllAspNetPerformanceCounters([FromBody] List<CounterRequestData> counters)
        {
            try
            {
                var responses = new List<CounterValueResponse>();

                var iisExpressSites = SiteInfoService.GetIisExpressSites();

                foreach (var site in iisExpressSites)
                {
                    // Make the CounterRequest here and then populate the Complete response to return
                    var counterRequest = new CounterRequest
                    {
                        AppInfo = site,
                        Counters = counters
                    };

                    responses.AddRange(GetCountersForApplication(counterRequest));
                }

                var w3wpSites = SiteInfoService.GetIisSites();

                foreach (var site in w3wpSites)
                {
                    // Make the CounterRequest here and then populate the Complete response to return
                    var counterRequest = new CounterRequest
                    {
                        AppInfo = site,
                        Counters = counters
                    };

                    responses.AddRange(GetCountersForApplication(counterRequest));
                }



                return Ok(responses);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
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
    }
}
