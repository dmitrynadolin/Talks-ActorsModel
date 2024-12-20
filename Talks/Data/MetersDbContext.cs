using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Metrics;

namespace Talks.Data;

public class MetersDbContext : DbContext
{
    public MetersDbContext(DbContextOptions<MetersDbContext> options) : base(options)
    {
    }
    public DbSet<Measurement> Measurements { get; set; } = null!;

}