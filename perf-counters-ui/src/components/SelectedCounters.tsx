import React from "react";

interface SelectedCountersProps {
  counters: string[];
}

const SelectedCounters: React.FC<SelectedCountersProps> = ({ counters }) => {
  return (
    <div className="mt-3">
      <h3>Selected Counters</h3>
      <ul className="list-group">
        {counters.map((counter, index) => (
          <li key={index} className="list-group-item">
            {counter}
          </li>
        ))}
      </ul>
    </div>
  );
};

export default SelectedCounters;
