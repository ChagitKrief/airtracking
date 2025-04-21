// 📁 File: src/component/FiltersPanel.tsx

import React from "react";
import { Typography, MenuItem, Checkbox, Button, Box } from "@mui/material";
import { insightOptions, statusOptions } from "../constants/filters";

interface FiltersPanelProps {
  open: boolean;
  onClose: () => void;
  onApplyFilters: () => void;
  selectedInsights: string[];
  selectedStatuses: string[];
  handleFilterChange: (value: string, type: "insight" | "status") => void;
}

/**
 * A reusable filters panel to filter shipments by insight and status.
 * Can be toggled and controlled from parent dashboard.
 */
const FiltersPanel: React.FC<FiltersPanelProps> = ({
  open,
  onClose,
  selectedInsights,
  selectedStatuses,
  handleFilterChange,
  onApplyFilters,
}) => {
  if (!open) return null;

  return (
    <Box
      sx={{
        maxHeight: 360,
        overflowY: "auto",
        scrollbarWidth: "none",
        "&::-webkit-scrollbar": {
          display: "none",
        },
      }}
    >
      <Typography variant="subtitle1" fontWeight="bold" sx={{ mb: 1 }}>
        Insights
      </Typography>
      {insightOptions.map((insight) => (
        <MenuItem
          key={insight.title}
          onClick={() => handleFilterChange(insight.title, "insight")}
        >
          <Checkbox checked={selectedInsights.includes(insight.title)} />
          {insight.title}
        </MenuItem>
      ))}

      <Typography variant="subtitle1" fontWeight="bold" sx={{ mt: 2, mb: 1 }}>
        Statuses
      </Typography>
      {statusOptions.map((status) => (
        <MenuItem
          key={status}
          onClick={() => handleFilterChange(status, "status")}
        >
          <Checkbox checked={selectedStatuses.includes(status)} />
          {status}
        </MenuItem>
      ))}

      <Button
        variant="contained"
        color="warning"
        fullWidth
        sx={{ mt: 2 }}
        onClick={() => {
          onApplyFilters(); 
          onClose(); 
        }}
      >
        Filter/Close
      </Button>
    </Box>
  );
};

export default FiltersPanel;
