using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using kriefTrackAiApi.Core.Models;
using kriefTrackAiApi.Core.Interfaces;
using kriefTrackAiApi.Infrastructure.Data;

namespace kriefTrackAiApi.Infrastructure.Repositories;

public class SmsRepository : ISmsRepository
{
    private readonly AppDbContext _context;

    public SmsRepository(AppDbContext context)
    {
        _context = context;
    }


    public async Task<Sms?> AddOrUpdateEntryAsync(string container, UserPhoneEntry entry)
    {
        var phoneDuplicates = entry.Phones
            .GroupBy(p => p)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (phoneDuplicates.Any())
        {
            throw new InvalidOperationException($"Duplicate phone numbers found: {string.Join(", ", phoneDuplicates)}");

        }
        var sms = await _context.Set<Sms>()
    .FirstOrDefaultAsync(s => s.Container == container);

        if (sms == null)
        {
            sms = new Sms
            {
                Id = Guid.NewGuid(),
                Container = container,
                UserPhones = new List<UserPhoneEntry>
{
    new UserPhoneEntry
    {
        UserId = entry.UserId,
        Phones = entry.Phones.ToList()
    }
}

            };
            await _context.Set<Sms>().AddAsync(sms);
        }
        else
        {
            var list = sms.UserPhones;
            var existingEntry = list.FirstOrDefault(e => e.UserId == entry.UserId);
            if (existingEntry != null)
            {
                existingEntry.Phones = entry.Phones;
            }
            else
            {
                list.Add(new UserPhoneEntry
                {
                    UserId = entry.UserId,
                    Phones = entry.Phones.ToList()
                });

            }
            sms.UserPhones = list;
        }

        await _context.SaveChangesAsync();

        return sms;
    }

    public async Task<bool> RemoveEntryAsync(string container, Guid userId)
    {
        var sms = await _context.Set<Sms>()
            .FirstOrDefaultAsync(s => s.Container == container);

        if (sms == null)
            return false;

        var list = sms.UserPhones;
        var removedCount = list.RemoveAll(e => e.UserId == userId);

        if (removedCount == 0)
            return false;

        if (!list.Any())
        {
            _context.Set<Sms>().Remove(sms);
        }
        else
        {
            sms.UserPhones = list;
        }

        await _context.SaveChangesAsync();
        return true;
    }


    public async Task<List<Sms>> GetAllByUserAsync(Guid userId)
    {
        var all = await _context.Set<Sms>().ToListAsync();

        var result = all
            .Where(s => s.UserPhones.Any(e => e.UserId == userId))
            .ToList();

        foreach (var sms in result)
        {
            sms.UserPhones = sms.UserPhones
                .Where(e => e.UserId == userId)
                .ToList();
        }

        return result;
    }
}
