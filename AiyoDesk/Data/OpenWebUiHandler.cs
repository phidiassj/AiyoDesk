using System.Linq;
using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.ClientModel;
using AiyoDesk.CommanandTools;
using System.IO;

namespace AiyoDesk.Data;

public class OpenWebUiHandler
{
    private readonly OpenWebUiDB _db;

    public static bool IsLocalDBExists()
    {
        string dbFile = Path.Combine(CommandLineExecutor.GetPackageRootPath(), "conda", "envs", "aiyodesk", "Lib", "site-packages", "open_webui", "data", "webui.db");
        return File.Exists(dbFile);
    }

    public OpenWebUiHandler(OpenWebUiDB db)
    {
        _db = db;
    }

    public void AddOrUpdateConnection(string url)
    {
        var config = _db.Configs.FirstOrDefault(x => x.DataJson.Contains("api.openai.com") && x.DataJson.Contains("127.0.0.1:"));
        string dataString = connectionjson.Replace("%EndpointUrl%", url);
        if (config == null)
        {
            config = new ConfigEntity();
            config.DataJson = dataString;
            config.Version = 0;
            config.CreatedAt = DateTime.UtcNow;
            config.UpdatedAt = DateTime.UtcNow;
            _ = _db.Configs.Add(config);
            _db.SaveChanges();
        }
        else if (!config.DataJson.Contains(url))
        {
            config.DataJson = dataString;
            config.UpdatedAt = DateTime.UtcNow;
            _db.Entry(config).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            _db.SaveChanges();
        }
    }

    private static string connectionjson = """{"version": 0, "ui": {"enable_signup": false}, "openai": {"enable": true, "api_base_urls": ["https://api.openai.com/v1", "%EndpointUrl%"], "api_keys": ["", ""], "api_configs": {"0": {}, "1": {"enable": true, "tags": [], "prefix_id": "", "model_ids": [], "connection_type": "external"}}}}""";
}

