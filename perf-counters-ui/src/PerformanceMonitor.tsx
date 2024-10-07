import React, { useState, useEffect } from "react";
import CategoryDropdown from "./components/CategoryDropdown";
import InstanceDropdown from "./components/InstanceDropdown";
import CounterList from "./components/CounterList";
import SelectedCounters from "./components/SelectedCounters";
import CounterChart from "./components/CounterChart";
import { usePerformanceMonitor } from "./hooks/usePerformanceMonitor";
import * as d3 from "d3";

const PerformanceMonitor: React.FC = () => {
  const {
    selectedCategory,
    selectedInstance,
    appendedCounters,
    counters,
    instances,
    handleCategoryChange,
    handleInstanceChange,
    handleAddCounters,
    setCounters,
    setInstances,
    setSelectedInstance,
  } = usePerformanceMonitor();

  const [filters, setFilters] = useState<string[]>([]);
  const [isFetching, setIsFetching] = useState<boolean>(true);
  const [graphData, setGraphData] = useState<any[]>([]);

  useEffect(() => {
    setFilters(appendedCounters);
  }, [appendedCounters]);

  useEffect(() => {
    // Clear filters and selected instance when the selected category changes
    setFilters([]);
    setSelectedInstance(""); // Reset selected instance
  }, [selectedCategory, setSelectedInstance]);

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

  const saveFilterState = () => {
    const filterState = appendedCounters.map((counter) => ({
      counter,
      selected: filters.includes(counter),
    }));
    sessionStorage.setItem("filterState", JSON.stringify(filterState));
  };

  const restoreFilterState = () => {
    const filterState = sessionStorage.getItem("filterState");
    if (filterState) {
      const parsedState = JSON.parse(filterState);
      const restoredCounters = parsedState.map((item: any) => item.counter);
      const restoredFilters = parsedState
        .filter((item: any) => item.selected)
        .map((item: any) => item.counter);
      handleAddCounters(restoredCounters);
      setFilters(restoredFilters);
    }
  };

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
      <SelectedCounters
        counters={appendedCounters}
        filters={filters}
        onFilterChange={handleFilterChange}
      />
      <div style={{ display: "flex", gap: "10px", marginBottom: "20px" }}>
        <button onClick={toggleFetching} className="btn btn-primary">
          {isFetching ? "Stop" : "Resume"}
        </button>
        <button onClick={clearGraphData} className="btn btn-secondary">
          Clear Graph
        </button>
        <button onClick={saveFilterState} className="btn btn-secondary">
          Save Filter State
        </button>
        <button onClick={restoreFilterState} className="btn btn-secondary">
          Restore Filter State
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

export default PerformanceMonitor;
