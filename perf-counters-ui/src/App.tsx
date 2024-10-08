import React from "react";
import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import PerformanceMonitor from "./PerformanceMonitor";
import IISPerformanceMonitor from "./IISPerformanceMonitor";
import Settings from "./Settings";
import Menu from "./Menu";
import AspNetPerformanceMonitor from "./components/aspnet/AspNetPerformanceMonitor";

const App: React.FC = () => {
  return (
    <Router>
      <Menu />
      <Routes>
        <Route path="/" element={<PerformanceMonitor />} />
        <Route path="/iis" element={<IISPerformanceMonitor />} />
        <Route path="/settings" element={<Settings />} />
        <Route path="/aspnet" element={<AspNetPerformanceMonitor />} />
      </Routes>
    </Router>
  );
};

export default App;
