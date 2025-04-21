using System;

namespace kriefTrackAiApi.Common.Dto
{
    public class ContainerPhoneDto
    {
        public int CustomerNumber { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string? SmsId { get; set; }
        public string? ShipmentId { get; set; }
    }
}
