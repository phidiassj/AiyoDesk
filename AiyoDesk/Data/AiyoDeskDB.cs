
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AiyoDesk.Data;

public class AiyoDeskDB : DbContext
{
    public DbSet<PackageSetting> PackageSettings { get; set; }
    public DbSet<SystemSetting> SystemSettings { get; set; }

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

[PrimaryKey(nameof(Id))]
public class SystemSetting
{
    [Key]
    public int Id { get; set; }
    public bool AutoRunAtStartup { get; set; }
    public bool MinToSystemTray { get; set; }
    public bool PassPackageCheck { get; set; }
    public bool BackendUseGPU { get; set; }
    public string? DefaultModelName { get; set; }
    public string? DefaultModelSubDir { get; set; }
}