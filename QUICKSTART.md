# NetScout - Quick Start Guide

Get NetScout running in 5 minutes! 🚀

## Step 1: Prerequisites Check

Make sure you have .NET 8.0 SDK installed:
```bash
dotnet --version
```

If not installed, download from: https://dotnet.microsoft.com/download/dotnet/8.0

## Step 2: Navigate to Project

```bash
cd NetScout
```

## Step 3: Restore Dependencies

```bash
dotnet restore
```

This downloads all required NuGet packages (Entity Framework, SQLite, etc.)

## Step 4: Build the Project

```bash
dotnet build
```

You should see "Build succeeded" with 0 warnings.

## Step 5: Run NetScout!

```bash
dotnet run
```

You'll see the NetScout menu appear!

## First Scan - Try This!

1. Select option **1** (Scan Network)
2. Enter your network prefix, for example:
   - Home network: Usually `192.168.1` or `192.168.0`
   - Work network: Check with `ipconfig` (Windows) or `ifconfig` (Mac/Linux)
3. Use default range (1-254) by pressing Enter twice
4. Wait for the scan to complete
5. Say **yes** when asked to port scan discovered devices
6. Select option **3** to view all discovered devices!

## Example Session

```
Select option: 1
Enter network prefix: 192.168.1
Enter start host: [press Enter for default]
Enter end host: [press Enter for default]

[+] Found active host: 192.168.1.1
[+] Found active host: 192.168.1.105
[+] Found active host: 192.168.1.150
...

Scan complete. Found 12 active hosts.
Would you like to perform a port scan? (y/n): y
```

## What's Happening Behind the Scenes?

- NetScout pings each IP address in your network
- Saves active devices to a SQLite database (`netscout.db`)
- Optionally scans common ports (SSH, HTTP, HTTPS, RDP, etc.)
- Stores all results for future reference

## Troubleshooting

### "dotnet: command not found"
Install the .NET 8.0 SDK from Microsoft's website.

### Permission Issues on Linux/Mac
You may need elevated privileges for network operations:
```bash
sudo dotnet run
```

### Scan finds no devices
- Make sure you're on the correct network
- Check your IP with `ipconfig` or `ifconfig`
- Some networks may block ICMP (ping) packets
- Try scanning a single known device first (option 2)

### Port scans are slow
- Port scanning takes time (checking 14 ports per device)
- You can modify the timeout in `NetworkScanner.cs` (line 16)
- Smaller network ranges scan faster

## Next Steps

Once you've done your first scan:

1. **View Device Details** (option 4) - See all open ports on a device
2. **Add Notes** (option 5) - Document what each device is
3. **Check the Database** - `netscout.db` is created in the project folder
4. **Customize** - Edit `NetworkScanner.cs` to scan different ports

## Adding to GitHub

```bash
# Initialize git repo (if not already)
git init

# Add all files
git add .

# Commit
git commit -m "Initial commit: NetScout v1.0"

# Add your GitHub repo as remote
git remote add origin https://github.com/yourusername/NetScout.git

# Push to GitHub
git push -u origin main
```

## Need Help?

- Check the full [README.md](README.md) for detailed documentation
- Review [CONTRIBUTING.md](CONTRIBUTING.md) for development guidelines
- Open an issue on GitHub if you find bugs

**Happy Scanning! 🎯**
