using System;
using System.Collections.Generic;

namespace kriefTrackAiApi.Common.Dto
{
    public class UserDto
    {
        public Guid Id { get; set; } // Primary Key
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; } = string.Empty;
        public string VerificationTmpPass { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string[] Reminders { get; set; } = Array.Empty<string>(); 
        public DateTime? MaxSubscriptionDate { get; set; }
        public int Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // List of Company IDs
        public List<Guid> CompanyIds { get; set; } = new();

        public override string ToString()
    {
        // Customize the output to include desired properties.
        return $"Id: {Id}, Name: {FirstName} {LastName}, CompanyIds: [{string.Join(", ", CompanyIds ?? new List<Guid>())}]";
    }
    }
}
