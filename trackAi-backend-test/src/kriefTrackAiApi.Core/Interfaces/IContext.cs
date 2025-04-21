using Microsoft.EntityFrameworkCore;
using kriefTrackAiApi.Core.Models;
using System.Threading.Tasks;

namespace kriefTrackAiApi.Core.Interfaces;

public interface IContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<Sms> SmsMessages { get; set; }

    Task Save();
}
