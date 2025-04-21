using System;

namespace kriefTrackAiApi.Common.Dto
{
    public class EmailAttachment
    {
        public string Name { get; set; } = string.Empty;
        public string ContentBytes { get; set; } = string.Empty;
        public string? ContentId { get; set; }  
    }

}

