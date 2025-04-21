using System;
using System.Collections.Generic;

namespace kriefTrackAiApi.Common.Dto
{
    public class LoginRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
