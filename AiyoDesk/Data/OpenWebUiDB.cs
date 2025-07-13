using System;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiyoDesk.Data;

public class OpenWebUiDB : DbContext
{
    public DbSet<ConfigEntity> Configs => Set<ConfigEntity>();
    public DbSet<UserEntity> Users => Set<UserEntity>();

    private readonly string _dbPath;

    public OpenWebUiDB(string dbPath)
    {
        _dbPath = dbPath;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={_dbPath}");
}

[Table("config")]
public class ConfigEntity
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("data")]
    public string DataJson { get; set; } = "";

    [Column("version")]
    public int Version { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}

[Table("user")]
public class UserEntity
{
    [Key]
    [Column("id")]
    public string Id { get; set; } = default!;
    [Column("name")]
    public string Name { get; set; } = default!;
    [Column("email")]
    public string EMail { get; set; } = default!;
    [Column("role")]
    public string Role { get; set; } = default!;
    [Column("profile_image_url")]
    public string ImageUrl { get; set; } = default!;
    [Column("api_key")]
    public string? ApiKey { get; set; }
    [Column("created_at")]
    public DateTime Created { get; set; }
    [Column("updated_at")]
    public DateTime Updated { get; set; }
    [Column("last_active_at")]
    public DateTime LastAct { get; set; }
    [Column("settings")]
    public string? Settings { get; set; }
    [Column("info")]
    public string? Info { get; set; }
    [Column("oauth_sub")]
    public string? AauthSub { get; set; }

}
