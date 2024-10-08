import React, { useState, useEffect } from "react";
import Select from "react-select";
import "bootstrap/dist/css/bootstrap.min.css";
import SimpleCounterChart from "../SimpleCounterChart"; // Adjust the import path as necessary

interface AspNetAppInfo {
  siteId: number;
  siteName: string;
  appPoolName: string;
  appPath: string;
}

interface ChartConfig {
  id: number;
  label: string;
  counters: string[];
  isFetching: boolean;
}

const AspNetPerformanceMonitor = () => {
  const [apps, setApps] = useState<Array<{ label: string; value: string }>>([]);
  const [selectedApp, setSelectedApp] = useState<{
    label: string;
    value: string;
  } | null>(null);
  const [charts, setCharts] = useState<ChartConfig[]>([]);
  const [chartId, setChartId] = useState(0);
  const [collapseStates, setCollapseStates] = useState<{
    [key: number]: boolean;
  }>({});

  useEffect(() => {
    fetch("http://localhost:22788/api/aspnetapps/iis-apps")
      .then((response) => response.json())
      .then((data: Array<AspNetAppInfo>) => {
        const formattedData = data.map((app) => {
          var parsedAppPath = app.appPath === "" ? "ROOT" : "" + app.appPath;
          var parsedAppPathValue =
            app.appPath === "" ? "ROOT" : "ROOT_" + app.appPath;
          return {
            label: `Site: ${app.siteName} App: ${parsedAppPath}`,
            value: `_LM_W3SVC_${app.siteId}_${parsedAppPathValue}`,
          };
        });
        setApps(formattedData);
      })
      .catch((error) => console.error("Error fetching data:", error));
  }, []);

  const mapSettingsToCounters = (settings: any[], appPoolValue: string) => {
    return settings.flatMap((setting) =>
      setting.selectedCounters.map(
        (counter: string) =>
          `\\${setting.selectedCategory}(${appPoolValue})\\${counter}`
      )
    );
  };

  const handleButtonClick = () => {
    if (!selectedApp) {
      console.error("No app selected");
      return;
    }

    const requestData = localStorage.getItem("requestsSettings");
    console.log("Request Data:", requestData);
    if (requestData) {
      const parsedData = JSON.parse(requestData);
      const counters = mapSettingsToCounters(parsedData, selectedApp.value);
      const newChart: ChartConfig = {
        id: chartId,
        label: selectedApp.label,
        counters: counters || [],
        isFetching: true,
      };
      setCharts([...charts, newChart]);
      setCollapseStates({ ...collapseStates, [chartId]: true });
      setChartId(chartId + 1);
    }
  };

  const toggleCollapse = (id: number) => {
    setCollapseStates({ ...collapseStates, [id]: !collapseStates[id] });
  };

  const toggleFetching = (id: number) => {
    setCharts((prevCharts) =>
      prevCharts.map((chart) =>
        chart.id === id ? { ...chart, isFetching: !chart.isFetching } : chart
      )
    );
  };

  const clearChartData = (id: number) => {
    setCharts((prevCharts) =>
      prevCharts.map((chart) =>
        chart.id === id ? { ...chart, counters: [] } : chart
      )
    );
  };

  return (
    <div className="container">
      <h1>ASP.NET Performance Monitor</h1>
      <div className="d-flex align-items-center">
        <div
          className="form-group"
          style={{ width: "30%", marginRight: "10px" }}
        >
          <Select
            options={apps}
            onChange={(selectedOption) => setSelectedApp(selectedOption)}
          />
        </div>
        <button className="btn btn-primary" onClick={handleButtonClick}>
          Add Chart
        </button>
      </div>
      {charts.map((chart) => (
        <div key={chart.id} className="my-3">
          <button
            className="btn btn-primary"
            type="button"
            onClick={() => toggleCollapse(chart.id)}
            aria-expanded={collapseStates[chart.id]}
            aria-controls={`collapse${chart.id}`}
            style={{ marginBottom: "1rem" }}
          >
            {chart.label}
          </button>
          <div
            className={`collapse ${collapseStates[chart.id] ? "show" : ""}`}
            id={`collapse${chart.id}`}
          >
            <div className="card card-body">
              <SimpleCounterChart
                counters={chart.counters}
                isFetching={chart.isFetching}
              />
              <button
                className="btn btn-secondary mt-2"
                onClick={() => toggleFetching(chart.id)}
              >
                {chart.isFetching ? "Stop Fetching" : "Start Fetching"}
              </button>
              <button
                className="btn btn-danger mt-2"
                onClick={() => clearChartData(chart.id)}
              >
                Clear Data
              </button>
            </div>
          </div>
        </div>
      ))}
    </div>
  );
};

export default AspNetPerformanceMonitor;
