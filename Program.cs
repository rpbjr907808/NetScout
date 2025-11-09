using Microsoft.EntityFrameworkCore;
using NetScout.Data;
using NetScout.Services;

namespace NetScout;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("╔════════════════════════════════════════╗");
        Console.WriteLine("║         NetScout v1.0                  ║");
        Console.WriteLine("║   Network Scanner & Device Manager     ║");
        Console.WriteLine("╚════════════════════════════════════════╝\n");

        // Initialize database
        using var context = new NetworkDbContext();
        await context.Database.EnsureCreatedAsync();
        
        var scanner = new NetworkScanner();
        var repository = new DeviceRepository(context);

        while (true)
        {
            Console.WriteLine("\n┌─ Main Menu ────────────────────────────┐");
            Console.WriteLine("│ 1. Scan Network                        │");
            Console.WriteLine("│ 2. Scan Single Host                    │");
            Console.WriteLine("│ 3. View All Devices                    │");
            Console.WriteLine("│ 4. View Device Details                 │");
            Console.WriteLine("│ 5. Add Notes to Device                 │");
            Console.WriteLine("│ 6. Delete Device                       │");
            Console.WriteLine("│ 7. Export to CSV                       │");
            Console.WriteLine("│ 8. Exit                                │");
            Console.Write("\nSelect option: ");

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await ScanNetworkAsync(scanner, repository);
                    break;
                case "2":
                    await ScanSingleHostAsync(scanner, repository);
                    break;
                case "3":
                    await ViewAllDevicesAsync(repository);
                    break;
                case "4":
                    await ViewDeviceDetailsAsync(repository);
                    break;
                case "5":
                    await AddDeviceNotesAsync(repository);
                    break;
                case "6":
                    await DeleteDeviceAsync(repository);
                    break;
                case "7":
                    await ExportToCsvAsync(repository);
                    break;
                case "8":
                    Console.WriteLine("\nExiting Netscout. Goodbye!");
                    return;
            }
        }
    }

    static async Task ScanNetworkAsync(NetworkScanner scanner, DeviceRepository repository)
    {
        Console.Write("\nEnter network prefix (e.g., 192.168.1): ");
        var networkPrefix = Console.ReadLine();
        
        if (string.IsNullOrEmpty(networkPrefix))
        {
            Console.WriteLine("[!] Invalid network prefix.");
            return;
        }

        Console.Write("Enter start host (default 1): ");
        var startInput = Console.ReadLine();
        int startHost = string.IsNullOrEmpty(startInput) ? 1 : int.Parse(startInput);

        Console.Write("Enter end host (default 254): ");
        var endInput = Console.ReadLine();
        int endHost = string.IsNullOrEmpty(endInput) ? 254 : int.Parse(endInput);

        Console.WriteLine();
        var activeHosts = await scanner.PingSweepAsync(networkPrefix, startHost, endHost);

        Console.WriteLine("\nSaving devices to database...");
        foreach (var ip in activeHosts)
        {
            await repository.AddOrUpdateDeviceAsync(ip, "Online");
        }

        Console.WriteLine($"\n[✓] Scan complete! {activeHosts.Count} devices saved to database.");
        
        Console.Write("\nWould you like to perform a port scan on discovered devices? (y/n): ");
        if (Console.ReadLine()?.ToLower() == "y")
        {
            foreach (var ip in activeHosts)
            {
                Console.WriteLine($"\nScanning ports on {ip}...");
                var device = await repository.GetDeviceByIpAsync(ip);
                if (device != null)
                {
                    var hostName = await scanner.GetHostNameAsync(ip);
                    var openPorts = await scanner.ScanPortsAsync(ip);
                    await repository.UpdateDeviceInfoAsync(device.Id, hostName, openPorts);
                }
            }
            Console.WriteLine("\n[✓] Port scanning complete!");
        }
    }

    static async Task ScanSingleHostAsync(NetworkScanner scanner, DeviceRepository repository)
    {
        Console.Write("\nEnter IP address: ");
        var ipAddress = Console.ReadLine();

        if (string.IsNullOrEmpty(ipAddress))
        {
            Console.WriteLine("[!] Invalid IP address.");
            return;
        }

        Console.WriteLine($"\nPinging {ipAddress}...");
        var isOnline = await scanner.PingHostAsync(ipAddress);

        if (!isOnline)
        {
            Console.WriteLine($"[!] Host {ipAddress} is not responding.");
            await repository.AddOrUpdateDeviceAsync(ipAddress, "Offline");
            return;
        }

        Console.WriteLine($"[✓] Host {ipAddress} is online!");
        
        var device = await repository.AddOrUpdateDeviceAsync(ipAddress, "Online");
        var hostName = await scanner.GetHostNameAsync(ipAddress);
        
        Console.WriteLine($"Hostname: {hostName ?? "Unknown"}");
        
        Console.Write("\nScan ports? (y/n): ");
        if (Console.ReadLine()?.ToLower() == "y")
        {
            var openPorts = await scanner.ScanPortsAsync(ipAddress);
            await repository.UpdateDeviceInfoAsync(device.Id, hostName, openPorts);
            Console.WriteLine($"\n[✓] Found {openPorts.Count} open ports.");
        }
    }

    static async Task ViewAllDevicesAsync(DeviceRepository repository)
    {
        var devices = await repository.GetAllDevicesAsync();

        if (devices.Count == 0)
        {
            Console.WriteLine("\n[!] No devices in database. Run a scan first.");
            return;
        }

        Console.WriteLine($"\n┌─ Discovered Devices ({devices.Count}) ────────────────────────────────────────────┐");
        Console.WriteLine("│ IP Address      │ Hostname           │ Status  │ Open Ports │ Last Seen          │");
        Console.WriteLine("├─────────────────┼────────────────────┼─────────┼────────────┼────────────────────┤");

        foreach (var device in devices)
        {
            var hostname = device.HostName ?? "Unknown";
            if (hostname.Length > 18) hostname = hostname.Substring(0, 15) + "...";
            
            var lastSeen = device.LastSeen?.ToString("MM/dd/yy HH:mm") ?? "Never";
            var portCount = device.OpenPorts.Count.ToString();

            Console.WriteLine($"│ {device.IpAddress,-15} │ {hostname,-18} │ {device.Status,-7} │ {portCount,-10} │ {lastSeen,-18} │");
        }
        Console.WriteLine("└─────────────────┴────────────────────┴─────────┴────────────┴────────────────────┘");
    }

    static async Task ViewDeviceDetailsAsync(DeviceRepository repository)
    {
        Console.Write("\nEnter IP address: ");
        var ipAddress = Console.ReadLine();

        if (string.IsNullOrEmpty(ipAddress))
        {
            Console.WriteLine("[!] Invalid IP address.");
            return;
        }

        var device = await repository.GetDeviceByIpAsync(ipAddress);

        if (device == null)
        {
            Console.WriteLine($"[!] Device {ipAddress} not found in database.");
            return;
        }

        Console.WriteLine($"\n┌─ Device Details ───────────────────────────────┐");
        Console.WriteLine($"│ IP Address:  {device.IpAddress}");
        Console.WriteLine($"│ Hostname:    {device.HostName ?? "Unknown"}");
        Console.WriteLine($"│ MAC Address: {device.MacAddress ?? "Unknown"}");
        Console.WriteLine($"│ Status:      {device.Status}");
        Console.WriteLine($"│ Device Type: {device.DeviceType ?? "Unknown"}");
        Console.WriteLine($"│ First Seen:  {device.FirstSeen?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Unknown"}");
        Console.WriteLine($"│ Last Seen:   {device.LastSeen?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Unknown"}");
        Console.WriteLine($"│ Notes:       {device.Notes ?? "None"}");
        Console.WriteLine($"└────────────────────────────────────────────────┘");

        if (device.OpenPorts.Any())
        {
            Console.WriteLine($"\n┌─ Open Ports ({device.OpenPorts.Count}) ─────────────────────────────┐");
            Console.WriteLine("│ Port   │ Service      │ Protocol │ Discovered At       │");
            Console.WriteLine("├────────┼──────────────┼──────────┼─────────────────────┤");

            foreach (var port in device.OpenPorts.OrderBy(p => p.PortNumber))
            {
                Console.WriteLine($"│ {port.PortNumber,-6} │ {port.ServiceName,-12} │ {port.Protocol,-8} │ {port.DiscoveredAt:MM/dd/yy HH:mm:ss} │");
            }
            Console.WriteLine("└────────┴──────────────┴──────────┴─────────────────────┘");
        }
    }

    static async Task AddDeviceNotesAsync(DeviceRepository repository)
    {
        Console.Write("\nEnter IP address: ");
        var ipAddress = Console.ReadLine();

        if (string.IsNullOrEmpty(ipAddress))
        {
            Console.WriteLine("[!] Invalid IP address.");
            return;
        }

        Console.Write("Enter notes: ");
        var notes = Console.ReadLine();

        await repository.UpdateDeviceNotesAsync(ipAddress, notes ?? "");
        Console.WriteLine("[✓] Notes updated successfully.");
    }

    static async Task DeleteDeviceAsync(DeviceRepository repository)
    {
        Console.Write("\nEnter IP address: ");
        var ipAddress = Console.ReadLine();

        if (string.IsNullOrEmpty(ipAddress))
        {
            Console.WriteLine("[!] Invalid IP address.");
            return;
        }

        Console.Write($"Are you sure you want to delete {ipAddress}? (y/n): ");
        if (Console.ReadLine()?.ToLower() == "y")
        {
            await repository.DeleteDeviceAsync(ipAddress);
            Console.WriteLine("[✓] Device deleted successfully.");
        }
    }

    static async Task ExportToCsvAsync(DeviceRepository repository)
    {
        var devices = await repository.GetAllDevicesAsync();
        
        if (devices.Count == 0)
        {
            Console.WriteLine("\n[!] No devices to export. Run a scan first.");
            return;
        }
        
        Console.Write("\nEnter filename (without .csv): ");
        var filename = Console.ReadLine();
        
        if (string.IsNullOrEmpty(filename))
        {
            filename = $"netscout_export_{DateTime.Now:yyyyMMdd_HHmmss}";
        }
        
        var filepath = $"{filename}.csv";
        
        var exportService = new ExportService();
        await exportService.ExportToCsvAsync(devices, filepath);
        
        Console.WriteLine($"[✓] File saved! You can open it with Excel or any spreadsheet app.");
    }
}