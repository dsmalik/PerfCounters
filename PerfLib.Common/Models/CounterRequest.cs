using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerfLib.Common.Models
{

    public class CounterRequest
    {
        public IisAppInfo AppInfo { get; set; }

        public List<CounterRequestData> Counters { get; set; }
    }

    public class CounterRequestData
    {
        public string SelectedCategory { get; set; }
        public List<string> SelectedCounters { get; set; }
    }

    public class IisAppInfo
    {
        public long SiteId { get; set; }
        public string SiteName { get; set; }
        public string AppPoolName { get; set; }
        public string AppPath { get; set; }
        public string ProcessName { get; set; }
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
        public string InstanceName { get; set; }
        public List<float> Values { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class CounterValueRequest
    {
        public string CategoryName { get; set; }
        public string CounterName { get; set; }
        public string InstanceName { get; set; }
    }

    public class CounterValueResponse
    {
        public string CategoryName { get; set; }
        public string CounterName { get; set; }
        public string InstanceName { get; set; }
        public float? Value { get; set; }
        public string ErrorMessage { get; set; }
        public string SiteName { get; set; }
    }

}
