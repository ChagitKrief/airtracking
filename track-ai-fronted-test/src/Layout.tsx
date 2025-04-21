import React from "react";
import { Box } from "@mui/material";
import { Outlet } from "react-router-dom";
import Navbar from "../src/component/Navbar";

export default function Layout() {
  return (
    <Box>
      <Navbar />
      <Box sx={{ marginTop: "70px", padding: "20px" }}>
        <Outlet /> 
      </Box>
    </Box>
  );
}
