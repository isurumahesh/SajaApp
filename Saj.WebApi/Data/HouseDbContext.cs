using Microsoft.EntityFrameworkCore;

public class HouseDbContext : DbContext
{
    public HouseDbContext(DbContextOptions<HouseDbContext> o) :
    base(o)
    {

    }
    public DbSet<HouseEntity> Houses => Set<HouseEntity>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        options.UseSqlite($"Data Source={Path.Join(path, "houses3.db")}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        SeedData.Seed(modelBuilder);
    }
}