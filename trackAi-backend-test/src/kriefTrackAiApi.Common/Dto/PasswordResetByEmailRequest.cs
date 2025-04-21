using System;
using System.Collections.Generic;

namespace kriefTrackAiApi.Common.Dto
{
    public class PasswordResetByEmailRequest
    {
        public string? Email { get; set; }
        public string? TempPassword { get; set; }
        public string? NewPassword { get; set; }
    }

}