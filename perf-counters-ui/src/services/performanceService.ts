import axios from "axios";

export const getPerformanceCategories = async (): Promise<string[]> => {
  const response = await axios.get<string[]>(
    "http://localhost:56139/api/perfcounter/categories/"
  );
  return response.data;
};
