using PerfLib.Common.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System.Web.Http;
using PerfLib.Common.Services;
using System.Web.Http.Cors;

namespace PerfCountersApi.NetFx.Controllers
{
    [EnableCors("*", "*", "*")]
    public class PerfCounterController : ApiController
    {
        // 1. Retrieve a list of all Performance Counter Categories
        [HttpGet]
        [Route("api/perfcounter/categories")]
        // ActionResult<IEnumerable<string>>
        public IHttpActionResult GetAllCategories()
        {
            var categories = PerformanceCounterCategory.GetCategories();
            var categoryNames = new List<string>();

            foreach (var category in categories)
            {
                categoryNames.Add(category.CategoryName);
            }

            return Ok(categoryNames);
        }

        // 2. Retrieve list of instances for a specified performance counter category
        [HttpGet]
        [Route("api/perfcounter/categories/{categoryName}/instances")]
        // ActionResult<IEnumerable<string>>
        public IHttpActionResult GetInstances(string categoryName)
        {
            try
            {
                var category = new PerformanceCounterCategory(categoryName);
                var instances = category.GetInstanceNames();

                // Do some special handling for w3wp instances to map them to AppPoolName
                var appPoolNameByWorkerProcess = AppPoolInfoProvider.GetAppPoolNameByWorkerProcess();

                if (instances.Length > 0 && appPoolNameByWorkerProcess.Count > 0)
                {
                    var mappedInstances = new List<string>();

                    foreach (var instance in instances)
                    {
                        mappedInstances.Add(instance);
                    }

                    return Ok(mappedInstances);
                }

                return Ok(instances);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // 3. Retrieve list of counter names based on the specified counter category
        [HttpGet]
        [Route("api/perfcounter/categories/{categoryName}/counters")]
        // ActionResult<IEnumerable<string>>
        public IHttpActionResult GetCounters(string categoryName, [FromUri] string instanceName = null)
        {
            try
            {
                var category = new PerformanceCounterCategory(categoryName);
                var counterNames = new List<string>();

                if (category.CategoryType == PerformanceCounterCategoryType.MultiInstance)
                {
                    var instances = category.GetInstanceNames();

                    if (!string.IsNullOrEmpty(instanceName))
                    {
                        if (!instances.Contains(instanceName))
                        {
                            return BadRequest($"Instance name '{instanceName}' is not valid for category '{categoryName}'.");
                        }

                        var counters = category.GetCounters(instanceName);

                        foreach (var counter in counters)
                        {
                            counterNames.Add($@"\{categoryName}({instanceName})\{counter.CounterName}");
                        }
                    }
                    else
                    {
                        foreach (var instance in instances)
                        {
                            var counters = category.GetCounters(instance);

                            foreach (var counter in counters)
                            {
                                counterNames.Add($@"\{categoryName}({instance})\{counter.CounterName}");
                            }
                        }
                    }
                }
                else
                {
                    var counters = category.GetCounters();

                    foreach (var counter in counters)
                    {
                        counterNames.Add($@"\{categoryName}\{counter.CounterName}");
                    }
                }

                return Ok(counterNames);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // 4. Retrieve the value of a specific counter for a specific instance
        [HttpGet]
        [Route("api/perfcounter/categories/{categoryName}/counters/{counterName}")]
        public IHttpActionResult GetCounterValue(string categoryName, string counterName, [FromUri] string instanceName = null)
        {
            try
            {
                PerformanceCounter counter;

                if (!string.IsNullOrEmpty(instanceName))
                {
                    counter = new PerformanceCounter(categoryName, counterName, instanceName, readOnly: true);
                }
                else
                {
                    counter = new PerformanceCounter(categoryName, counterName, readOnly: true);
                }

                float value = counter.NextValue();
                return Ok(value);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // 5. Retrieve values for multiple counters in a single request
        [HttpPost]
        [Route("api/perfcounter/counters/values")]
        // ActionResult<IEnumerable<CounterValueResponse>>
        public IHttpActionResult GetMultipleCounterValues([FromBody] IEnumerable<CounterValueRequest> requests)
        {
            var responses = new List<CounterValueResponse>();

            foreach (var request in requests)
            {
                try
                {
                    PerformanceCounter counter;

                    if (!string.IsNullOrEmpty(request.InstanceName))
                    {
                        counter = new PerformanceCounter(request.CategoryName, request.CounterName, request.InstanceName, readOnly: true);
                    }
                    else
                    {
                        counter = new PerformanceCounter(request.CategoryName, request.CounterName, readOnly: true);
                    }

                    float value = counter.NextValue();
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
                        ErrorMessage = ex.Message
                    });
                }
            }

            return Ok(responses);
        }

        // 6. Retrieve a list of values for multiple counters over a period of time
        [HttpPost]
        [Route("api/perfcounter/counters/values/over-time")]
        // Task<ActionResult<IEnumerable<CounterValuesOverTimeResponse>>>
        public async Task<IHttpActionResult> GetCounterValuesOverTime([FromBody] CounterValuesOverTimeRequest request, CancellationToken cancellationToken)
        {
            var responses = new List<CounterValuesOverTimeResponse>();

            // Initialize the response objects for each counter request
            foreach (var counterRequest in request.CounterRequests)
            {
                responses.Add(new CounterValuesOverTimeResponse
                {
                    CategoryName = counterRequest.CategoryName,
                    CounterName = counterRequest.CounterName,
                    InstanceName = counterRequest.InstanceName,
                    Values = new List<float>() // Initialize as List<float>
                });
            }

            for (int i = 0; i < request.DurationInSeconds; i++)
            {
                foreach (var counterRequest in request.CounterRequests)
                {
                    try
                    {
                        PerformanceCounter counter;

                        if (!string.IsNullOrEmpty(counterRequest.InstanceName))
                        {
                            counter = new PerformanceCounter(counterRequest.CategoryName, counterRequest.CounterName, counterRequest.InstanceName, readOnly: true);
                        }
                        else
                        {
                            counter = new PerformanceCounter(counterRequest.CategoryName, counterRequest.CounterName, readOnly: true);
                        }

                        float value = counter.NextValue();

                        // Find the corresponding response object and add the value
                        var response = responses.First(r => r.CategoryName == counterRequest.CategoryName && r.CounterName == counterRequest.CounterName && r.InstanceName == counterRequest.InstanceName);
                        response.Values.Add(value);
                    }
                    catch (Exception ex)
                    {
                        // Find the corresponding response object and set the error message
                        var response = responses.First(r => r.CategoryName == counterRequest.CategoryName && r.CounterName == counterRequest.CounterName && r.InstanceName == counterRequest.InstanceName);
                        response.ErrorMessage = ex.Message;
                    }
                }

                await Task.Delay(1000, cancellationToken); // Wait for 1 second
            }

            return Ok(responses);
        }

        // 7. Retrieve list of counter names for a specified performance counter category without requiring instances
        [HttpGet]
        [Route("api/perfcounter/categories/{categoryName}/counters/names")]
        // ActionResult<IEnumerable<string>>
        public IHttpActionResult GetCounterNamesWithoutInstances(string categoryName)
        {
            try
            {
                var category = new PerformanceCounterCategory(categoryName);
                var counterNames = new List<string>();

                if (category.CategoryType == PerformanceCounterCategoryType.MultiInstance)
                {
                    var instances = category.GetInstanceNames();
                    if (instances.Length > 0)
                    {
                        var counters = category.GetCounters(instances[0]);
                        foreach (var counter in counters)
                        {
                            counterNames.Add(counter.CounterName);
                        }
                    }
                    else
                    {
                        // return BadRequest($"No instances found for multi-instance category '{categoryName}'.");
                        return Ok(counterNames);
                    }
                }
                else
                {
                    var counters = category.GetCounters();
                    foreach (var counter in counters)
                    {
                        counterNames.Add(counter.CounterName);
                    }
                }

                return Ok(counterNames);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // 8. Retrieve a list of Application Pools
        [HttpGet]
        [Route("api/perfcounter/app-pools")]
        //  IEnumerable<AppPoolInfo> 
        public IHttpActionResult GetAppPools()
        {
            try
            {
                var appPoolNameByWorkerProcess = AppPoolInfoProvider.GetAppPoolNameByWorkerProcess();
                var appPoolInfos = appPoolNameByWorkerProcess.Select(kvp => new AppPoolInfo
                {
                    CounterInstanceName = kvp.Key,
                    AppPoolName = kvp.Value
                }).ToList();

                return Ok(appPoolInfos);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
