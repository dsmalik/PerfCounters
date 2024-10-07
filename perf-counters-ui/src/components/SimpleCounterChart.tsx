import React, { useEffect, useRef, useState } from "react";
import * as d3 from "d3";
import axios from "axios";

interface SimpleCounterChartProps {
  counters: string[];
  isFetching: boolean;
}

interface CounterData {
  timestamp: Date;
  name: string;
  value: number;
}

const SimpleCounterChart: React.FC<SimpleCounterChartProps> = ({
  counters,
  isFetching,
}) => {
  console.log("SimpleCounterChart Props:", {
    counters,
    isFetching,
  });

  const [graphData, setGraphData] = useState<any[]>([]);

  const chartRef = useRef<SVGSVGElement | null>(null);

  useEffect(() => {
    if (counters.length === 0 || !isFetching) return;

    const fetchData = async () => {
      const payload = counters
        .map((counter) => {
          const regex = /\\(.+?)(?:\((.*?)\))?\\(.+)/;
          const match = counter.match(regex);
          if (match) {
            return {
              categoryName: match[1],
              counterName: match[3],
              instanceName: match[2] || undefined,
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
        const timestamp = new Date();
        const newData = response.data.map((d: any) => {
          const name = d.instanceName
            ? `\\${d.categoryName}(${d.instanceName})\\${d.counterName}`
            : `\\${d.categoryName}\\${d.counterName}`;
          return {
            timestamp,
            name,
            value: d.value,
          };
        });
        console.log("Fetched data:", newData); // Log fetched data
        setGraphData((prevData) => [...prevData, ...newData]);
      } catch (error) {
        console.error("Error retrieving counter values:", error);
      }
    };

    const intervalId = setInterval(fetchData, 1000);

    return () => {
      clearInterval(intervalId);
    };
  }, [counters, isFetching, setGraphData]);

  useEffect(() => {
    if (graphData.length === 0) return;

    const svg = d3.select(chartRef.current);
    svg.selectAll("*").remove(); // Clear previous content

    const margin = { top: 20, right: 30, bottom: 60, left: 60 };
    const width = 800 - margin.left - margin.right;
    const height = 400 - margin.top - margin.bottom;

    const x = d3
      .scaleTime()
      .domain(d3.extent(graphData, (d) => d.timestamp) as [Date, Date])
      .range([0, width]);

    const y = d3
      .scaleLinear()
      .domain([0, d3.max(graphData, (d) => d.value) || 0])
      .nice()
      .range([height, 0]);

    const color = d3.scaleOrdinal(d3.schemeCategory10);

    const line = d3
      .line<CounterData>()
      .x((d) => x(d.timestamp))
      .y((d) => y(d.value));

    const chart = svg
      .append("g")
      .attr("transform", `translate(${margin.left},${margin.top})`);

    const groupedData = d3.group(graphData, (d) => d.name);

    groupedData.forEach((values, key) => {
      const sanitizedKey = key.replace(/[^a-zA-Z0-9]/g, "_"); // Sanitize key
      chart
        .append("path")
        .datum(values)
        .attr("fill", "none")
        .attr("stroke", color(sanitizedKey))
        .attr("stroke-width", 1.5)
        .attr("d", line);

      chart
        .selectAll(`.dot-${sanitizedKey}`)
        .data(values)
        .enter()
        .append("circle")
        .attr("class", `dot-${sanitizedKey}`)
        .attr("cx", (d) => x(d.timestamp))
        .attr("cy", (d) => y(d.value))
        .attr("r", 3)
        .attr("fill", color(sanitizedKey));
    });

    chart
      .append("g")
      .attr("class", "x-axis")
      .attr("transform", `translate(0,${height})`)
      .call(d3.axisBottom(x))
      .append("text")
      .attr("fill", "#000")
      .attr("x", width / 2)
      .attr("y", 40)
      .attr("text-anchor", "middle")
      .text("Time");

    chart
      .append("g")
      .attr("class", "y-axis")
      .call(d3.axisLeft(y))
      .append("text")
      .attr("fill", "#000")
      .attr("transform", "rotate(-90)")
      .attr("x", -height / 2)
      .attr("y", -50)
      .attr("text-anchor", "middle")
      .text("Value");

    // Add legend
    const legend = svg
      .append("g")
      .attr(
        "transform",
        `translate(${margin.left},${height + margin.bottom - 20})`
      );

    let legendIndex = 0;
    let totalWidth = 1000;
    let baseOffsetFromTop = 50;
    const legendItemWidth = 400; // Width of each legend item
    const svgWidth = totalWidth - margin.left - margin.right; // Available width for legend
    const itemsPerRow = 2; //Math.floor(svgWidth / legendItemWidth); // Number of items per row

    groupedData.forEach((_, key) => {
      const sanitizedKey = key.replace(/[^a-zA-Z0-9]/g, "_"); // Sanitize key
      const xOffset =
        baseOffsetFromTop + (legendIndex % itemsPerRow) * legendItemWidth; // Adjust x position
      const yOffset =
        baseOffsetFromTop + Math.floor(legendIndex / itemsPerRow) * 20; // Adjust y position

      legend
        .append("rect")
        .attr("x", xOffset)
        .attr("y", yOffset)
        .attr("width", 10)
        .attr("height", 10)
        .attr("fill", color(sanitizedKey));

      legend
        .append("text")
        .attr("x", xOffset + 15)
        .attr("y", yOffset + 9)
        .text(key)
        .style("font-size", "12px")
        .attr("alignment-baseline", "middle");

      legendIndex++;
    });
  }, [graphData]);

  return (
    <div>
      <svg ref={chartRef} width="{totalWidth}" height="650"></svg>
    </div>
  );
};

export default SimpleCounterChart;
