import { format } from "date-fns";
import { hasFlag } from "country-flag-icons";
import Flags from "country-flag-icons/react/3x2";
import { JSX } from "react";

/** Safely escape special characters in regex */
export function escapeRegExp(value: string): string {
  return value.replace(/[-[\]{}()*+?.,\\^$|#\s]/g, "\\$&");
}

/** Format date string to readable form */
export function formatDate(dateString?: string): string {
  if (!dateString) return "N/A";
  const date = new Date(dateString);
  return isNaN(date.getTime()) ? "N/A" : format(date, "dd MMM''yy h:mm a");
}

/** Show visual ETA with color dot and formatted date */
export function formatEtaWithColor(dateString: string | undefined, status: string): JSX.Element {
  const formattedDate = formatDate(dateString);
  if (formattedDate === "N/A") return <span style={{ color: "#999" }}>N/A</span>;

  let color = "#3b82f6";
  if (status.includes("Early")) color = "#14b8a6";
  else if (status.includes("Significant delay")) color = "#e67e22";
  else if (status.includes("Critical delay")) color = "#ef4444";
  else if (status.includes("Tracking Completed")) color = "#22c55e";

  return (
    <div style={{ display: "flex", alignItems: "center", gap: "6px" }}>
      <span
        style={{
          width: "8px",
          height: "8px",
          backgroundColor: color,
          borderRadius: "50%",
          display: "inline-block",
        }}
      />
      <span>{formattedDate}</span>
    </div>
  );
}

/** Format delay insight with dynamic coloring */
export function formatStatusInsights(value: string): JSX.Element {
  if (!value || value === "N/A" || value === "Unknown") {
    return <span style={{ color: "#999", fontSize: "13px" }}>N/A</span>;
  }

  let color = "#3b82f6";
  let sign = "";
  let label = "as initial ETA";
  let displayValue = "On Time";

  if (value.includes("Early")) {
    color = "#14b8a6";
    sign = "-";
    displayValue = `${value.replace(/\D/g, "")}d early`;
  } else if (value.includes("Significant delay")) {
    color = "#e67e22";
    sign = "+";
    displayValue = `${value.replace(/\D/g, "")}d late`;
  } else if (value.includes("Critical delay")) {
    color = "#ef4444";
    sign = "+";
    displayValue = `${value.replace(/\D/g, "")}d late`;
  } else if (value.includes("Tracking Completed")) {
    color = "#22c55e";
    displayValue = "Tracking Completed";
    label = "";
  }

  return (
    <div
      style={{
        display: "flex",
        flexDirection: "column",
        alignItems: "flex-start",
        lineHeight: "1.2",
        padding: "4px 0",
      }}
    >
      <span style={{ color, fontWeight: "bold", fontSize: "14px" }}>
        {sign}
        {displayValue}
      </span>
      {label && <span style={{ color: "#888", fontSize: "12px" }}>{label}</span>}
    </div>
  );
}

/** Get country flag component from locode */
export function getFlag(locode = ""): JSX.Element | null {
  const countryCode = locode.slice(0, 2).toUpperCase();
  if (!hasFlag(countryCode)) return null;
  const FlagComponent = Flags[countryCode];
  if (!FlagComponent) return null;

  const regionNames = new Intl.DisplayNames(["en"], { type: "region" });
  const countryName = regionNames.of(countryCode);

  return (
    <FlagComponent
      title={countryName}
      style={{
        width: "20px",
        height: "14px",
        marginRight: "4px",
        borderRadius: "2px",
        boxShadow: "0 0 2px rgba(0,0,0,0.2)",
      }}
    />
  );
}

/** Combine port flag + name + locode */
export function getPortWithFlag(
  locode?: string,
  name?: string
): JSX.Element | string {
  if (!locode || !name) return "N/A";
  return (
    <div
      style={{
        display: "flex",
        flexDirection: "row",
        alignItems: "center",
        gap: "6px",
        textAlign: "left",
        padding: "3px 0",
      }}
    >
      {getFlag(locode)}
      <span style={{ fontSize: "13px", fontWeight: "bold" }}>{name}</span>
      <span style={{ fontSize: "11px", color: "#666" }}>{locode}</span>
    </div>
  );
}

/** Difference in days between predicted and initial ETA */
export function diffCarrierDays(
  initialETA?: string,
  predictedETA?: string
): number | null {
  if (!initialETA || !predictedETA) return null;
  const initial = new Date(initialETA);
  const predicted = new Date(predictedETA);
  const msPerDay = 86400000;
  return Math.floor((predicted.getTime() - initial.getTime()) / msPerDay);
}

/** Calculate percentage progress of path */
export function getPathProgress(shipment: any): number {
  if (
    !shipment.status ||
    !shipment.initialCarrierETA ||
    !shipment.status.predicted?.datetime
  ) {
    return 0;
  }
  const shipmentStart = new Date(
    shipment.status.actualDepartureAt || shipment.status.estimatedDepartureAt
  );
  const shipmentEnd = new Date(
    shipment.status.predicted.datetime || shipment.status.actualArrivalAt
  );
  const now = new Date();
  if (shipmentStart >= shipmentEnd) return 100;
  const total = shipmentEnd.getTime() - shipmentStart.getTime();
  const elapsed = now.getTime() - shipmentStart.getTime();
  return Math.min(100, Math.max(0, (elapsed / total) * 100));
}
