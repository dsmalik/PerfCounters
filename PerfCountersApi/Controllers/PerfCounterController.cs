using GetCounterInfoByAppPool;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace PerfCountersApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PerfCounterController : ControllerBase
    {
        // 1. Retrieve a list of all Performance Counter Categories
        [HttpGet("categories")]
        public ActionResult<IEnumerable<string>> GetAllCategories()
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
        [HttpGet("categories/{categoryName}/instances")]
        public ActionResult<IEnumerable<string>> GetInstances(string categoryName)
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
        [HttpGet("categories/{categoryName}/counters")]
        public ActionResult<IEnumerable<string>> GetCounters(string categoryName, [FromQuery] string instanceName = null)
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
        [HttpGet("categories/{categoryName}/counters/{counterName}")]
        public ActionResult<float> GetCounterValue(string categoryName, string counterName, [FromQuery] string instanceName = null)
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
        [HttpPost("counters/values")]
        public ActionResult<IEnumerable<CounterValueResponse>> GetMultipleCounterValues([FromBody] IEnumerable<CounterValueRequest> requests)
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
        [HttpPost("counters/values/over-time")]
        public async Task<ActionResult<IEnumerable<CounterValuesOverTimeResponse>>> GetCounterValuesOverTime([FromBody] CounterValuesOverTimeRequest request, CancellationToken cancellationToken)
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
        [HttpGet("categories/{categoryName}/counters/names")]
        public ActionResult<IEnumerable<string>> GetCounterNamesWithoutInstances(string categoryName)
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
        [HttpGet("app-pools")]
        public ActionResult<IEnumerable<AppPoolInfo>> GetAppPools()
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
    public class AppPoolInfo
    {
        public string CounterInstanceName { get; set; }
        public string AppPoolName { get; set; }
    }

    public class CounterValuesOverTimeRequest
    {
        public IEnumerable<CounterValueRequest> CounterRequests { get; set; }
        public int DurationInSeconds { get; set; }
    }

    public class CounterValuesOverTimeResponse
    {
        public string CategoryName { get; set; }
        public string CounterName { get; set; }
        public string? InstanceName { get; set; }
        public List<float> Values { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class CounterValueRequest
    {
        public string CategoryName { get; set; }
        public string CounterName { get; set; }
        public string? InstanceName { get; set; }
    }

    public class CounterValueResponse
    {
        public string CategoryName { get; set; }
        public string CounterName { get; set; }
        public string? InstanceName { get; set; }
        public float? Value { get; set; }
        public string ErrorMessage { get; set; }
    }
}
