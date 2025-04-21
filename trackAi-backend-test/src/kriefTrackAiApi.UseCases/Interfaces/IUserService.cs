using kriefTrackAiApi.Common.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace kriefTrackAiApi.UseCases.Interfaces;

  public interface IUserService
  {
      Task<List<UserDto>> GetAllAsync();
      Task<UserDto> GetByIdAsync(Guid id);
      Task<UserDto> AddAsync(UserDto userDto);
      Task<UserDto> UpdateAsync(UserDto userDto);
      Task<bool> DeleteAsync(Guid id);
  }
