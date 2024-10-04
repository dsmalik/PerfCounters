import axios from "axios";

export const getPerformanceCategories = async (): Promise<string[]> => {
  const response = await axios.get<string[]>(
    "http://localhost:22788/api/perfcounter/categories/"
  );
  return response.data;
};
