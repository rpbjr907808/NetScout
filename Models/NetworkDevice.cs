namespace NetScout.Models;

public class NetworkDevice
{
    public int Id { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string? HostName { get; set; }
    public string? MacAddress { get; set; }
    public string Status { get; set; } = "Unknown"; // Online, Offline, Unknown
    public DateTime? FirstSeen { get; set; }
    public DateTime? LastSeen { get; set; }
    public string? DeviceType { get; set; } // Router, Switch, Workstation, Server, etc.
    public string? Manufacturer { get; set; }
    public string? Notes { get; set; }
    public List<OpenPort> OpenPorts { get; set; } = new();
}

public class OpenPort
{
    public int Id { get; set; }
    public int NetworkDeviceId { get; set; }
    public int PortNumber { get; set; }
    public string? ServiceName { get; set; }
    public string Protocol { get; set; } = "TCP";
    public DateTime DiscoveredAt { get; set; }
    public NetworkDevice? NetworkDevice { get; set; }
}
