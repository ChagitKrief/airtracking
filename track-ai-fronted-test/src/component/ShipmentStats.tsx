// 📁 File: src/component/ShipmentStats.tsx

import React from "react";
import { Box, Typography } from "@mui/material";
import { useTheme } from "@mui/material/styles";
import useMediaQuery from "@mui/material/useMediaQuery";

interface ShipmentStatsProps {
  stats: {
    total: number;
    onTime: number;
    early: number;
    delayed: number;
    critical: number;
    completed: number;
  };
  statusColors: Record<string, string>;
}

interface StatusKey {
  label: string;
  key: keyof Omit<ShipmentStatsProps["stats"], "total">;
}

/**
 * Displays visual summary of shipment status counts with colored progress bars.
 */
const ShipmentStats: React.FC<ShipmentStatsProps> = ({
  stats,
  statusColors,
}) => {
  const getPercentage = (value: number) =>
    stats.total ? (value / stats.total) * 100 : 0;

  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down("sm"));

  const statusKeys: StatusKey[] = [
    { label: "On Time", key: "onTime" },
    { label: "Early (1+ days)", key: "early" },
    { label: "Significant delay (1-4 days)", key: "delayed" },
    { label: "Critical delay (5+ days)", key: "critical" },
    { label: "Tracking Completed", key: "completed" },
  ];

  return (
    <Box
      display="flex"
      flexDirection={isMobile ? "column" : "row"}
      alignItems={isMobile ? "stretch" : "center"}
      gap={isMobile ? 2 : 0}
    >
      <Box
        sx={{
          textAlign: "center",
          pr: isMobile ? 0 : 3,
          borderRight: isMobile ? "none" : "1px solid #e0e0e0",
          borderBottom: isMobile ? "1px solid #e0e0e0" : "none",
          pb: isMobile ? 1 : 0,
        }}
      >
        <Typography variant="h4" sx={{ fontWeight: "bold", color: "#1e293b" }}>
          {stats.total}
        </Typography>
        <Typography variant="subtitle2" color="textSecondary">
          Total Shipments
        </Typography>
      </Box>

      <Box sx={{ flex: 1, pl: 3 }}>
        {statusKeys.map(({ label, key }) => (
          <Box key={key} display="flex" alignItems="center" mb={0.5}>
            <Typography
              variant="body2"
              sx={{
                width: isMobile ? 130 : 180,
                fontWeight: 600,
                fontSize: isMobile ? "13px" : "14px",
              }}
            >
              {label}
            </Typography>
            <Box
              sx={{
                flex: 1,
                mx: 1,
                backgroundColor: "#e5e7eb",
                borderRadius: "4px",
                overflow: "hidden",
              }}
            >
              <Box
                sx={{
                  width: `${getPercentage(stats[key])}%`,
                  height: isMobile ? 6 : 8,
                  backgroundColor: statusColors[key],
                  transition: "width 0.5s ease-in-out",
                }}
              />
            </Box>
            <Typography variant="body2" sx={{ fontWeight: 700 }}>
              {stats[key]}
            </Typography>
          </Box>
        ))}
      </Box>
    </Box>
  );
};

export default ShipmentStats;
