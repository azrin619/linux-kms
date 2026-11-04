using System;
using System.Net;
using System.Runtime.InteropServices;

namespace KMSEmulator
{
    public static class ManagedIpHelper
    {
        public static object[] GetExtendedTcpTable(bool sorted)
        {
            // Check if running on Linux / Unix / MacOS
            PlatformID p = Environment.OSVersion.Platform;
            if (p == PlatformID.Unix || p == PlatformID.MacOSX || (int)p == 4 || (int)p == 128)
            {
                return new object[0];
            }

            try
            {
                return IpHelper.GetExtendedTcpTable(sorted);
            }
            catch
            {
                return new object[0];
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TcpRow
    {
        public uint state;
        public uint localAddr;
        public uint localPort;
        public uint remoteAddr;
        public uint remotePort;
        public uint owningPid;

        // Required property aliases for KMSEmulator.cs compatibility
        public IPEndPoint LocalEndPoint
        {
            get
            {
                try
                {
                    return new IPEndPoint(new IPAddress(localAddr), (ushort)IPAddress.NetworkToHostOrder((short)localPort));
                }
                catch
                {
                    return new IPEndPoint(IPAddress.Any, 0);
                }
            }
        }

        public int ProcessId
        {
            get { return (int)owningPid; }
        }
    }

    internal static class IpHelper
    {
        public enum TcpTableType
        {
            BasicListener = 0,
            BasicConnections = 1,
            BasicAll = 2,
            OwnerPidListener = 3,
            OwnerPidConnections = 4,
            OwnerPidAll = 5,
            OwnerModuleListener = 6,
            OwnerModuleConnections = 7,
            OwnerModuleAll = 8
        }

        [DllImport("iphlpapi.dll", SetLastError = true)]
        private static extern uint GetExtendedTcpTable(
            IntPtr pTcpTable,
            ref int dwOutBufLen,
            bool sort,
            int ipVersion,
            TcpTableType tblClass,
            uint reserved
        );

        public static object[] GetExtendedTcpTable(bool sorted)
        {
            int bufferSize = 0;
            GetExtendedTcpTable(IntPtr.Zero, ref bufferSize, sorted, 2, TcpTableType.OwnerPidAll, 0);
            return new object[0];
        }
    }
}
