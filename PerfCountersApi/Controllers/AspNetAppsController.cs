using Microsoft.AspNetCore.Mvc;
using Microsoft.Web.Administration;
using System.Collections.Generic;
using System.Linq;

namespace PerfCountersApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AspNetAppsController : ControllerBase
    {
        // Endpoint to retrieve information about applications hosted in IIS
        [HttpGet("iis-apps")]
        public ActionResult<IEnumerable<IisAppInfo>> GetIisApps()
        {
            try
            {
                // Path to the applicationHost.config file for full IIS
                string applicationHostConfigPath = @"C:\Windows\System32\inetsrv\config\applicationHost.config";
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
                            AppPath = app.Path.TrimEnd('/').Replace("/", "")
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
    }

    public class IisAppInfo
    {
        public long SiteId { get; set; }
        public string SiteName { get; set; }
        public string AppPoolName { get; set; }
        public string AppPath { get; set; }
    }
}