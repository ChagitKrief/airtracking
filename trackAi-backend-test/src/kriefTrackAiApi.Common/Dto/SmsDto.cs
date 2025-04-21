using System;
using System.Text.Json;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace kriefTrackAiApi.Common.Dto
{
    public class SmsDto
    {
        public Guid Id { get; set; }

        public string Container { get; set; } = string.Empty;

        public string UserPhonesJson { get; set; } = "[]";

        [NotMapped]
        public List<UserPhoneEntryDto> UserPhones
        {
            get => JsonSerializer.Deserialize<List<UserPhoneEntryDto>>(UserPhonesJson) ?? new();
            set => UserPhonesJson = JsonSerializer.Serialize(value);
        }
    }
    [NotMapped]
    public class UserPhoneEntryDto
    {
        public Guid UserId { get; set; }

        public List<string> Phones { get; set; } = new();
    }
}

