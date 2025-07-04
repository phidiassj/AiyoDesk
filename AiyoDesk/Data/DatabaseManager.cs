
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace AiyoDesk.Data;

public class DatabaseManager : IDisposable
{
    public async Task<PackageSetting?> GetPackageSetting(string PackageName)
    {
        var result = await dbContext.PackageSettings.FirstOrDefaultAsync(x => x.PackageName == PackageName);
        return result;
    }
    public async Task<PackageSetting?> GetPackageSetting(int PackageId)
    {
        var result = await dbContext.PackageSettings.FirstOrDefaultAsync(x => x.Id == PackageId);
        return result;
    }
    public async Task<PackageSetting> SavePackageSetting(PackageSetting packageSetting)
    {
        PackageSetting result = null!;
        if (packageSetting.Id <= 0)
        {
            var retObj = await dbContext.PackageSettings.AddAsync(packageSetting);
            result = retObj.Entity;
        }
        else
        {
            var existing = dbContext.PackageSettings.Update(packageSetting);
            result = existing.Entity;
        }
        await dbContext.SaveChangesAsync();
        return result;
    }

    private AiyoDeskDB dbContext = null!;

    public DatabaseManager()
    {
        using (var db = new AiyoDeskDB())
        {
            db.Database.EnsureCreated(); // 確保資料庫存在
        }
        dbContext = new AiyoDeskDB();
    }

    public void Dispose()
    {
        try
        {
            dbContext.Dispose();
        }
        catch { }
    }
}
