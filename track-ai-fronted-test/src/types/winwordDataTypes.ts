export interface ShipmentStatus {
  estimatedArrivalAt?: string;
  actualArrivalAt?: string;
  pol?: { properties: { name: string } };
  pod?: { properties: { locode: string } };
}

export interface Shipment {
  smsAlert: string;
  containerNumber: string;
  bol: string;
  carrier: { shortName: string };
  initialCarrierETD?: string;
  latestCarrierETD?: string;
  status: ShipmentStatus;
  statusText?: string;
  vessel?: string;
  maritimeAIPredictedETA?: string;
  originCountry?: string;
  supplierName?: string;
  consigneeAddress?: string;
  customerReference?: string;
}
  
  export interface TrackedShipments {
    total?: number;
    data?: Shipment[];
  }
  
  export interface WinwordData {
    data?: {
      trackedShipments?: TrackedShipments;
    };
  }
  
  export interface SMSData {
    containerNumber: string;
    phoneNumber: string;
  }
  