import axios from "axios";

export const getCategoryInstances = async (
  category: string
): Promise<string[]> => {
  const response = await axios.get<string[]>(
    // `/api/perfcounter/categories/ASP.NET%20Apps%20v4.0.30319/instances`
    `http://localhost:22788/api/perfcounter/categories/${category}/instances`
  );
  return response.data;
};
