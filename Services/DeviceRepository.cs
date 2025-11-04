using Microsoft.EntityFrameworkCore;
using NetScout.Data;
using NetScout.Models;

namespace NetScout.Services;

public class DeviceRepository
{
    private readonly NetworkDbContext _context;

    public DeviceRepository(NetworkDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Adds or updates a device in the database
    /// </summary>
    public async Task<NetworkDevice> AddOrUpdateDeviceAsync(string ipAddress, string status = "Online")
    {
        var device = await _context.NetworkDevices
            .Include(d => d.OpenPorts)
            .FirstOrDefaultAsync(d => d.IpAddress == ipAddress);

        if (device == null)
        {
            device = new NetworkDevice
            {
                IpAddress = ipAddress,
                Status = status,
                FirstSeen = DateTime.UtcNow,
                LastSeen = DateTime.UtcNow
            };
            _context.NetworkDevices.Add(device);
        }
        else
        {
            device.Status = status;
            device.LastSeen = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return device;
    }

    /// <summary>
    /// Updates device with scan results
    /// </summary>
    public async Task UpdateDeviceInfoAsync(int deviceId, string? hostName, List<int> openPorts)
    {
        var device = await _context.NetworkDevices
            .Include(d => d.OpenPorts)
            .FirstOrDefaultAsync(d => d.Id == deviceId);

        if (device == null) return;

        if (!string.IsNullOrEmpty(hostName))
        {
            device.HostName = hostName;
        }

        // Remove old port records
        _context.OpenPorts.RemoveRange(device.OpenPorts);

        // Add new port records
        foreach (var port in openPorts)
        {
            device.OpenPorts.Add(new OpenPort
            {
                PortNumber = port,
                ServiceName = GetServiceName(port),
                Protocol = "TCP",
                DiscoveredAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Gets all devices from the database
    /// </summary>
    public async Task<List<NetworkDevice>> GetAllDevicesAsync()
    {
        return await _context.NetworkDevices
            .Include(d => d.OpenPorts)
            .OrderBy(d => d.IpAddress)
            .ToListAsync();
    }

    /// <summary>
    /// Gets device by IP address
    /// </summary>
    public async Task<NetworkDevice?> GetDeviceByIpAsync(string ipAddress)
    {
        return await _context.NetworkDevices
            .Include(d => d.OpenPorts)
            .FirstOrDefaultAsync(d => d.IpAddress == ipAddress);
    }

    /// <summary>
    /// Deletes a device from the database
    /// </summary>
    public async Task DeleteDeviceAsync(string ipAddress)
    {
        var device = await _context.NetworkDevices
            .FirstOrDefaultAsync(d => d.IpAddress == ipAddress);

        if (device != null)
        {
            _context.NetworkDevices.Remove(device);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Updates device notes
    /// </summary>
    public async Task UpdateDeviceNotesAsync(string ipAddress, string notes)
    {
        var device = await _context.NetworkDevices
            .FirstOrDefaultAsync(d => d.IpAddress == ipAddress);

        if (device != null)
        {
            device.Notes = notes;
            await _context.SaveChangesAsync();
        }
    }

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
}
