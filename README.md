# NetScout

A network scanning and device management tool built in C#
This is a personal project to enhance  my programming skills and also to gain networking skills 

## 🚀 Features

- **Network Discovery**: Perform ping sweeps to discover active hosts on your network
- **Port Scanning**: Scan common ports on discovered devices to identify running services
- **Device Inventory**: Maintain a SQLite database of all discovered network devices
- **Host Information**: Resolve hostnames and track device status over time
- **Device Management**: Add notes, view details, and manage your network inventory
- **Real-time Scanning**: Asynchronous operations for fast network analysis

## 🛠️ Technology Stack

- **Language**: C# / .NET 8.0
- **Database**: SQLite with Entity Framework Core
- **Network Libraries**: System.Net.NetworkInformation, System.Net.Sockets

## 📋 Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- VS Code or any text editor
- Terminal/Command Prompt

## 🔧 Installation

1. Clone this repository:
```bash
git clone https://github.com/yourusername/NetScout.git
cd NetScout
```

2. Restore dependencies:
```bash
dotnet restore
```

3. Build the project:
```bash
dotnet build
```

4. Run the application:
```bash
dotnet run
```

## 📖 Usage

### Main Menu Options

1. **Scan Network**: Discover all active devices on a network range
   - Enter network prefix (e.g., `192.168.1`)
   - Specify host range (default: 1-254)
   - Optionally perform port scanning on discovered devices

2. **Scan Single Host**: Deep scan a specific IP address
   - Check if host is online
   - Resolve hostname
   - Scan for open ports

3. **View All Devices**: Display inventory of all discovered devices
   - IP addresses
   - Hostnames
   - Online/Offline status
   - Number of open ports
   - Last seen timestamp

4. **View Device Details**: Get comprehensive information about a specific device
   - Full device information
   - List of all open ports with service names
   - Discovery timestamps

5. **Add Notes to Device**: Document information about network devices
   - Add administrative notes
   - Track device purpose or issues

6. **Delete Device**: Remove a device from the inventory

### Example Workflow

```bash
# Start NetScout
dotnet run

# Scan your local network
Select option: 1
Enter network prefix: 192.168.1
Enter start host: 1
Enter end host: 254

# View discovered devices
Select option: 3

# Get details on a specific device
Select option: 4
Enter IP address: 192.168.1.1
```

## 🔍 Scanned Ports

NetScout scans the following common ports by default:

| Port | Service |
|------|---------|
| 21 | FTP |
| 22 | SSH |
| 23 | Telnet |
| 25 | SMTP |
| 53 | DNS |
| 80 | HTTP |
| 110 | POP3 |
| 143 | IMAP |
| 443 | HTTPS |
| 445 | SMB |
| 3306 | MySQL |
| 3389 | RDP |
| 5432 | PostgreSQL |
| 8080 | HTTP-Alt |

## 📁 Project Structure

```
NetScout/
├── Models/
│   └── NetworkDevice.cs      # Data models for devices and ports
├── Services/
│   ├── NetworkScanner.cs     # Network scanning logic
│   └── DeviceRepository.cs   # Database operations
├── Data/
│   └── NetworkDbContext.cs   # Entity Framework context
├── Program.cs                # Main application entry point
└── NetScout.csproj          # Project configuration
```

## 🗄️ Database Schema

### NetworkDevice Table
- Id (Primary Key)
- IpAddress (Unique)
- HostName
- MacAddress
- Status (Online/Offline/Unknown)
- FirstSeen
- LastSeen
- DeviceType
- Manufacturer
- Notes

### OpenPort Table
- Id (Primary Key)
- NetworkDeviceId (Foreign Key)
- PortNumber
- ServiceName
- Protocol
- DiscoveredAt

## 🎯 Use Cases

- **Network Administrators**: Maintain an up-to-date inventory of network devices
- **Security Auditing**: Identify unauthorized devices and open ports
- **IT Asset Management**: Track devices across multiple networks
- **Penetration Testing**: Reconnaissance phase of security assessments
- **Home Network Management**: Monitor devices on your home network

## ⚠️ Legal Disclaimer

**IMPORTANT**: Only use NetScout on networks you own or have explicit permission to scan. Unauthorized network scanning may be illegal in your jurisdiction. Always obtain proper authorization before scanning any network.

## 🛣️ Roadmap

- [ ] Export inventory to CSV/JSON
- [ ] Web-based dashboard
- [ ] SNMP device information gathering
- [ ] Network topology mapping
- [ ] Scheduled automatic scans
- [ ] Email alerts for new devices
- [ ] MAC address vendor lookup
- [ ] Advanced port scanning options (UDP, custom port ranges)
- [ ] Integration with vulnerability databases

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the project
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 👨‍💻 Author

Built with ☕ by RB

## 🙏 Acknowledgments

- Inspired by network administration tools like nmap and Angry IP Scanner
- Built as part of a career transition project from programming to  either network engineering or cloud engineering 
