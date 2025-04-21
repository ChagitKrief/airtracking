import React from "react";
import { useParams, useSearchParams } from "react-router-dom";
import { Box, Typography } from "@mui/material";

export default function WindWrapperPage() {
  const { containerId } = useParams();
  const [searchParams] = useSearchParams();
  const url = searchParams.get("url");

  if (!url) {
    return (
      <Typography color="error" sx={{ p: 4 }}>
        No external URL provided.
      </Typography>
    );
  }

  return (
    <Box
      sx={{
        height: "calc(100vh - 90px)",
        width: "100%",
        border: "1px solid #ddd",
        borderRadius: "8px",
        overflow: "hidden",
      }}
    >
      <iframe
        src={url}
        title={`Container ${containerId} Tracking`}
        width="100%"
        height="100%"
        style={{ border: "none" }}
      />
    </Box>
  );
}
