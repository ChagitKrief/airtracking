import { useState } from "react";

/**
 * Custom hook to manage shipment filters: search text, selected companies, insights, and statuses.
 * Provides handlers for changing and clearing filters in a consistent way.
 */
export function useShipmentFilters() {
  const [searchText, setSearchText] = useState("");
  const [selectedCompanies, setSelectedCompanies] = useState<string[]>([]);
  const [pendingInsights, setPendingInsights] = useState<string[]>([]);
  const [selectedInsights, setSelectedInsights] = useState<string[]>([]);
  const [pendingStatuses, setPendingStatuses] = useState<string[]>([]);
  const [selectedStatuses, setSelectedStatuses] = useState<string[]>([]);

  /** Set new search text */
  const requestSearch = (text: string) => setSearchText(text);

  /** Add or remove a filter value from the selected arrays */
  const handlePendingFilterChange = (value: string, type: "insight" | "status") => {
    const toggleValue = (prev: string[]) =>
      prev.includes(value) ? prev.filter((v) => v !== value) : [...prev, value];

    if (type === "insight") {
      setPendingInsights(toggleValue);
    } else {
      setSelectedStatuses(toggleValue); // <- update selectedStatuses immediately
    }
  };

  /** Commit selected filters */
  const applyPendingFilters = () => {
    setSelectedInsights([...pendingInsights]);
    // no longer updating selectedStatuses here
  };

  /** Reset all filters */
  const clearFilters = () => {
    setSelectedCompanies([]);
    setPendingInsights([]);
    setSelectedInsights([]);
    setPendingStatuses([]);
    setSelectedStatuses([]);
    setSearchText("");
  };

  return {
    searchText,
    requestSearch,
    selectedCompanies,
    setSelectedCompanies,
    pendingInsights,
    setPendingInsights,
    selectedInsights,
    selectedStatuses,
    pendingStatuses,
    setPendingStatuses,
    setSelectedStatuses,
    handlePendingFilterChange,
    applyPendingFilters,
    clearFilters,
  };
}