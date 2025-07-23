using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace AiyoDesk.LocalHost;

public class LocalIpAddressHelper
{
    /// <summary>
    /// 取得所有本機 IP 位址（包含 IPv4 和 IPv6）
    /// </summary>
    public static List<IPAddress> GetAllLocalIPAddresses()
    {
        List<IPAddress> ipList = new List<IPAddress>();
        string hostName = Dns.GetHostName();

        IPAddress[] addresses = Dns.GetHostAddresses(hostName);
        foreach (var ip in addresses)
        {
            // 排除回送位址（例如 127.0.0.1, ::1）
            if (!IPAddress.IsLoopback(ip))
            {
                ipList.Add(ip);
            }
        }

        return ipList;
    }

    /// <summary>
    /// 取得所有本機 IPv4 位址
    /// </summary>
    public static List<IPAddress> GetAllLocalIPv4Addresses()
    {
        List<IPAddress> ipv4List = new List<IPAddress>();
        foreach (var ip in GetAllLocalIPAddresses())
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                ipv4List.Add(ip);
            }
        }
        return ipv4List;
    }

    /// <summary>
    /// 取得所有本機 IPv6 位址
    /// </summary>
    public static List<IPAddress> GetAllLocalIPv6Addresses()
    {
        List<IPAddress> ipv6List = new List<IPAddress>();
        foreach (var ip in GetAllLocalIPAddresses())
        {
            if (ip.AddressFamily == AddressFamily.InterNetworkV6)
            {
                ipv6List.Add(ip);
            }
        }
        return ipv6List;
    }

    public static async Task<bool> IsPortRunningAsync(int port, int timeoutMs = 3000)
    {
        using var cts = new CancellationTokenSource(timeoutMs);
        using var client = new HttpClient { Timeout = TimeSpan.FromMilliseconds(timeoutMs) };
        try
        {
            var url = $"http://127.0.0.1:{port}/";
            using var resp = await client.GetAsync(url, cts.Token);
            return resp.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
