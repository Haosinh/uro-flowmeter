using Microsoft.EntityFrameworkCore;
using UroMeter.DataAccess.Models;

namespace UroMeter.DataAccess;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    public DbSet<Record> Records { get; set; }

    public DbSet<RecordData> RecordDatas { get; set; }
}
