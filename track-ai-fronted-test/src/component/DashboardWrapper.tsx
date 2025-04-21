import { useEffect, useState } from "react";
import {Box, IconButton, Tooltip } from "@mui/material";
import {
  ChevronLeft,
  ChevronRight,
  Fullscreen,
  FullscreenExit,
} from "@mui/icons-material";
import DashboardPage from "../pages/Dashboard";
import MapIframe from "./MapIframe";
import { useLoading } from "../context/LoadingContext";
import { useWinwordContext } from "../context/WinwordContext";

export default function DashboardWrapper() {
  const { resetWinwordData } = useWinwordContext();
  const [isExpanded, setIsExpanded] = useState(false);
  const [selectedContainerId, setSelectedContainerId] = useState<string | null>(
    null
  );
  const [isMapFullScreen, setIsMapFullScreen] = useState(false);
  const [isMapLoaded, setIsMapLoaded] = useState(false);
  const { showLoading, hideLoading } = useLoading();

  useEffect(() => {
    if (isMapLoaded) {
      hideLoading();
    } else {
      showLoading();
    }
  }, [isMapLoaded]);

  const toggleTable = () => {
    setIsExpanded((prev) => !prev);
    if (!isExpanded && isMapFullScreen) {
      setIsMapFullScreen(false);
    }
  };

  const toggleMapFull = () => {
    setIsMapFullScreen((prev) => !prev);
    if (!isMapFullScreen && isExpanded) {
      setIsExpanded(false);
    }
  };

  const sharedButtonStyle = {
    width: 40,
    height: 40,
    backgroundColor: "white",
    boxShadow: 2,
    padding: 0,
    "&:hover": { backgroundColor: "#f0f0f0" },
    transition: "all 0.2s ease",
  };

  return (
    <Box sx={{ position: "relative", height: "100vh", width: "100vw" }}>
      <MapIframe
        containerId={selectedContainerId}
        isFull={isMapFullScreen}
        isCompact={!isExpanded}
        onLoadChange={setIsMapLoaded}
      />

      {!isMapFullScreen && !isExpanded && (
        <Box
          sx={{
            position: "absolute",
            top: "50%",
            left: "calc(50% - 40px)",
            transform: "translateY(-50%)",
            zIndex: 10,
          }}
        >
          <Tooltip title="Increase Table">
            <IconButton
              onClick={toggleTable}
              sx={{
                ...sharedButtonStyle,
                borderRadius: "20px 0 0 20px",
                borderRight: "1px solid #ccc",
              }}
            >
              <ChevronRight sx={{ fontSize: 18 }} />
            </IconButton>
          </Tooltip>
        </Box>
      )}

      {!isMapFullScreen && !isExpanded && (
        <Box
          sx={{
            position: "absolute",
            top: "50%",
            left: "50%",
            transform: "translateY(-50%)",
            zIndex: 10,
          }}
        >
          <Tooltip title="Increase Map">
            <IconButton
              onClick={toggleMapFull}
              sx={{
                ...sharedButtonStyle,
                borderRadius: "0 20px 20px 0",
              }}
            >
              <Fullscreen sx={{ fontSize: 18 }} />
            </IconButton>
          </Tooltip>
        </Box>
      )}

      {isExpanded && (
        <Box
          sx={{
            position: "fixed",
            top: "50%",
            right: 0,
            transform: "translateY(-50%)",
            zIndex: 9998,
          }}
        >
          <Tooltip title="Small Table">
            <IconButton
              onClick={toggleTable}
              sx={{
                ...sharedButtonStyle,
                borderRadius: "20px 0 0 20px",
              }}
            >
              <ChevronLeft sx={{ fontSize: 18 }} />
            </IconButton>
          </Tooltip>
        </Box>
      )}
      {isMapFullScreen && (
        <Box
          sx={{
            position: "absolute",
            top: "50%",
            left: 0,
            transform: "translateY(-50%)",
            zIndex: 10,
          }}
        >
          <Tooltip title="Small Map">
            <IconButton
              onClick={toggleMapFull}
              sx={{
                ...sharedButtonStyle,
                borderRadius: "0 20px 20px 0",
              }}
            >
              <FullscreenExit sx={{ fontSize: 18 }} />
            </IconButton>
          </Tooltip>
        </Box>
      )}

      {!isMapFullScreen && (
        <Box
          sx={{
            position: "absolute",
            top: 0,
            left: 0,
            width: isExpanded ? "100%" : "50%",
            height: "100%",
            transition: "width 0.3s ease-in-out",
            backgroundColor: "white",
            zIndex: 2,
          }}
        >
          <DashboardPage
            isCompact={!isExpanded}
            onRowSelected={setSelectedContainerId}
          />
        </Box>
      )}
    </Box>
  );
}
