using AutoMapper;
using kriefTrackAiApi.Common.Dto;
using kriefTrackAiApi.Core.Models;
using kriefTrackAiApi.Core.Interfaces;
using kriefTrackAiApi.UseCases.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace kriefTrackAiApi.UseCases.Services;

public class SmsService : ISmsService
{
    private readonly ISmsRepository _repository;
    private readonly IMapper _mapper;

    public SmsService(ISmsRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<SmsDto?> AddOrUpdateEntryAsync(string container, UserPhoneEntryDto entryDto)
    {
        var entry = _mapper.Map<UserPhoneEntry>(entryDto);
        var result = await _repository.AddOrUpdateEntryAsync(container, entry);
        return result == null ? null : _mapper.Map<SmsDto>(result);
    }

    public async Task<bool> RemoveEntryAsync(string container, Guid userId)
    {
        return await _repository.RemoveEntryAsync(container, userId);
    }

    public async Task<List<SmsDto>> GetAllByUserAsync(Guid userId)
    {
        var result = await _repository.GetAllByUserAsync(userId);
        return _mapper.Map<List<SmsDto>>(result);
    }
}