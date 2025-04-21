import { JSX } from "react";

export interface BusinessDataItem {
    key: string;
    value: string;
}

export interface Metadata {
    businessData?: BusinessDataItem[];
}

export interface Carrier {
    shortName?: string;
}

export interface StatusEvent {
    description?: string;
    vessel?: { name?: string };
}

export interface PortProperties {
    locode?: string;
    name?: string;
}

export interface PortData {
    properties?: PortProperties;
}

export interface Status {
    pol?: PortData;
    pod?: PortData;
    currentEvent?: StatusEvent;
    actualDepartureAt?: string;
    estimatedDepartureAt?: string;
    actualArrivalAt?: string;
    estimatedArrivalAt?: string;
    predicted?: {
        datetime?: string;
        diffFromCarrierDays?: number;
    };
    voyageStatus?: string;
    events?: StatusEvent[];
}

export interface ShipmentData {
    id: string;
    containerNumber: string;
    bol: string | null;
    carrier?: Carrier;
    status?: Status;
    initialCarrierETA?: string;
    initialCarrierETD?: string;
}

export interface TrackedShipment {
    id: string;
    metadata?: Metadata;
    shipment: ShipmentData;
}

export interface ShipmentRow {
    id: string;
    shipmentId: string;
    containerNumber: string;
    bol: string;
    carrier: string;
    initialCarrierETD: string;
    latestCarrierETDOrATD: string;
    pol: string | JSX.Element;
    pod: string | JSX.Element;
    path: {
        pol: string;
        pod: string;
        progress: number;
    };
    containerStatus: string;
    currentVessel: string;
    initialCarrierETA: string;
    latestCarrierETAOrATA: string;
    maritimeAiPredictedETA: string;
    statusInsights: string;
    originCountry: string;
    supplierName: string;
    consigneeAddress: string;
    customerReference: string;
    customerNumber: string;
    differenceFromCarrierDays: number | null;
    insights: string | number;
    events: any[];
    voyageStatus: string;
}
