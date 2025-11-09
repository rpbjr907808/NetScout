using System.Text;
using NetScout.Models;

namespace NetScout.Services;

public class ExportService
{
    /// <summary>
    /// Exports devices to CSV format
    /// </summary>
    public async Task ExportToCsvAsync(List<NetworkDevice> devices, string filePath)
    {
        var csv = new StringBuilder();
        
        // Header row
        csv.AppendLine("IP Address,Hostname,Status,Open Ports,First Seen,Last Seen,Notes");
        
        // Data rows
        foreach (var device in devices)
        {
            var hostname = device.HostName ?? "Unknown";
            var openPorts = string.Join(";", device.OpenPorts.Select(p => p.PortNumber));
            var firstSeen = device.FirstSeen?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Unknown";
            var lastSeen = device.LastSeen?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Unknown";
            var notes = device.Notes?.Replace(",", ";") ?? ""; // Replace commas to avoid CSV issues
            
            csv.AppendLine($"{device.IpAddress},{hostname},{device.Status},\"{openPorts}\",{firstSeen},{lastSeen},{notes}");
        }
        
        await File.WriteAllTextAsync(filePath, csv.ToString());
        Console.WriteLine($"\n[✓] Exported {devices.Count} devices to {filePath}");
    }
}