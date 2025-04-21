using System;
using System.Text.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace kriefTrackAiApi.Core.Models;

 public class Sms
{
    public Guid Id { get; set; }

    public string Container { get; set; } = string.Empty;

    public string UserPhonesJson { get; set; } = "[]"; 

    [NotMapped]
    public List<UserPhoneEntry> UserPhones
    {
        get => JsonSerializer.Deserialize<List<UserPhoneEntry>>(UserPhonesJson) ?? new();
        set => UserPhonesJson = JsonSerializer.Serialize(value);
    }
}
[NotMapped]
public class UserPhoneEntry
{
    public Guid UserId { get; set; }

    public List<string> Phones { get; set; } = new();
}
