using System;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiyoDesk.Data;

public class OpenWebUiDB : DbContext
{
    public DbSet<ConfigEntity> Configs => Set<ConfigEntity>();

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