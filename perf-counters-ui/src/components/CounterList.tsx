import React, { useEffect, useState } from "react";
import Select, { MultiValue } from "react-select";
import axios from "axios";

interface CounterListProps {
  selectedCategory: string;
  selectedInstance: string;
  onAddCounters: (counters: string[]) => void;
  counters: string[];
  setCounters: (counters: string[]) => void;
}

const CounterList: React.FC<CounterListProps> = ({
  selectedCategory,
  selectedInstance,
  onAddCounters,
  counters,
  setCounters,
}) => {
  const [selectedCounters, setSelectedCounters] = useState<
    { value: string; label: string }[]
  >([]);

  useEffect(() => {
    const fetchCounters = async () => {
      if (selectedCategory && (selectedInstance || selectedInstance === "")) {
        let url = `http://localhost:22788/api/perfcounter/categories/${encodeURIComponent(
          selectedCategory
        )}/counters`;
        if (selectedInstance) {
          url += `?instanceName=${encodeURIComponent(selectedInstance)}`;
        }
        const response = await axios.get<string[]>(url);
        setCounters(response.data);
      } else {
        setCounters([]);
      }
    };

    fetchCounters();
  }, [selectedCategory, selectedInstance, setCounters]);

  useEffect(() => {
    // Clear selected counters when the selected category or instance changes
    setSelectedCounters([]);
  }, [selectedCategory, selectedInstance]);

  const handleCounterChange = (
    selectedOptions: MultiValue<{ value: string; label: string }>
  ) => {
    setSelectedCounters(selectedOptions as { value: string; label: string }[]);
  };

  const handleAddCounters = () => {
    onAddCounters(selectedCounters.map((option) => option.value));
  };

  const options = counters.map((counter) => ({
    value: counter,
    label: counter,
  }));

  return (
    <div>
      <label htmlFor="counterSelect">Counters</label>
      <Select
        id="counterSelect"
        isMulti
        options={options}
        value={selectedCounters}
        onChange={handleCounterChange}
        isClearable
        placeholder="Select counters"
        isDisabled={counters.length === 0}
      />
      <button className="btn btn-primary mt-2" onClick={handleAddCounters}>
        Add Counters
      </button>
    </div>
  );
};

export default CounterList;
