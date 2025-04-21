using kriefTrackAiApi.Common.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace kriefTrackAiApi.UseCases.Interfaces;

public interface ISmsService
{
    Task<SmsDto?> AddOrUpdateEntryAsync(string container, UserPhoneEntryDto entry);
    Task<bool> RemoveEntryAsync(string container, Guid userId);
    Task<List<SmsDto>> GetAllByUserAsync(Guid userId);
}
