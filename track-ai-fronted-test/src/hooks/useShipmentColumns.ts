
import { useMemo } from "react";
import { getFullShipmentColumns, compactShipmentColumns } from "../constants/shipmentColumns";

/**
 * Returns the appropriate DataGrid column layout for shipments based on layout mode.
 * @param isCompact - whether the layout is compact
 * @param userId - current user ID for context
 * @param userPhones - SMS tracking entries for containers
 * @param setSelectedContainerId - setter for container tracking modal
 * @param setDialogOpen - toggle modal visibility
 */
export function useShipmentColumns(
  isCompact: boolean,
  userId: string,
  userPhones: any[],
  setSelectedContainerId: (id: string) => void,
  setDialogOpen: (open: boolean) => void
) {
  const fullColumns = useMemo(
    () => getFullShipmentColumns(userId, userPhones, setSelectedContainerId, setDialogOpen),
    [userId, userPhones]
  );

  return isCompact ? compactShipmentColumns : fullColumns;
}
