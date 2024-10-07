import React from "react";

interface SelectedCountersProps {
  counters: string[];
  filters: string[];
  onFilterChange: (counter: string) => void;
}

const SelectedCounters: React.FC<SelectedCountersProps> = ({
  counters,
  filters,
  onFilterChange,
}) => {
  return (
    <div className="selected-counters">
      <h3>Selected Counters</h3>
      {counters.map((counter) => (
        <div key={counter} className="form-check">
          <input
            type="checkbox"
            className="form-check-input"
            id={counter}
            checked={filters.includes(counter)}
            onChange={() => onFilterChange(counter)}
          />
          <label className="form-check-label" htmlFor={counter}>
            {counter}
          </label>
        </div>
      ))}
    </div>
  );
};

export default SelectedCounters;
