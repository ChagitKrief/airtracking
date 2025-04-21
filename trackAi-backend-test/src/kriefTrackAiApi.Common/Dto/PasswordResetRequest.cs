using System;
using System.Collections.Generic;

namespace kriefTrackAiApi.Common.Dto
{
    public class PasswordResetRequest
    {
        public Guid UserId { get; set; }
        public string TempPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
