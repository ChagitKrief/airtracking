// 📁 File: src/utils/filterUtils.ts

import { ShipmentRow } from "../types/TrackedShipmentType";

interface ShipmentFilters {
  selectedCompanies: string[];
  searchText: string;
  selectedInsights: string[];
  selectedStatuses: string[];
}

/**
 * Filters an array of shipment rows based on multiple dynamic filters.
 * @param rows - full list of mapped shipment rows
 * @param filters - active filtering selections
 */
export function filterShipments(
  rows: ShipmentRow[],
  filters: ShipmentFilters
): ShipmentRow[] {
  const { selectedCompanies, searchText, selectedInsights, selectedStatuses } = filters;
  let result = [...rows];

  if (selectedCompanies.length > 0) {
    result = result.filter((row) => selectedCompanies.includes(row.customerNumber));
  }

  if (searchText) {
    const regex = new RegExp(searchText, "i");
    result = result.filter((row) =>
      Object.values(row).some((value) => value && regex.test(value.toString()))
    );
  }

  if (selectedInsights.length > 0) {
    result = result.filter((row) => selectedInsights.includes(row.statusInsights));
  }

  if (selectedStatuses.length > 0) {
    result = result.filter((row) =>
      selectedStatuses.some((status) =>
        row.statusInsights.startsWith(status.split("(")[0].trim())
      )
    );
  }

  return result;
}
