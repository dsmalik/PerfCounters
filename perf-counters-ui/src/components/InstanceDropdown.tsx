import React, { useEffect, useState } from "react";
import Select from "react-select";
import { getCategoryInstances } from "../services/instanceService";

interface InstanceDropdownProps {
  selectedCategory: string;
  selectedInstance: string;
  onInstanceChange: (instance: string) => void;
  instances: string[];
  setInstances: (instances: string[]) => void;
}

const InstanceDropdown: React.FC<InstanceDropdownProps> = ({
  selectedCategory,
  selectedInstance,
  onInstanceChange,
  instances,
  setInstances,
}) => {
  useEffect(() => {
    const fetchInstances = async () => {
      if (selectedCategory) {
        const data = await getCategoryInstances(selectedCategory);
        const sortedData = data.sort();
        setInstances(sortedData);
      } else {
        setInstances([]);
      }
    };

    fetchInstances();
  }, [selectedCategory, setInstances]);

  const handleChange = (selectedOption: { value: string } | null) => {
    const instance = selectedOption ? selectedOption.value : "";
    onInstanceChange(instance);
  };

  const options = instances.map((instance) => ({
    value: instance,
    label: instance,
  }));

  return (
    <div className="form-group">
      <label htmlFor="instanceSelect">Instances</label>
      <Select
        id="instanceSelect"
        options={options}
        value={options.find((option) => option.value === selectedInstance)}
        onChange={handleChange}
        isClearable
        placeholder="Select an instance"
        isDisabled={instances.length === 0}
      />
    </div>
  );
};

export default InstanceDropdown;
