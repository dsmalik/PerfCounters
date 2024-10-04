import React, { useState, useEffect } from "react";
import CategoryDropdown from "./components/CategoryDropdown";
import InstanceDropdown from "./components/InstanceDropdown";
import CounterList from "./components/CounterList";
import SelectedCounters from "./components/SelectedCounters";
import CounterChart from "./components/CounterChart";
import { usePerformanceMonitor } from "./hooks/usePerformanceMonitor";
import * as d3 from "d3";

const App: React.FC = () => {
  const {
    selectedCategory,
    selectedInstance,
    appendedCounters,
    counters,
    instances,
    handleCategoryChange,
    handleInstanceChange,
    handleAddCounters,
    handleRetrieveValues,
    setCounters,
    setInstances,
  } = usePerformanceMonitor();

  const [filters, setFilters] = useState<string[]>([]);
  const [isFetching, setIsFetching] = useState<boolean>(true);
  const [graphData, setGraphData] = useState<any[]>([]);

  useEffect(() => {
    setFilters(appendedCounters);
  }, [appendedCounters]);

  const handleFilterChange = (counter: string) => {
    setFilters((prevFilters) =>
      prevFilters.includes(counter)
        ? prevFilters.filter((f) => f !== counter)
        : [...prevFilters, counter]
    );
  };

  const toggleFetching = () => {
    setIsFetching((prev) => !prev);
  };

  const clearGraphData = () => {
    setGraphData([]);
  };

  const color = d3.scaleOrdinal(d3.schemeCategory10);

  return (
    <div className="container">
      <h1>Performance Monitor</h1>
      <div
        className="dropdown-container"
        style={{
          display: "flex",
          gap: "10px",
          marginBottom: "20px",
          width: "100%",
          alignItems: "flex-start",
        }}
      >
        <div style={{ flex: 1 }}>
          <CategoryDropdown
            onCategoryChange={(category) => {
              handleCategoryChange(category);
            }}
          />
        </div>
        <div style={{ flex: 1 }}>
          <InstanceDropdown
            selectedCategory={selectedCategory}
            selectedInstance={selectedInstance}
            onInstanceChange={handleInstanceChange}
            instances={instances}
            setInstances={setInstances}
          />
        </div>
        <div style={{ flex: 1 }}>
          <CounterList
            selectedCategory={selectedCategory}
            selectedInstance={selectedInstance}
            onAddCounters={handleAddCounters}
            counters={counters}
            setCounters={setCounters}
          />
        </div>
      </div>
      <SelectedCounters counters={appendedCounters} />
      <div className="form-group">
        <label>Filter Counters</label>
        {appendedCounters.map((counter, index) => (
          <div
            key={counter}
            className="form-check"
            style={{ display: "flex", alignItems: "center" }}
          >
            <input
              type="checkbox"
              className="form-check-input"
              id={counter}
              checked={filters.includes(counter)}
              onChange={() => handleFilterChange(counter)}
            />
            <label
              className="form-check-label"
              htmlFor={counter}
              style={{ marginLeft: "8px" }}
            >
              <span
                style={{
                  backgroundColor: color(counter),
                  width: "12px",
                  height: "12px",
                  display: "inline-block",
                  marginRight: "8px",
                }}
              ></span>
              {counter}
            </label>
          </div>
        ))}
      </div>
      <div style={{ display: "flex", gap: "10px", marginBottom: "20px" }}>
        <button onClick={toggleFetching} className="btn btn-primary">
          {isFetching ? "Stop" : "Resume"}
        </button>
        <button onClick={clearGraphData} className="btn btn-secondary">
          Clear Graph
        </button>
      </div>
      <CounterChart
        selectedCategory={selectedCategory}
        selectedInstance={selectedInstance}
        appendedCounters={appendedCounters}
        filters={filters}
        isFetching={isFetching}
        graphData={graphData}
        setGraphData={setGraphData}
      />
    </div>
  );
};

export default App;
