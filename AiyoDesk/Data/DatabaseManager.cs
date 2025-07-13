
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
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

    public async Task SaveOpenWebUIToken(string token)
    {
        SystemSetting exSetting = GetSystemSetting();
        exSetting.OpenWebUIToken = token;
        dbContext.SystemSettings.Update(exSetting);
        await dbContext.SaveChangesAsync();
    }
    public async Task SaveBackendUseGPU(bool useGpu)
    {
        SystemSetting exSetting = GetSystemSetting();
        exSetting.BackendUseGPU = useGpu;
        dbContext.SystemSettings.Update(exSetting);
        await dbContext.SaveChangesAsync();
    }
    public async Task SavePassPackageCheck()
    {
        SystemSetting exSetting = GetSystemSetting();
        exSetting.PassPackageCheck = true;
        dbContext.SystemSettings.Update(exSetting);
        await dbContext.SaveChangesAsync();
    }
    public async Task SaveSystemSetting(SystemSetting systemSetting)
    {
        SystemSetting exSetting = GetSystemSetting();
        dbContext.SystemSettings.Update(systemSetting);
        await dbContext.SaveChangesAsync();
    }
    public SystemSetting GetSystemSetting()
    {
        SystemSetting systemSetting = null!;
        if (dbContext.SystemSettings.Count() <= 0)
        {
            systemSetting = new SystemSetting();
            systemSetting = dbContext.SystemSettings.Add(systemSetting).Entity;
            dbContext.SaveChanges();
        }
        else
        {
            systemSetting = dbContext.SystemSettings.First();
        }
        return systemSetting;
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
