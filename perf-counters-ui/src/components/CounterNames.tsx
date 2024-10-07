import React from "react";
import Select from "react-select";

interface CounterNamesProps {
  counters: string[];
  selectedCounters: string[];
  onChange: (selected: string[]) => void;
}

const CounterNames: React.FC<CounterNamesProps> = ({
  counters,
  selectedCounters,
  onChange,
}) => {
  const options = counters.map((counter) => ({
    value: counter,
    label: counter,
  }));

  const handleChange = (selectedOptions: any) => {
    const selectedValues = selectedOptions
      ? selectedOptions.map((option: any) => option.value)
      : [];
    onChange(selectedValues);
  };

  return (
    <div>
      <label htmlFor="counter-select">Counter Names</label>
      <Select
        id="counter-select"
        options={options}
        isMulti
        value={options.filter((option) =>
          selectedCounters.includes(option.value)
        )}
        onChange={handleChange}
        placeholder="Select counters"
      />
    </div>
  );
};

export default CounterNames;
