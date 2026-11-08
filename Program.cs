using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CombinedKmsServer
{
    public interface ILogger { void LogMessage(string message); }
    public class ConsoleLogger : ILogger
    {
        public void LogMessage(string message)
        {
            Console.WriteLine("[{0}] {1}", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"), message);
        }
    }

    public interface IKMSServerSettings
    {
        int KMSPort { get; }
        byte[] KMSHWID { get; }
        uint VLActivationInterval { get; }
        uint VLRenewalInterval { get; set; }
    }

    public class KMSServerSettings : IKMSServerSettings
    {
        private uint _vlRenewalInterval = 259200; // 180 Days in minutes

        public int KMSPort { get { return 1688; } }
        public byte[] KMSHWID { get { return new byte[] { 0x36, 0x4F, 0x46, 0x3A, 0x88, 0x63, 0xD3, 0x5F }; } }
        public uint VLActivationInterval { get { return 120; } }

        public uint VLRenewalInterval
        {
            get { return _vlRenewalInterval; }
            set { _vlRenewalInterval = value; } // No 43200-minute upper bound check
        }
    }

    public interface IMessageHandler { byte[] HandleRequest(byte[] request); }

    public class KMSRequestHandler : IMessageHandler
    {
        private readonly IKMSServerSettings _settings;
        private readonly ILogger _logger;

        public KMSRequestHandler(IKMSServerSettings settings, ILogger logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public byte[] HandleRequest(byte[] request)
        {
            const int unknownDataSize = 8;
            if (request == null || request.Length < unknownDataSize + 3)
            {
                return new byte[0]; // Bounds checking guard for port scans
            }

            _logger.LogMessage("KMS Activation Request Received.");
            return new byte[0];
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            ILogger logger = new ConsoleLogger();
            IKMSServerSettings settings = new KMSServerSettings();
            KMSRequestHandler requestHandler = new KMSRequestHandler(settings, logger);

            TcpListener listener = new TcpListener(IPAddress.Any, settings.KMSPort);
            listener.Start();

            logger.LogMessage("KMS Server V4 Started on Port " + settings.KMSPort);
            logger.LogMessage("Renewal Interval: " + settings.VLRenewalInterval + " minutes (180 Days)");

            while (true)
            {
                try
                {
                    using (TcpClient client = listener.AcceptTcpClient())
                    using (NetworkStream stream = client.GetStream())
                    {
                        byte[] buffer = new byte[4096];
                        int bytesRead = stream.Read(buffer, 0, buffer.Length);
                        if (bytesRead > 0)
                        {
                            byte[] request = new byte[bytesRead];
                            Array.Copy(buffer, request, bytesRead);
                            byte[] response = requestHandler.HandleRequest(request);
                            if (response != null && response.Length > 0)
                            {
                                stream.Write(response, 0, response.Length);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogMessage("Error: " + ex.Message);
                }
            }
        }
    }
}
