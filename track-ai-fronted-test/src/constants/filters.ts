  //  Constant options for filter select menus
import { addDays } from "date-fns";

const currentDate = new Date();
const currentDatePastThreeDays = addDays(currentDate, 3);

export const insightOptions = [
  {
    title: "Arriving soon (1-3 days)",
    field: "shipment_arriving_at",
    ranges: { gte: currentDate.toISOString(), lte: currentDatePastThreeDays.toISOString() },
  },
  {
    title: "Arrived",
    field: "shipment_voyage_status",
    values: "arrived",
  },
  {
    title: "Tracking completed",
    field: "shipment_voyage_status",
    values: "trackingCompleted",
  },
  {
    title: "Rollover at POL",
    field: "shipment_delay_reasons",
    values: "RLV_POL",
  },
  {
    title: "Rollover at TSP",
    field: "shipment_delay_reasons",
    values: "RLV_TSP",
  },
  {
    title: "Late departure",
    field: "shipment_delay_reasons",
    values: "LTD_POL",
  },
  {
    title: "Transshipment delay",
    field: "shipment_delay_reasons",
    values: "TSD_TSP",
  },
  {
    title: "Insufficient T/S time",
    field: "shipment_delay_reasons",
    values: "ITT_TSP",
  },
  {
    title: "Routing deficiency",
    field: "shipment_delay_reasons",
    values: ["RDF_TSP", "RDF_POL"],
  },
  {
    title: "Late allocation",
    field: "shipment_delay_reasons",
    values: "FVD_POD",
  },
];

export const statusOptions = [
  "On Time",
  "Early (1+ days)",
  "Significant delay (1-4 days)",
  "Critical delay (5+ days)",
  "Tracking Completed",
];
