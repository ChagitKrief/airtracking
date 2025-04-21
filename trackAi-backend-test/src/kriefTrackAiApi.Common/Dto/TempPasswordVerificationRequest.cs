using System;
using System.Collections.Generic;

namespace kriefTrackAiApi.Common.Dto
{
    public class TempPasswordVerificationRequest
    {
        public Guid UserId { get; set; }
        public string TempPassword { get; set; } = string.Empty;
    }
}
