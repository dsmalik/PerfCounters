import React, { useState, useEffect } from "react";
import CategoryDropdown from "./components/CategoryDropdown";
import CounterNames from "./components/CounterNames";
import { usePerformanceMonitor } from "./hooks/usePerformanceMonitor";
import axios from "axios";

const Settings: React.FC = () => {
  const {
    selectedCategory,
    appendedCounters,
    counters,
    handleCategoryChange,
    handleAddCounters,
    setCounters,
  } = usePerformanceMonitor();

  const [threading, setThreading] = useState(false);
  const [memory, setMemory] = useState(false);
  const [cpu, setCpu] = useState(false);
  const [requests, setRequests] = useState(false);
  const [filters, setFilters] = useState<string[]>([]);
  const [selectedCounters, setSelectedCounters] = useState<string[]>([]);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  useEffect(() => {
    setFilters(appendedCounters);
  }, [appendedCounters]);

  useEffect(() => {
    // Clear filters and selected counters when the selected category changes
    setFilters([]);
    setSelectedCounters([]);
  }, [selectedCategory]);

  const handleCheckboxChange = (
    setter: React.Dispatch<React.SetStateAction<boolean>>
  ) => {
    return (event: React.ChangeEvent<HTMLInputElement>) => {
      setter(event.target.checked);
    };
  };

  const handleCategoryChangeWithFetch = async (category: string) => {
    handleCategoryChange(category);
    if (category) {
      try {
        const response = await axios.get(
          `http://localhost:22788/api/perfcounter/categories/${category}/counters/names`
        );
        setCounters(response.data);
        setErrorMessage(null); // Clear the error message when a category is selected
        setSelectedCounters([]); // Clear selected counters when category changes
      } catch (error) {
        console.error("Error fetching counters:", error);
      }
    } else {
      setCounters([]);
    }
  };

  const handleAddSettings = () => {
    const newSetting = {
      selectedCategory,
      selectedCounters,
    };

    const addToLocalStorage = (key: string) => {
      const existingSettings = JSON.parse(localStorage.getItem(key) || "[]");
      existingSettings.push(newSetting);
      localStorage.setItem(key, JSON.stringify(existingSettings));
    };

    if (threading) {
      addToLocalStorage("threadingSettings");
    }
    if (memory) {
      addToLocalStorage("memorySettings");
    }
    if (cpu) {
      addToLocalStorage("cpuSettings");
    }
    if (requests) {
      addToLocalStorage("requestsSettings");
    }
  };

  return (
    <div className="container">
      <h1>Settings</h1>
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
          <CategoryDropdown onCategoryChange={handleCategoryChangeWithFetch} />
        </div>
        <div style={{ flex: 1 }}>
          <CounterNames
            counters={counters}
            selectedCounters={selectedCounters}
            onChange={setSelectedCounters}
          />
        </div>
      </div>
      <h2>Monitor Category</h2>
      <form>
        <div className="form-check">
          <input
            type="checkbox"
            className="form-check-input"
            id="threading"
            checked={threading}
            onChange={handleCheckboxChange(setThreading)}
          />
          <label className="form-check-label" htmlFor="threading">
            Threading performance
          </label>
        </div>
        <div className="form-check">
          <input
            type="checkbox"
            className="form-check-input"
            id="memory"
            checked={memory}
            onChange={handleCheckboxChange(setMemory)}
          />
          <label className="form-check-label" htmlFor="memory">
            Memory
          </label>
        </div>
        <div className="form-check">
          <input
            type="checkbox"
            className="form-check-input"
            id="cpu"
            checked={cpu}
            onChange={handleCheckboxChange(setCpu)}
          />
          <label className="form-check-label" htmlFor="cpu">
            CPU usage
          </label>
        </div>
        <div className="form-check">
          <input
            type="checkbox"
            className="form-check-input"
            id="requests"
            checked={requests}
            onChange={handleCheckboxChange(setRequests)}
          />
          <label className="form-check-label" htmlFor="requests">
            Request related metrics
          </label>
        </div>
      </form>
      <button onClick={handleAddSettings} className="btn btn-primary">
        Add
      </button>
      {errorMessage && <div className="alert alert-danger">{errorMessage}</div>}
    </div>
  );
};

export default Settings;
