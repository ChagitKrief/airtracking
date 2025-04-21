using kriefTrackAiApi.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace kriefTrackAiApi.Core.Interfaces;

  public interface ISmsRepository
{
    Task<Sms?> AddOrUpdateEntryAsync(string container, UserPhoneEntry entry);
    Task<bool> RemoveEntryAsync(string container, Guid userId);
    Task<List<Sms>> GetAllByUserAsync(Guid userId);
}

