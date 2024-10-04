import { useState } from "react";
import axios from "axios";

export const usePerformanceMonitor = () => {
  const [selectedCategory, setSelectedCategory] = useState<string>("");
  const [selectedInstance, setSelectedInstance] = useState<string>("");
  const [appendedCounters, setAppendedCounters] = useState<string[]>([]);
  const [counters, setCounters] = useState<string[]>([]);
  const [instances, setInstances] = useState<string[]>([]);

  const handleCategoryChange = (category: string) => {
    setSelectedCategory(category);
    resetInstanceAndCounters();
  };

  const handleInstanceChange = (instance: string) => {
    setSelectedInstance(instance);
  };

  const handleAddCounters = (counters: string[]) => {
    setAppendedCounters((prevCounters) => [...prevCounters, ...counters]);
  };

  const handleRetrieveValues = async () => {
    const payload = appendedCounters
      .map((counter) => {
        const regex = /\\(.+?)(?:\((.*?)\))?\\(.+)/;
        const match = counter.match(regex);
        if (match) {
          return {
            CategoryName: match[1],
            CounterName: match[3],
            InstanceName: match[2] || undefined,
          };
        }
        return null;
      })
      .filter(Boolean);

    try {
      const response = await axios.post(
        "http://localhost:22788/api/perfcounter/counters/values",
        payload,
        {
          headers: {
            "Content-Type": "application/json",
          },
        }
      );
      return response.data; // Return the retrieved values
    } catch (error) {
      console.error("Error retrieving counter values:", error);
      return [];
    }
  };

  const resetInstanceAndCounters = () => {
    setSelectedInstance("");
    setCounters([]);
    setInstances([]);
  };

  return {
    selectedCategory,
    selectedInstance,
    appendedCounters,
    counters,
    instances,
    handleCategoryChange,
    handleInstanceChange,
    handleAddCounters,
    handleRetrieveValues,
    resetInstanceAndCounters,
    setCounters,
    setInstances,
  };
};
