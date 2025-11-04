# NetScout Architecture

## System Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                         USER INTERFACE                          │
│                         (Program.cs)                            │
│                                                                 │
│  ┌────────────┐  ┌────────────┐  ┌────────────┐  ┌──────────┐ │
│  │   Scan     │  │   View     │  │   Manage   │  │  Delete  │ │
│  │  Network   │  │  Devices   │  │   Notes    │  │  Device  │ │
│  └─────┬──────┘  └─────┬──────┘  └─────┬──────┘  └────┬─────┘ │
└────────┼───────────────┼───────────────┼──────────────┼────────┘
         │               │               │              │
         │               │               │              │
         ▼               ▼               ▼              ▼
┌─────────────────────────────────────────────────────────────────┐
│                        SERVICE LAYER                            │
├─────────────────────────────┬───────────────────────────────────┤
│                             │                                   │
│   ┌──────────────────────┐  │   ┌──────────────────────────┐   │
│   │  NetworkScanner.cs   │  │   │  DeviceRepository.cs     │   │
│   │                      │  │   │                          │   │
│   │  • PingSweepAsync()  │  │   │  • AddOrUpdateDevice()   │   │
│   │  • ScanPortsAsync()  │  │   │  • GetAllDevices()       │   │
│   │  • PingHostAsync()   │  │   │  • GetDeviceByIp()       │   │
│   │  • IsPortOpenAsync() │  │   │  • UpdateDeviceInfo()    │   │
│   │  • GetHostNameAsync()│  │   │  • DeleteDevice()        │   │
│   └───────────┬──────────┘  │   └───────────┬──────────────┘   │
└───────────────┼─────────────┴───────────────┼──────────────────┘
                │                             │
                │  Network                    │  Database
                │  Operations                 │  Operations
                │                             │
                ▼                             ▼
┌───────────────────────────┐   ┌─────────────────────────────────┐
│    NETWORK PROTOCOLS      │   │       DATA LAYER                │
│                           │   │                                 │
│  ┌──────────────────────┐ │   │  ┌──────────────────────────┐  │
│  │  System.Net.         │ │   │  │  NetworkDbContext.cs     │  │
│  │  NetworkInformation  │ │   │  │  (Entity Framework)      │  │
│  │                      │ │   │  │                          │  │
│  │  • Ping              │ │   │  │  DbSet<NetworkDevice>    │  │
│  │  • IPStatus          │ │   │  │  DbSet<OpenPort>         │  │
│  └──────────────────────┘ │   │  └────────────┬─────────────┘  │
│                           │   └────────────────┼────────────────┘
│  ┌──────────────────────┐ │                   │
│  │  System.Net.Sockets  │ │                   │
│  │                      │ │                   ▼
│  │  • TcpClient         │ │        ┌──────────────────────┐
│  │  • Socket            │ │        │   netscout.db        │
│  └──────────────────────┘ │        │   (SQLite Database)  │
│                           │        │                      │
│  ┌──────────────────────┐ │        │  Tables:             │
│  │  System.Net.Dns      │ │        │  • NetworkDevices    │
│  │                      │ │        │  • OpenPorts         │
│  │  • GetHostEntryAsync │ │        └──────────────────────┘
│  └──────────────────────┘ │
└───────────────────────────┘
```

## Data Flow

### 1. Network Scan Flow
```
User Input (IP Range)
    ↓
NetworkScanner.PingSweepAsync()
    ↓
For each IP: System.Net.NetworkInformation.Ping
    ↓
Active IPs collected
    ↓
DeviceRepository.AddOrUpdateDevice() for each
    ↓
SQLite Database (NetworkDevices table)
```

### 2. Port Scan Flow
```
Active Device IPs
    ↓
NetworkScanner.ScanPortsAsync()
    ↓
For each port: System.Net.Sockets.TcpClient.ConnectAsync()
    ↓
Open ports collected
    ↓
DeviceRepository.UpdateDeviceInfo()
    ↓
SQLite Database (OpenPorts table with FK to NetworkDevices)
```

### 3. Hostname Resolution Flow
```
IP Address
    ↓
NetworkScanner.GetHostNameAsync()
    ↓
System.Net.Dns.GetHostEntryAsync()
    ↓
Hostname returned
    ↓
Stored in NetworkDevice.HostName
```

## Database Schema

```
┌─────────────────────────────────────┐
│        NetworkDevices               │
├─────────────────────────────────────┤
│ PK  Id (int)                        │
│     IpAddress (string) [UNIQUE]     │
│     HostName (string)               │
│     MacAddress (string)             │
│     Status (string)                 │
│     FirstSeen (DateTime)            │
│     LastSeen (DateTime)             │
│     DeviceType (string)             │
│     Manufacturer (string)           │
│     Notes (string)                  │
└──────────────┬──────────────────────┘
               │ 1:Many
               │
┌──────────────▼──────────────────────┐
│           OpenPorts                 │
├─────────────────────────────────────┤
│ PK  Id (int)                        │
│ FK  NetworkDeviceId (int)           │
│     PortNumber (int)                │
│     ServiceName (string)            │
│     Protocol (string)               │
│     DiscoveredAt (DateTime)         │
└─────────────────────────────────────┘
```

## Component Responsibilities

### Program.cs (UI Layer)
- Display interactive menu
- Handle user input
- Orchestrate service calls
- Format and display results
- Manage application flow

### NetworkScanner.cs (Service Layer)
**Responsibility:** All network operations
- Ping hosts to check availability
- Scan TCP ports
- Resolve hostnames
- Handle network timeouts
- Concurrent scanning operations

### DeviceRepository.cs (Service Layer)
**Responsibility:** All database operations
- CRUD operations on devices
- Query device inventory
- Update scan results
- Manage device relationships
- Handle database transactions

### NetworkDbContext.cs (Data Layer)
**Responsibility:** Database configuration
- Define entity relationships
- Configure SQLite connection
- Set up indexes and constraints
- Manage migrations

### Models/NetworkDevice.cs (Data Layer)
**Responsibility:** Data structures
- Define device properties
- Define port properties
- Establish relationships
- Validation rules

## Key Design Patterns Used

### 1. Repository Pattern
`DeviceRepository.cs` abstracts database operations, making it easy to change databases later.

### 2. Separation of Concerns
- UI logic → Program.cs
- Business logic → Services/
- Data access → Data/
- Data models → Models/

### 3. Async/Await Pattern
All I/O operations are asynchronous for better performance.

### 4. Dependency Injection (Simple)
Services are instantiated and passed as dependencies.

## Technology Stack Details

```
┌─────────────────────────────────────────────────┐
│           .NET 8.0 Runtime                      │
├─────────────────────────────────────────────────┤
│                                                 │
│  NuGet Packages:                                │
│  • Microsoft.EntityFrameworkCore.Sqlite (8.0)   │
│  • Microsoft.EntityFrameworkCore.Design (8.0)   │
│  • System.Net.NetworkInformation (4.3.0)        │
│                                                 │
│  Built-in Libraries:                            │
│  • System.Net.Sockets                           │
│  • System.Net.Dns                               │
│  • System.Threading.Tasks                       │
│                                                 │
└─────────────────────────────────────────────────┘
```

## Performance Characteristics

**Ping Sweep (Class C /24):**
- Sequential: ~254 seconds (1 sec timeout × 254 hosts)
- With async/parallel: ~5-10 seconds
- Improvement: **~25-50x faster**

**Port Scanning:**
- 14 ports per host
- Sequential per host: ~14 seconds
- Concurrent port scanning: ~1-2 seconds per host
- Improvement: **~7-14x faster**

## Security Considerations

**Current Implementation:**
- Read-only network operations
- No authentication bypass attempts
- Respects network timeouts
- No packet injection

**Future Considerations:**
- Rate limiting to avoid DoS appearance
- User authentication for multi-user deployments
- Audit logging of scan activities
- Compliance with network scanning policies

## Scalability

**Current Limits:**
- SQLite: ~10,000 devices (sufficient for most SMB networks)
- Concurrent operations: Limited by system resources
- Single-threaded database writes

**Future Improvements:**
- PostgreSQL/MySQL for large-scale deployments
- Distributed scanning agents
- Result caching
- Batch operations

## Extension Points

Where to add new features:

1. **New Scan Types** → Add methods to `NetworkScanner.cs`
2. **New Device Properties** → Update `NetworkDevice.cs` model
3. **Export Formats** → Create new service in `Services/`
4. **UI Changes** → Modify `Program.cs` menu
5. **New Protocols** → Add protocol handlers to `NetworkScanner.cs`

---

This architecture is designed to be:
- ✅ **Modular** - Easy to modify individual components
- ✅ **Testable** - Services can be unit tested
- ✅ **Maintainable** - Clear separation of concerns
- ✅ **Extensible** - Easy to add new features
- ✅ **Professional** - Follows industry best practices
