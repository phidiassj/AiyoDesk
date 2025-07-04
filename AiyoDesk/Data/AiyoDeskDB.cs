
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AiyoDesk.Data;

public class AiyoDeskDB : DbContext
{
    public DbSet<PackageSetting> PackageSettings { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=AiyoDesk.db");
    }
}

[PrimaryKey(nameof(Id))]
public class PackageSetting
{
    [Key]
    public int Id { get; set; }
    public string PackageName { get; set; } = null!;
    public int LocalPort { get; set; }
    public string? ActivateCommand { get; set; }
    public bool AutoActivate { get; set; }
}