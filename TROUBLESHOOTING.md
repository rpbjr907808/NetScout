# NetScout Troubleshooting Guide

Common issues and their solutions when running NetScout.

## Installation Issues

### "dotnet: command not found"

**Problem:** .NET SDK not installed or not in PATH

**Solution:**
1. Download .NET 8.0 SDK from https://dotnet.microsoft.com/download
2. Install and restart terminal
3. Verify: `dotnet --version`

### "The project file could not be loaded"

**Problem:** Wrong directory or corrupted .csproj file

**Solution:**
```bash
# Make sure you're in the NetScout directory
cd NetScout
ls NetScout.csproj  # Should show the file

# If file is corrupted, regenerate:
dotnet new console
# Then restore the original .csproj content
```

### "Package restore failed"

**Problem:** Network issues or NuGet configuration

**Solution:**
```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore again
dotnet restore

# If still failing, check internet connection
```

## Build Issues

### "CS0246: The type or namespace name '...' could not be found"

**Problem:** Missing using statements or package references

**Solution:**
```bash
# Restore packages
dotnet restore

# Clean and rebuild
dotnet clean
dotnet build
```

### "Database provider initialization error"

**Problem:** Entity Framework packages not installed

**Solution:**
```bash
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet restore
dotnet build
```

## Runtime Issues

### "Access Denied" or "Permission Error"

**Problem:** Insufficient privileges for network operations

**Solution:**

**Windows:**
- Run terminal as Administrator
- Right-click Command Prompt/PowerShell → "Run as administrator"

**Linux/Mac:**
```bash
sudo dotnet run
```

**Why?** Raw socket operations and ICMP require elevated privileges.

### "No devices found" during scan

**Problem:** Multiple possible causes

**Solution Checklist:**

1. **Check your network prefix:**
```bash
# Windows
ipconfig

# Linux/Mac
ifconfig
# or
ip addr show

# Look for your IP, e.g., 192.168.1.105
# Use prefix: 192.168.1
```

2. **Firewall blocking ICMP:**
- Windows: Allow app through Windows Defender Firewall
- Linux: Check `sudo iptables -L`
- Mac: System Preferences → Security & Privacy → Firewall

3. **Network doesn't respond to ping:**
Some networks block ICMP. Try scanning a single known device:
```bash
Select option: 2
Enter IP: [your router IP, usually .1 or .254]
```

4. **Wrong network range:**
Make sure you're scanning the correct subnet:
- Home networks: Usually 192.168.0.x or 192.168.1.x
- Corporate: Ask network admin

### Scan is too slow

**Problem:** Large network range with default timeout

**Solution:**

**Option 1 - Reduce scan range:**
Instead of scanning 1-254, try:
```
Start host: 1
End host: 50
```

**Option 2 - Reduce timeout (modify code):**
In `NetworkScanner.cs`, line 16:
```csharp
// Change from:
private readonly int _timeout;

// Constructor:
public NetworkScanner(int timeout = 1000)  // 1 second

// To:
public NetworkScanner(int timeout = 500)   // 0.5 seconds
```

**Option 3 - Scan fewer ports:**
In `NetworkScanner.cs`, modify `CommonPorts` array:
```csharp
// Scan only critical ports
private static readonly int[] CommonPorts = { 22, 80, 443 };
```

### "Database is locked" error

**Problem:** Another process accessing the database

**Solution:**
1. Close all other NetScout instances
2. Delete `netscout.db`, `netscout.db-shm`, `netscout.db-wal`
3. Restart application (will recreate database)

### Port scans find no open ports on devices you know have services

**Problem:** Firewall blocking or ports not in common list

**Solution:**

**Check if service is actually running:**
```bash
# Windows
netstat -an | findstr :80

# Linux/Mac
netstat -an | grep :80
```

**Scan custom ports (modify code):**
In `Program.cs`, find `ScanSingleHostAsync` and add:
```csharp
Console.Write("Enter ports to scan (comma-separated): ");
var portsInput = Console.ReadLine();
var customPorts = portsInput.Split(',').Select(int.Parse).ToArray();
var openPorts = await scanner.ScanPortsAsync(ipAddress, customPorts);
```

### "System.Net.Sockets.SocketException: Connection refused"

**Problem:** This is actually normal! It means the port is closed.

**Solution:** No action needed. The scanner catches this and marks port as closed.

### Hostname resolution returns "Unknown"

**Problem:** DNS not configured or device has no reverse DNS entry

**Solution:**
1. This is normal for many devices
2. Some networks don't have reverse DNS configured
3. You can manually add hostnames using option 5 (Add Notes)

## Database Issues

### "Cannot open database file"

**Problem:** File permissions or corrupted database

**Solution:**
```bash
# Check permissions
ls -la netscout.db

# If corrupted, delete and restart:
rm netscout.db*
dotnet run  # Will recreate fresh database
```

### "Foreign key constraint failed"

**Problem:** Database schema issue

**Solution:**
```bash
# Delete and recreate database
rm netscout.db*

# Or add migration (advanced):
dotnet ef migrations add FixForeignKey
dotnet ef database update
```

### Lost all my data!

**Problem:** Deleted database file

**Prevention:**
```bash
# Backup your database regularly
cp netscout.db netscout.db.backup

# Or automated backup script:
# Create backup_db.sh:
#!/bin/bash
DATE=$(date +%Y%m%d_%H%M%S)
cp netscout.db "backups/netscout_$DATE.db"
```

## Platform-Specific Issues

### Windows Issues

**PowerShell Execution Policy:**
```powershell
# If you get "cannot be loaded because running scripts is disabled"
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

**Windows Defender blocking:**
- Add NetScout folder to exclusions
- Settings → Update & Security → Windows Security → Virus & threat protection
- Manage settings → Exclusions → Add folder

### Linux Issues

**Missing packages:**
```bash
# Ubuntu/Debian
sudo apt update
sudo apt install dotnet-sdk-8.0

# Fedora
sudo dnf install dotnet-sdk-8.0
```

**Permission errors:**
```bash
# Run with sudo or configure capabilities:
sudo setcap cap_net_raw+ep /usr/bin/dotnet
```

### Mac Issues

**Gatekeeper blocking:**
- System Preferences → Security & Privacy
- Click "Allow Anyway" if blocked

**Rosetta 2 (M1/M2 Macs):**
```bash
# Should work natively, but if issues:
arch -x86_64 dotnet run
```

## Network Configuration Issues

### Scanning VPN networks

**Problem:** Connected to VPN and getting unexpected results

**Solution:**
1. Check your VPN IP: `ipconfig` or `ifconfig`
2. Scan VPN subnet if needed
3. Note: Your VPN provider may block scanning

### Scanning across subnets

**Problem:** Want to scan 192.168.0.x and 192.168.1.x

**Solution:**
Scan each subnet separately:
```bash
# First scan
Network prefix: 192.168.0

# Second scan
Network prefix: 192.168.1
```

### Docker/VM networks

**Problem:** Scanning Docker or VM networks

**Solution:**
- Docker networks usually: 172.17.0.x
- VMware: 192.168.x.x
- VirtualBox: 10.0.2.x
- Check with `docker network inspect bridge` or VM settings

## Performance Issues

### High CPU usage

**Normal:** Network scanning is CPU-intensive
**Solution:**
- Reduce concurrent operations (modify `Task.WhenAll`)
- Increase timeout (fewer retries)
- Scan smaller ranges

### High memory usage

**Problem:** Scanning very large networks (1000+ hosts)

**Solution:**
- Scan in batches (e.g., 50 hosts at a time)
- Implement pagination in code
- Consider splitting into multiple databases

### Application hangs

**Problem:** Deadlock or infinite wait

**Solution:**
1. Press Ctrl+C to exit
2. Check for devices that don't respond (might cause timeout issues)
3. Reduce timeout in code

## VS Code Issues

### IntelliSense not working

**Solution:**
1. Install C# Dev Kit extension
2. Reload window: Ctrl+Shift+P → "Reload Window"
3. Run: `dotnet restore`

### Debugger not attaching

**Solution:**
1. Check `.vscode/launch.json` exists
2. Set breakpoint
3. Press F5
4. If fails, rebuild: `dotnet build`

### Build task not found

**Solution:**
Copy tasks.json from project (already included)
Or: Ctrl+Shift+P → "Tasks: Configure Task"

## Getting Help

### Before asking for help, gather this info:

1. **Operating System & Version:**
```bash
# Windows
systeminfo | findstr /B "OS Name OS Version"

# Linux
cat /etc/os-release

# Mac
sw_vers
```

2. **.NET Version:**
```bash
dotnet --version
dotnet --info
```

3. **Error Message:**
Copy full error text, including stack trace

4. **What you were doing:**
Exact steps to reproduce

5. **Network info (if relevant):**
```bash
# Your IP configuration
ipconfig    # Windows
ifconfig    # Linux/Mac
```

### Where to get help:

1. **Check documentation first:**
   - README.md
   - QUICKSTART.md
   - This file (TROUBLESHOOTING.md)

2. **Search existing issues:**
   - GitHub Issues tab

3. **Create new issue:**
   - Include all info from above
   - Be specific
   - Provide error messages

4. **Community resources:**
   - Reddit: r/csharp, r/networking
   - Stack Overflow
   - Discord: C# server

## Still Having Issues?

### Debug mode for more info:

Add this to any method to see what's happening:
```csharp
Console.WriteLine($"DEBUG: Variable value = {variableName}");
```

### Enable detailed logging:

In `Program.cs`, add try-catch with full exception:
```csharp
try 
{
    // Your code
}
catch (Exception ex)
{
    Console.WriteLine($"ERROR: {ex.GetType().Name}");
    Console.WriteLine($"Message: {ex.Message}");
    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
}
```

### Test individual components:

```csharp
// Test ping only
var scanner = new NetworkScanner();
var result = await scanner.PingHostAsync("8.8.8.8");
Console.WriteLine($"Ping 8.8.8.8: {result}");

// Test port scan only
var ports = await scanner.ScanPortsAsync("8.8.8.8", new[] { 80, 443 });
Console.WriteLine($"Open ports: {string.Join(", ", ports)}");
```

## Preventive Measures

**Best practices to avoid issues:**

1. ✅ Always run `dotnet restore` after pulling changes
2. ✅ Keep .NET SDK updated
3. ✅ Backup database regularly
4. ✅ Test on small ranges first
5. ✅ Use version control (Git)
6. ✅ Read error messages carefully
7. ✅ Check network configuration before scanning
8. ✅ Run with appropriate permissions

---

**Remember:** Most issues are either permissions or network configuration. Start there!

If you've solved an issue not listed here, consider contributing it via GitHub pull request!
