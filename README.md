# Linux KMS Server (Customized)

Custom C# KMS Server emulator modified for cross-platform Linux deployment, containerization, and enhanced stability.

## Key Changes & Improvements

1. **Robust Bounds Checking (`KMSRequestHandler.cs`)**
   - Implemented array length validation (`request.Length < 11`) before index accessing.
   - Prevents `System.IndexOutOfRangeException` crashes caused by malformed packets, empty network probes, or port scanners on port 1688.

2. **Reactivation Renewal Interval Extended (`KMSServerSettings.cs`)**
   - Updated `VLRenewalInterval` to **259,200 minutes** (180 days) to match standard Microsoft KMS renewal periods instead of the default 30-day window.

3. **Cross-Platform & Container Readiness**
   - Retargeted project capabilities to **.NET 8.0** alongside legacy Mono/Framework execution.
   - Added multi-stage `Dockerfile` for single-command containerized deployment across Linux and Windows environments.

4. **Background Service Support**
   - Added native systemd configuration options (`kms.service`) for automated background execution and self-healing restarts on system boot.

## Quick Start (Docker)

```bash
docker build -t kms-server .
docker run -d --name kms-server --restart unless-stopped -p 1688:1688 kms-server
