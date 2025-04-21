using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using kriefTrackAiApi.Core.Models;
using kriefTrackAiApi.Core.Interfaces;
using kriefTrackAiApi.Infrastructure.Data;

namespace kriefTrackAiApi.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    private readonly Argon2PasswordHasher _passwordHasher;
    public UserRepository(AppDbContext context)
    {
        _context = context;
        _passwordHasher = new Argon2PasswordHasher();
    }

    public async Task<List<User>> GetAllAsync()
    {
        var users = await _context.Users
         .AsNoTracking()
         .ToListAsync();

        return users ?? new List<User>();
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        Console.WriteLine($"[GetByIdAsync] User {user?.Email} CompanyIds: {string.Join(", ", user?.CompanyIds ?? new())}");
        return user;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        Console.WriteLine($"[GetByEmailAsync] User {user?.Email} CompanyIds: {string.Join(", ", user?.CompanyIds ?? new())}");
        return user;
    }


    public async Task<User> AddAsync(User user)
    {
        if (user.CompanyIds == null)
        {
            user.CompanyIds = new List<Guid>();
        }

        if (!string.IsNullOrWhiteSpace(user.Password))
        {
            user.Password = _passwordHasher.HashPassword(user.Password);
        }

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        return user;
    }


    public async Task<User?> UpdateAsync(Guid id, User user)
    {
        var existingUser = await _context.Users.FindAsync(id);
        if (existingUser == null) return null;

        existingUser.FirstName = !string.IsNullOrWhiteSpace(user.FirstName) ? user.FirstName : existingUser.FirstName;
        existingUser.LastName = !string.IsNullOrWhiteSpace(user.LastName) ? user.LastName : existingUser.LastName;
        existingUser.Email = !string.IsNullOrWhiteSpace(user.Email) ? user.Email : existingUser.Email;
        existingUser.Phone = string.IsNullOrWhiteSpace(user.Phone) ? null : user.Phone.Trim();
        existingUser.VerificationTmpPass = !string.IsNullOrWhiteSpace(user.VerificationTmpPass) ? user.VerificationTmpPass : existingUser.VerificationTmpPass;
        existingUser.IsActive = user.IsActive;
        existingUser.Role = user.Role;
        existingUser.MaxSubscriptionDate = user.MaxSubscriptionDate ?? existingUser.MaxSubscriptionDate;

        if (!string.IsNullOrWhiteSpace(user.Password))
        {
            if (!user.Password.StartsWith("$argon2") && !_passwordHasher.VerifyPassword(user.Password, existingUser.Password))
            {
                existingUser.Password = _passwordHasher.HashPassword(user.Password);
            }
        }

        if (user.Reminders != null && user.Reminders.Any())
        {
            existingUser.Reminders = user.Reminders;
        }

        existingUser.CompanyIds = user.CompanyIds != null && user.CompanyIds.Any() ? user.CompanyIds : existingUser.CompanyIds;

        await _context.SaveChangesAsync();
        return existingUser;
    }



    public async Task<bool> DeleteAsync(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<User> RegisterAsync(User user)
    {
        user.Password = _passwordHasher.HashPassword(user.Password);
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User?> ValidateUserAsync(string email, string password)
    {
        var user = await GetByEmailAsync(email);

        if (user == null || !_passwordHasher.VerifyPassword(password, user.Password))
        {
            return null;
        }
        return user;
    }

}
