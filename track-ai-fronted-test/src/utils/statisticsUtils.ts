import { ShipmentRow } from "../types/TrackedShipmentType";

/**
 * Calculates statistics summary for filtered shipment rows.
 * @param filteredRows - array of rows to analyze
 */
export function calculateShipmentStats(filteredRows: ShipmentRow[]) {
  const stats = {
    total: filteredRows.length,
    onTime: 0,
    early: 0,
    delayed: 0,
    critical: 0,
    completed: 0,
  };

  filteredRows.forEach((row) => {
    const status = row.statusInsights;
    if (status === "Tracking Completed") stats.completed++;
    else if (status.includes("On Time")) stats.onTime++;
    else if (status.includes("Early")) stats.early++;
    else if (status.includes("Significant delay")) stats.delayed++;
    else if (status.includes("Critical delay")) stats.critical++;
  });

  return stats;
}
