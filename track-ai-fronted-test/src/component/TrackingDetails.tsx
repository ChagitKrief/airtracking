// TrackingDetails.tsx
import React from "react";
import { Box, Typography, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Paper } from "@mui/material";

interface Event {
  description?: string;
  location?: string;
  vessel?: { name?: string };
  voyage?: string;
  plannedAt?: string;
  actualAt?: string;
}

export const showEventIconHandler = (eventRecord: Event) => {
  let imgAttr = { imgSrc: "", imgAlt: "" };
  if (eventRecord.description === "Empty to shipper") {
    imgAttr.imgSrc = "/Empty_to_shipper.png";
    imgAttr.imgAlt = "Empty_to_shipper";
  } else if (eventRecord.description === "Gate in at first POL") {
    // ...
  }
  // ... icon mapping ...
  return imgAttr;
};

interface TrackingDetailsProps {
  rowData: any;
  compact?: boolean;
}

export default function TrackingDetails({ rowData, compact = false }: TrackingDetailsProps) {
  const events: Event[] = rowData?.events || [];
  
  // If no data is provided or component is used directly (not via DataGrid)
  if (!rowData) {
    return <Typography color="text.secondary">No tracking data available</Typography>;
  }

  return (
    <Box sx={{ 
      p: compact ? 1 : 2,
      border: "1px solid #ccc",
      borderRadius: 2,
      backgroundColor: "#f9f9f9",
    }}>
      <Typography variant={compact ? "subtitle1" : "h6"} sx={{ mb: 2 }}>
        Container Tracking for {rowData.containerNumber}
      </Typography>
      
      <TableContainer component={Paper} sx={{ boxShadow: 0 }}>
        <Table size={compact ? "small" : "medium"}>
          <TableHead>
            <TableRow>
              <TableCell>Status</TableCell>
              <TableCell>Location</TableCell>
              <TableCell>Vessel</TableCell>
              <TableCell>Voyage</TableCell>
              <TableCell>Planned / Actual At</TableCell>
              <TableCell>Icon</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {events.length > 0 ? (
              events.map((event, index) => {
                const { imgSrc, imgAlt } = showEventIconHandler(event);
                return (
                  <TableRow key={index} sx={{ '&:last-child td, &:last-child th': { border: 0 } }}>
                    <TableCell>{event.description || "N/A"}</TableCell>
                    <TableCell>{event.location || "N/A"}</TableCell>
                    <TableCell>{event.vessel?.name || "N/A"}</TableCell>
                    <TableCell>{event.voyage || "N/A"}</TableCell>
                    <TableCell>
                      {event.plannedAt || "N/A"} / {event.actualAt || "N/A"}
                    </TableCell>
                    <TableCell>
                      {imgSrc ? (
                        <img src={imgSrc} alt={imgAlt} width={24} />
                      ) : (
                        "—"
                      )}
                    </TableCell>
                  </TableRow>
                );
              })
            ) : (
              <TableRow>
                <TableCell colSpan={6} align="center">
                  No tracking events available
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </TableContainer>
    </Box>
  );
}