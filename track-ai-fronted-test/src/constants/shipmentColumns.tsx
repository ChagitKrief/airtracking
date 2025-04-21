// 📁 File: src/constants/shipmentColumns.ts

import { GridColDef } from "@mui/x-data-grid";
import {
  formatEtaWithColor,
  formatStatusInsights,
} from "../utils/shipmentUtils";
import { LinearProgress, Stack, IconButton, Tooltip } from "@mui/material";
import PhoneEnabledIcon from "@mui/icons-material/PhoneEnabled";
import FiberManualRecordIcon from "@mui/icons-material/FiberManualRecord";

/**
 * Compact column layout used for small table view
 */
export const compactShipmentColumns: GridColDef[] = [
  {
    field: "containerInfo",
    headerName: "Container + BOL",
    width: 220,
    renderCell: (params: any) =>
      `${params.row.containerNumber} | ${params.row.bol}`,
  },
  {
    field: "path",
    headerName: "Path",
    width: 250,
    renderCell: (params) => (
      <div
        style={{
          width: "100%",
          display: "flex",
          alignItems: "center",
          gap: "10px",
        }}
      >
        <span style={{ fontWeight: "bold" }}>
          {params.value.pol} → {params.value.pod}
        </span>
        <LinearProgress
          variant="determinate"
          value={params.value.progress}
          sx={{ flexGrow: 1, height: "10px", borderRadius: "5px" }}
        />
        <span style={{ fontSize: "12px", fontWeight: "bold" }}>
          {Math.round(params.value.progress)}%
        </span>
      </div>
    ),
  },
  {
    field: "latestCarrierETAOrATA",
    headerName: "Latest Carrier ETA/ATA",
    width: 180,
    renderCell: (params) =>
      formatEtaWithColor(params.value, params.row.statusInsights),
  },
  {
    field: "maritimeAiPredictedETA",
    headerName: "Maritime AI Predicted ETA",
    width: 180,
  },
];

/**
 * Full column layout for standard dashboard table view
 * @param userId string
 * @param userPhones array of SMS entries
 * @param setSelectedContainerId function
 * @param setDialogOpen function
 */
export const getFullShipmentColumns = (
  userId: string,
  userPhones: any[],
  setSelectedContainerId: (id: string) => void,
  setDialogOpen: (open: boolean) => void
): GridColDef[] => [
  { field: "containerNumber", headerName: "Container Number", width: 180 },
  { field: "bol", headerName: "BOL", width: 160 },
  { field: "carrier", headerName: "Latest Carrier", width: 140 },
  { field: "initialCarrierETD", headerName: "Initial Carrier ETD", width: 180 },
  {
    field: "latestCarrierETDOrATD",
    headerName: "Latest Carrier ETD/ATD",
    width: 180,
  },
  {
    field: "pol",
    headerName: "POL",
    width: 200,
    renderCell: (params) => params.value || "N/A",
  },
  {
    field: "pod",
    headerName: "POD",
    width: 200,
    renderCell: (params) => params.value || "N/A",
  },
  {
    field: "path",
    headerName: "Path",
    width: 250,
    renderCell: (params) => (
      <div
        style={{
          width: "100%",
          display: "flex",
          alignItems: "center",
          gap: "10px",
        }}
      >
        <span style={{ fontWeight: "bold" }}>
          {params.value.pol} → {params.value.pod}
        </span>
        <LinearProgress
          variant="determinate"
          value={params.value.progress}
          sx={{ flexGrow: 1, height: "10px", borderRadius: "5px" }}
        />
        <span style={{ fontSize: "12px", fontWeight: "bold" }}>
          {Math.round(params.value.progress)}%
        </span>
      </div>
    ),
  },
  { field: "containerStatus", headerName: "Container Status", width: 220 },
  { field: "currentVessel", headerName: "Current Vessel", width: 180 },
  { field: "initialCarrierETA", headerName: "Initial Carrier ETA", width: 180 },
  {
    field: "latestCarrierETAOrATA",
    headerName: "Latest Carrier ETA/ATA",
    width: 200,
    renderCell: (params) =>
      formatEtaWithColor(params.value, params.row.statusInsights),
  },
  {
    field: "maritimeAiPredictedETA",
    headerName: "Maritime AI Predicted ETA",
    width: 200,
  },
  {
    field: "statusInsights",
    headerName: "Status & Insights",
    width: 220,
    renderCell: (params) => formatStatusInsights(params.value),
  },
  { field: "originCountry", headerName: "Origin Country", width: 180 },
  { field: "supplierName", headerName: "Supplier Name", width: 220 },
  { field: "consigneeAddress", headerName: "Consignee Address", width: 220 },
  { field: "customerReference", headerName: "Customer Reference", width: 200 },
  //////////////////////////////////////
  // { field: "description", headerName: "Status", width: 200 },
  // { field: "location", headerName: "Location", width: 200 },
  // { field: "vessel", headerName: "Vessel", width: 200 },
  // { field: "voyage", headerName: "Voyage", width: 160 },
  // { field: "timeInfo", headerName: "Planned / Actual At", width: 200 },
  //////////////////////////////////////
  {
    field: "actions",
    headerName: "",
    width: 80,
    sortable: false,
    filterable: false,
    renderCell: (params: any) => {
      const sms = userPhones.find(
        (s) => s.container === params.row.containerNumber
      );
      const trackedEntry = sms?.userPhones.find(
        (e: { userId: string }) => e.userId === userId
      );
      const isPhoneTracked = trackedEntry && trackedEntry.phones.length > 0;

      return (
        <Tooltip
          title={
            isPhoneTracked ? "Phone tracking is active" : "Track this container"
          }
        >
          <IconButton
            onClick={(e) => {
              e.stopPropagation();
              setSelectedContainerId(params.row.containerNumber);
              setDialogOpen(true);
            }}
          >
            <Stack direction="row" spacing={0.5} alignItems="center">
              <PhoneEnabledIcon />
              {isPhoneTracked && (
                <FiberManualRecordIcon sx={{ fontSize: 10, color: "red" }} />
              )}
            </Stack>
          </IconButton>
        </Tooltip>
      );
    },
  },
];
