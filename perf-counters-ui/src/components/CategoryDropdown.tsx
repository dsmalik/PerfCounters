import React, { useEffect, useState } from "react";
import Select from "react-select";
import { getPerformanceCategories } from "../services/performanceService";

interface CategoryDropdownProps {
  onCategoryChange: (category: string) => void;
}

const CategoryDropdown: React.FC<CategoryDropdownProps> = ({
  onCategoryChange,
}) => {
  const [categories, setCategories] = useState<string[]>([]);
  const [selectedCategory, setSelectedCategory] = useState<string | null>(null);

  useEffect(() => {
    const fetchCategories = async () => {
      const data = await getPerformanceCategories();
      const filteredData = data
        .filter(
          (category) =>
            category.includes("ASP.NET") ||
            category.includes("Web Service") ||
            category.includes("Process") ||
            category.includes(".NET") ||
            category.includes("TCP")
        )
        .sort();
      setCategories(filteredData);
    };

    fetchCategories();
  }, []);

  const handleChange = (selectedOption: { value: string } | null) => {
    const category = selectedOption ? selectedOption.value : "";
    setSelectedCategory(category);
    onCategoryChange(category);
  };

  const options = categories.map((category) => ({
    value: category,
    label: category,
  }));

  return (
    <div className="form-group">
      <label htmlFor="categorySelect">Select Performance Category</label>
      <Select
        id="categorySelect"
        options={options}
        value={options.find((option) => option.value === selectedCategory)}
        onChange={handleChange}
        isClearable
        placeholder="Select a category"
      />
    </div>
  );
};

export default CategoryDropdown;
