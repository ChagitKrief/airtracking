import { TrackedShipment } from "../types/TrackedShipmentType";
import {
  formatDate,
  formatStatusInsights,
  getPathProgress,
  getPortWithFlag,
} from "../utils/shipmentUtils";

export function mapShipmentsToRows(
  shipments: TrackedShipment[],
  userId: string
) {
  return shipments.map((tracked, index) => {
    const pol = tracked.shipment.status?.pol?.properties || {};
    const pod = tracked.shipment.status?.pod?.properties || {};

    const podPort =
      tracked.metadata?.businessData?.find(
        (item: { key: string; }) => item.key === "Discharge Port"
      )?.value || "N/A";

    const customerNumber =
      tracked.metadata?.businessData?.find(
        (item: { key: string; }) => item.key === "Customer Code"
      )?.value || "N/A";

    const latestStatus = tracked.shipment.status;
    const lastEvent =
      latestStatus?.events?.[latestStatus.events.length - 1] ?? null;

    const diffDays = latestStatus?.predicted?.diffFromCarrierDays ?? 0;

    const statusInsights =
      latestStatus?.voyageStatus === "trackingCompleted"
        ? "Tracking Completed"
        : diffDays === null
          ? "N/A"
          : diffDays === 0
            ? "On Time"
            : diffDays < 0
              ? `Early (${Math.abs(diffDays)}+ days)`
              : diffDays <= 4
                ? `Significant delay (${diffDays} days)`
                : `Critical delay (${diffDays}+ days)`;

    return {
      id: tracked.shipment.id ?? index,
      containerNumber: tracked.shipment.containerNumber || "N/A",
      bol: tracked.shipment.bol || "N/A",
      carrier: tracked.shipment.carrier?.shortName || "N/A",

      initialCarrierETD: tracked.shipment.initialCarrierETD
        ? formatDate(tracked.shipment.initialCarrierETD)
        : "N/A",

      latestCarrierETDOrATD: latestStatus?.actualDepartureAt
        ? formatDate(latestStatus.actualDepartureAt)
        : latestStatus?.estimatedDepartureAt
          ? formatDate(latestStatus.estimatedDepartureAt)
          : "N/A",

      pol: tracked.shipment.status?.pol
        ? getPortWithFlag(pol.locode, pol.name)
        : "N/A",

      pod: tracked.shipment.status?.pod
        ? getPortWithFlag(pod.locode, podPort)
        : "N/A",

      path: {
        pol: pol.locode || "N/A",
        pod: pod.locode || "N/A",
        progress: getPathProgress(tracked.shipment),
      },

      containerStatus:
        latestStatus?.currentEvent?.description ||
        lastEvent?.description ||
        "Unknown",

      currentVessel:
        latestStatus?.currentEvent?.vessel?.name ||
        lastEvent?.vessel?.name ||
        "N/A",

      initialCarrierETA: tracked.shipment.initialCarrierETA
        ? formatDate(tracked.shipment.initialCarrierETA)
        : "N/A",

      latestCarrierETAOrATA: latestStatus?.actualArrivalAt
        ? formatDate(latestStatus.actualArrivalAt)
        : latestStatus?.estimatedArrivalAt
          ? formatDate(latestStatus.estimatedArrivalAt)
          : "N/A",

      maritimeAiPredictedETA: latestStatus?.predicted?.datetime
        ? formatDate(latestStatus.predicted.datetime)
        : "N/A",

      originCountry:
        tracked.metadata?.businessData?.find(
          (item: { key: string; }) => item.key.trim() === "Origin Country"
        )?.value || "N/A",

      supplierName:
        tracked.metadata?.businessData?.find(
          (item: { key: string; }) => item.key === "Supplier Name"
        )?.value || "N/A",

      consigneeAddress:
        tracked.metadata?.businessData?.find(
          (item: { key: string; }) => item.key === "Consignee Address"
        )?.value || "N/A",

      customerReference:
        tracked.metadata?.businessData?.find(
          (item: { key: string; }) => item.key === "Customer Reference"
        )?.value || "N/A",

      customerNumber,
      statusInsights,
      insights: statusInsights,
      differenceFromCarrierDays: diffDays,
      voyageStatus: latestStatus?.voyageStatus ?? "N/A",
      events: latestStatus?.events ?? [],
      currentEventIndex: latestStatus?.events?.findIndex(
        (e) => e.description === latestStatus?.currentEvent?.description
      ) ?? -1,
    };
  });
}
