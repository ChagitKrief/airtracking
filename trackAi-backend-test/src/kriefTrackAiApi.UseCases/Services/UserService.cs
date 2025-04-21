using AutoMapper;
using kriefTrackAiApi.Common.Dto;
using kriefTrackAiApi.Core.Models;
using kriefTrackAiApi.Core.Interfaces;
using kriefTrackAiApi.UseCases.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using kriefTrackAiApi.Infrastructure.Email;

namespace kriefTrackAiApi.UseCases.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly IMapper _mapper;

    private readonly EmailWelcomeService _emailWelcomeService;

    public UserService(
        IUserRepository repository,
        IMapper mapper,
        EmailWelcomeService emailWelcomeService)
    {
        _repository = repository;
        _mapper = mapper;
        _emailWelcomeService = emailWelcomeService;
    }


    public async Task<List<UserDto>> GetAllAsync()
    {
        var users = await _repository.GetAllAsync();
        Console.WriteLine($"Fetched {users?.Count ?? 0} users from DB");

        if (users == null || users.Count == 0)
        {
            Console.WriteLine(" No users found, returning empty list.");
            return new List<UserDto>();
        }

        try
        {
            var mappedUsers = _mapper.Map<List<UserDto>>(users);
            Console.WriteLine($" Successfully mapped {mappedUsers.Count} users.");
            return mappedUsers;
        }
        catch (Exception ex)
        {
            Console.WriteLine($" AutoMapper error: {ex.Message}");
            throw;
        }
    }

    public async Task<UserDto> GetByIdAsync(Guid id)
    {
        var user = await _repository.GetByIdAsync(id);
        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> AddAsync(UserDto userDto)
    {
        var existingUser = await _repository.GetByEmailAsync(userDto.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException($"User with email {userDto.Email} already exists.");
        }

        var user = _mapper.Map<User>(userDto);
        user.CompanyIds ??= new List<Guid>();

        var rawPassword = user.Password;

        await _repository.AddAsync(user);
        var userDtoResult = _mapper.Map<UserDto>(user);

        _ = Task.Run(() =>
        {
            return _emailWelcomeService.SendWelcomeEmailAsync(user, rawPassword);
        });

        return userDtoResult;
    }


    public async Task<UserDto> UpdateAsync(UserDto userDto)
    {
        var user = _mapper.Map<User>(userDto);
        if (user.CompanyIds == null)
        {
            user.CompanyIds = new List<Guid>();
        }

        var updatedUser = await _repository.UpdateAsync(user.Id, user);
        return _mapper.Map<UserDto>(updatedUser);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _repository.DeleteAsync(id);
    }
}
