using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using kriefTrackAiApi.Core.Models;

namespace kriefTrackAiApi.Core.Interfaces;

  public interface IUserRepository
  {
      Task<List<User>> GetAllAsync();
      Task<User?> GetByIdAsync(Guid id);
      Task<User?> GetByEmailAsync(string email);
      Task<User> AddAsync(User user);
      Task<User?> UpdateAsync(Guid id, User user);
      Task<bool> DeleteAsync(Guid id);
      Task<User> RegisterAsync(User user);
      Task<User?> ValidateUserAsync(string email, string password);
  }
