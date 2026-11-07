# Linux KMS Server (Customized)  Version 3.0

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




##  **How to use and run**

# download the tar.gz or pull.
install XBUILD and other stuff in yr Linux
xbuild /p:Configuration=Release KMSEmulator.NET/KMSEmulator.NET.csproj
This will export the EXE FILE. I have removed ALOT of nonsense so it would build properly and run without any DLL issues.

sudo mono KMSEmulator.NET/bin/Release/KMSEmulator.NET.exe

# User Side
1. ** Check the Distro Version
      DISM /ONLINE /get-targetEditions
      This will show which version UR on, and What Key is needed. See my gist if u need

2. ** UPDATE THE IMAGE    
      DISM /online /Set-Edition:(PRODUCTKEY) /AcceptEULA

3. ** ACTIVATE IT
      Open Elevated Command *(WIN)Cmd > Run as Administrator
      SLMGR /DLV  >Verify the image and type is to VLK
      SLMGR /SKMS YOUR-KMS.SERVER:1688 *(or watever U chose in config)
            slmgr /skms kms.azrin.info:1688 if you are testing
      SLMGR /ato *(Activate it),it should see Product Activated
      SLMGR /dlv again to verify
<img width="1085" height="812" alt="image" src="https://github.com/user-attachments/assets/148a326f-08f6-4a76-b718-ed469ca6b579" />


On Server side, it should show :
<img width="822" height="527" alt="image" src="https://github.com/user-attachments/assets/562dbf74-ea26-4c03-bc24-52e6e43901ce" />

KMS Activation Request (KMS V6.0) Received:
KMS PID: 03612-00206-491-710219-03-18441-14393.0000-1062023
Application ID: 55c92734-d682-4d71-983e-d6ec3f16059f
Client Machine ID: e564fa68-9b53-440d-ba25-033d93049512
KMS Counted ID: 907f1f65-adcd-4a2e-95bc-4bf500bc6e58
SKUID ID: 7dc26449-db21-4e09-ba37-28f2958506a6
KMS Activation Response (KMS V6.0) sent.

Good luck










#Licence Key
##Operating System Edition	KMS Client Product Key
** 🖥️ Windows 10 / 11
Windows 10/11 Pro	W269N-WFGWX-YVC9B-4J6C9-T83GX
Windows 10/11 Pro N	MH37W-N47XK-V7XM9-C7227-GCQG9
Windows 10/11 Pro for Workstations	NRG8B-VKK3Q-CXVCJ-9G2XF-6Q84J
Windows 10/11 Pro for Workstations N	9FNHH-K3HBT-3W4TD-6383H-6XYWF
 
Windows 10/11 Education	NW6C2-QMPVW-D7KKK-3GKT6-VCFB2
Windows 10/11 Education N	2WH4N-8QGBV-H22JP-CT43Q-MDWWJ
Windows 10/11 Pro Education	6TP4R-GNPTD-KYYHQ-7B7DP-J447Y
Windows 10/11 Pro Education N	YVWGF-BXNMC-HTQYQ-CPQ99-66QFC
 
Windows 10/11 Enterprise	NPPR9-FWDCX-D2C8J-H872K-2YT43
Windows 10/11 Enterprise N	DPH2V-TTNVB-4X9Q3-TJR4H-KHJW4
Windows 10/11 Enterprise G	YYVX9-NTFWV-6MDM3-9PT4T-4M68B
Windows 10/11 Enterprise G N	44RPN-FTY23-9VTTB-MP9BX-T84FV
 
Windows 11 Enterprise LTSC 2024	M7XTQ-FN8P6-TTKYV-9D4CC-J462D
Windows 10 Enterprise LTSC 2021
Windows 10 Enterprise LTSC 2019
 
Windows 11 Enterprise N LTSC 2024	92NFX-8DJQP-P6BBQ-THF9C-7CG2H
Windows 10 Enterprise N LTSC 2021
Windows 10 Enterprise N LTSC 2019
 
Windows 10 Enterprise LTSB 2016	DCPHK-NFMTC-H88MJ-PFHPY-QJ4BJ
Windows 10 Enterprise LTSB 2015	WNMTR-4C88C-JK8YV-HQ7T2-76DF9
 
Windows 10 Enterprise N LTSB 2016	QFFDN-GRT3P-VKWWX-X7T3R-8B639
Windows 10 Enterprise N LTSB 2015	2F77B-TNFGY-69QQF-B8YKP-D69TJ
 
Windows IoT Enterprise LTSC 2024/2021	KBN8V-HFGQ4-MGXVD-347P6-PDQGT
 
