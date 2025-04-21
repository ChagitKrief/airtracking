import { useEffect} from "react";
import { Box } from "@mui/material";

interface MapIframeProps {
  containerId: string | null;
  isFull?: boolean;
  isCompact?: boolean;
  onLoadChange?: (isLoaded: boolean) => void;
}

export default function MapIframe({
  containerId,
  isFull = false,
  isCompact = false,
  onLoadChange,
}: MapIframeProps) {

  const MAPBOX_BASE_URL = __BLAZOR_MAPBOX_URL__;
  const src = containerId
    ? `${MAPBOX_BASE_URL}/${containerId}`
    : MAPBOX_BASE_URL;
  // const src = containerId
  //     ? `http://192.168.171.30:57677/blazor/mapbox/${containerId}`
  //     : `http://192.168.171.30:57677/blazor/mapbox`;
  
  useEffect(() => {
    onLoadChange?.(false); // reset loading on URL change
  }, [src]);

  let width = "100%";
  if (!isFull && isCompact) width = "50%";

  return (
    <Box
      sx={{
        position: "absolute",
        top: 0,
        right: 0,
        width,
        height: "100%",
        zIndex: 1,
        transition: "width 0.3s ease-in-out",
      }}
    >
      <iframe
        key={src}
        src={src}
        title="Map"
        width="100%"
        height="100%"
        onLoad={() => onLoadChange?.(true)}
        style={{ border: "none" }}
      />
    </Box>
  );
}
