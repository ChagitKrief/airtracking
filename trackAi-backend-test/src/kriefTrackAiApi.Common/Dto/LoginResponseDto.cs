using System;
using System.Collections.Generic;

namespace kriefTrackAiApi.Common.Dto
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Role { get; set; }
        public DateTime Expiration { get; set; }
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string[] Reminders { get; set; } = Array.Empty<string>();
        public List<CustomerDto> ActiveCustomers { get; set; } = new List<CustomerDto>();
        public List<DataItem>? WinwordData { get; set; }
    }


    public class CustomerDto
    {
        public Guid Id { get; set; }
        public int CustomerNumber { get; set; }
        public string CustomerName { get; set; } = string.Empty;
    }


}
