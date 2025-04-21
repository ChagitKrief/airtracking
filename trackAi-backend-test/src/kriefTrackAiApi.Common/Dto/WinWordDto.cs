namespace kriefTrackAiApi.Common.Dto
{
    public class BusinessDataItem
    {
        public string? key { get; set; }
        public string? value { get; set; }
    }

    public class Metadata
    {
        public List<BusinessDataItem>? businessData { get; set; }
        public string? eta { get; set; }
        public string? jobNumber { get; set; }
    }

    public class Predicted
    {
        public string? datetime { get; set; }
        public string? code { get; set; }
        public int? diffFromCarrierDays { get; set; }
    }

    public class CarrierTimestamps
    {
        public string? code { get; set; }
        public DateTime? datetime { get; set; }
    }

    public class Departure
    {
        public CarrierTimestamps? timestamps { get; set; }
    }

    public class Milestone
    {
        public string? type { get; set; }
        public string? utcOffset { get; set; }
        public Departure? departure { get; set; }
    }

    public class Vessel
    {
        public string? name { get; set; }
    }

    public class Timestamps
    {
        public DateTime? datetime { get; set; }
        public DateTime? datetimeLocalized { get; set; }
        public string? code { get; set; }
    }

    public class Event
    {
        public string? description { get; set; }
        public Port? port { get; set; }
        public Vessel? vessel { get; set; }
        public string? voyage { get; set; }
        public Timestamps? timestamps { get; set; }
    }

    public class CurrentEvent
    {
        public string? description { get; set; }
        public Timestamps? timestamps { get; set; }
        public Vessel? vessel { get; set; }
    }

    public class Port
    {
        public Properties? properties { get; set; }
    }

    public class Properties
    {
        public string? locode { get; set; }
        public string? name { get; set; }
    }

    public class Pol
    {
        public Properties? properties { get; set; }
    }

    public class Pod
    {
        public Properties? properties { get; set; }
    }

    public class Status
    {
        public string? id { get; set; }
        public Pol? pol { get; set; }
        public Pod? pod { get; set; }
        public List<Event>? events { get; set; }
        public DateTime? estimatedArrivalAt { get; set; }
        public DateTime? actualArrivalAt { get; set; }
        public DateTime? estimatedDepartureAt { get; set; }
        public DateTime? actualDepartureAt { get; set; }
        public Predicted? predicted { get; set; }
        public List<Milestone>? milestones { get; set; }
        public string? voyageStatus { get; set; }
        public CurrentEvent? currentEvent { get; set; }
    }

    public class Carrier
    {
        public string? code { get; set; }
        public string? shortName { get; set; }
    }

    public class Shipment
    {
        public string? id { get; set; }
        public string? containerNumber { get; set; }
        public string? bol { get; set; }
        public string? scac { get; set; }
        public Carrier? carrier { get; set; }
        public Status? status { get; set; }
        public string? initialCarrierETA { get; set; }
        public string? initialCarrierETD { get; set; }
    }

    public class DataItem
    {
        public string? id { get; set; }
        public string? sharedShipmentLink { get; set; }
        public Metadata? metadata { get; set; }
        public Shipment? shipment { get; set; }
    }

    public class TrackedShipments
    {
        public int? total { get; set; }
        public int? totalFiltered { get; set; }
        public List<DataItem>? data { get; set; }

        public TrackedShipments()
        {
            data = new List<DataItem>();
        }
    }

    public class Data
    {
        public TrackedShipments? trackedShipments { get; set; }
        public List<DataItem>? trackedShipmentsByIds { get; set; }

        public Data()
        {
            trackedShipments = new TrackedShipments();
            trackedShipmentsByIds = new List<DataItem>();
        }
    }

    public class Root
    {
        public Data? data { get; set; }

        public Root()
        {
            data = new Data();
        }
    }

    public class TokenData
    {
        public string? publicAPIToken { get; set; }
    }

    public class TokenRoot
    {
        public TokenData? data { get; set; }
    }

    public class TrackedShipmentsRec
    {
        public int? totalFiltered { get; set; } = 0;
    }

    public class OntimeData
    {
        public TrackedShipmentsRec trackedShipments { get; set; }

        public OntimeData()
        {
            trackedShipments = new TrackedShipmentsRec();
        }
    }

    public class Root1
    {
        public OntimeData data { get; set; }

        public Root1()
        {
            data = new OntimeData();
        }
    }
}
