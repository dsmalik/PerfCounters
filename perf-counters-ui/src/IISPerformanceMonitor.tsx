import React, { useState, useEffect } from "react";
import axios from "axios";
import CounterChart from "./components/CounterChart";
import "./IISPerformanceMonitor.css"; // Assuming you add the CSS styles in this file
import SimpleCounterChart from "./components/SimpleCounterChart";

const IISPerformanceMonitor: React.FC = () => {
  const [appPools, setAppPools] = useState<
    { value: string; label: string; checked?: boolean }[]
  >([]);
  const [threadingSettings, setThreadingSettings] = useState<any[]>([]);
  const [memorySettings, setMemorySettings] = useState<any[]>([]);
  const [cpuSettings, setCpuSettings] = useState<any[]>([]);
  const [requestsSettings, setRequestsSettings] = useState<any[]>([]);
  const [graphData, setGraphData] = useState<any[]>([]);
  const [isFetching, setIsFetching] = useState(false);

  useEffect(() => {
    const fetchAppPools = async () => {
      try {
        const response = await axios.get(
          "http://localhost:22788/api/perfcounter/app-pools"
        );
        const appPoolsData = response.data.map(
          (appPool: { counterInstanceName: string; appPoolName: string }) => ({
            value: appPool.counterInstanceName,
            label: appPool.appPoolName,
          })
        );
        setAppPools(appPoolsData);
      } catch (error) {
        console.error("Error fetching application pools:", error);
      }
    };

    fetchAppPools();
  }, []);

  useEffect(() => {
    const loadSettings = (
      key: string,
      setter: React.Dispatch<React.SetStateAction<any[]>>
    ) => {
      const settings = JSON.parse(localStorage.getItem(key) || "[]");
      setter(settings);
    };

    loadSettings("threadingSettings", setThreadingSettings);
    loadSettings("memorySettings", setMemorySettings);
    loadSettings("cpuSettings", setCpuSettings);
    loadSettings("requestsSettings", setRequestsSettings);
  }, []);

  const handleCheckboxChange = (index: number) => {
    const newAppPools = [...appPools];
    newAppPools[index].checked = !newAppPools[index].checked;
    setAppPools(newAppPools);
  };

  const getSelectedInstances = () => {
    return appPools.filter((appPool) => appPool.checked);
  };

  const mapSettingsToCounters = (settings: any[], appPoolValue: string) => {
    return settings.flatMap((setting) =>
      setting.selectedCounters.map(
        (counter: string) =>
          `\\${setting.selectedCategory}(${appPoolValue})\\${counter}`
      )
    );
  };

  const selectedInstances = getSelectedInstances();

  console.log("Selected Instances:", selectedInstances);
  return (
    <div className="container">
      <h1>IIS Performance Monitor</h1>
      <p>
        Details related to performance counters of IIS worker processes will be
        displayed here.
      </p>
      <div className="form-group">
        <label>Application Pools</label>
        <ul className="list-group list-group-horizontal">
          {appPools.map((appPool, index) => (
            <li key={appPool.value} className="list-group-item">
              <input
                type="checkbox"
                checked={appPool.checked || false}
                onChange={() => handleCheckboxChange(index)}
              />
              {appPool.label}
            </li>
          ))}
        </ul>
      </div>
      <button
        onClick={() => setIsFetching(!isFetching)}
        className="btn btn-primary"
      >
        {isFetching ? "Stop Fetching" : "Start Fetching"}
      </button>
      {selectedInstances.length === 0 && (
        <p>Please select an application pool to view performance data.</p>
      )}
      {selectedInstances.length > 0 && (
        <div className="charts">
          {selectedInstances.map((instance) => (
            <div key={instance.value}>
              <h2>
                Application Pool: {instance.label} {instance.value}
              </h2>
              <SimpleCounterChart
                counters={mapSettingsToCounters(
                  threadingSettings,
                  instance.value
                )}
                isFetching={isFetching}
              />
              {/* <CounterChart
                selectedCategory="Memory"
                selectedInstance={instance.value}
                appendedCounters={mapSettingsToCounters(
                  memorySettings,
                  instance.value
                )}
                filters={mapSettingsToCounters(memorySettings, instance.value)}
                isFetching={isFetching}
                graphData={graphData}
                setGraphData={setGraphData}
              />
              <CounterChart
                selectedCategory="CPU"
                selectedInstance={instance.value}
                appendedCounters={mapSettingsToCounters(
                  cpuSettings,
                  instance.value
                )}
                filters={mapSettingsToCounters(cpuSettings, instance.value)}
                isFetching={isFetching}
                graphData={graphData}
                setGraphData={setGraphData}
              /> */}
              {/* <SimpleCounterChart
                counters={mapSettingsToCounters(
                  requestsSettings,
                  instance.value
                )}
                isFetching={isFetching}
              /> */}
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default IISPerformanceMonitor;
