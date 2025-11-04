using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using NetScout.Models;

namespace NetScout.Services;

public class NetworkScanner
{
    private readonly int _timeout;
    private static readonly int[] CommonPorts = { 21, 22, 23, 25, 53, 80, 110, 143, 443, 445, 3306, 3389, 5432, 8080 };

    public NetworkScanner(int timeout = 1000)
    {
        _timeout = timeout;
    }

    /// <summary>
    /// Performs a ping sweep on the specified network range
    /// </summary>
    public async Task<List<string>> PingSweepAsync(string networkPrefix, int startHost = 1, int endHost = 254)
    {
        Console.WriteLine($"Starting ping sweep on {networkPrefix}.{startHost}-{endHost}...");
        var activeHosts = new List<string>();
        var tasks = new List<Task>();

        for (int i = startHost; i <= endHost; i++)
        {
            string ip = $"{networkPrefix}.{i}";
            tasks.Add(Task.Run(async () =>
            {
                if (await PingHostAsync(ip))
                {
                    lock (activeHosts)
                    {
                        activeHosts.Add(ip);
                        Console.WriteLine($"[+] Found active host: {ip}");
                    }
                }
            }));
        }

        await Task.WhenAll(tasks);
        Console.WriteLine($"\nScan complete. Found {activeHosts.Count} active hosts.");
        return activeHosts;
    }

    /// <summary>
    /// Pings a single host to check if it's online
    /// </summary>
    public async Task<bool> PingHostAsync(string ipAddress)
    {
        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(ipAddress, _timeout);
            return reply.Status == IPStatus.Success;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Scans common ports on a target host
    /// </summary>
    public async Task<List<int>> ScanPortsAsync(string ipAddress, int[]? ports = null)
    {
        ports ??= CommonPorts;
        var openPorts = new List<int>();
        
        Console.WriteLine($"Scanning {ports.Length} ports on {ipAddress}...");

        var tasks = ports.Select(async port =>
        {
            if (await IsPortOpenAsync(ipAddress, port))
            {
                lock (openPorts)
                {
                    openPorts.Add(port);
                    Console.WriteLine($"[+] Port {port} is open on {ipAddress} ({GetServiceName(port)})");
                }
            }
        });

        await Task.WhenAll(tasks);
        return openPorts;
    }

    /// <summary>
    /// Checks if a specific port is open on a host
    /// </summary>
    private async Task<bool> IsPortOpenAsync(string ipAddress, int port)
    {
        try
        {
            using var client = new TcpClient();
            var connectTask = client.ConnectAsync(ipAddress, port);
            var timeoutTask = Task.Delay(_timeout);

            var completedTask = await Task.WhenAny(connectTask, timeoutTask);
            
            if (completedTask == connectTask && client.Connected)
            {
                return true;
            }
            
            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the common service name for a port number
    /// </summary>
    private string GetServiceName(int port)
    {
        return port switch
        {
            21 => "FTP",
            22 => "SSH",
            23 => "Telnet",
            25 => "SMTP",
            53 => "DNS",
            80 => "HTTP",
            110 => "POP3",
            143 => "IMAP",
            443 => "HTTPS",
            445 => "SMB",
            3306 => "MySQL",
            3389 => "RDP",
            5432 => "PostgreSQL",
            8080 => "HTTP-Alt",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Attempts to resolve hostname from IP
    /// </summary>
    public async Task<string?> GetHostNameAsync(string ipAddress)
    {
        try
        {
            var hostEntry = await Dns.GetHostEntryAsync(ipAddress);
            return hostEntry.HostName;
        }
        catch
        {
            return null;
        }
    }
}
