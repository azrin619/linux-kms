using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using KMSEmulator.Logging;
using KMSEmulator.RPC;

namespace KMSEmulator
{
    /// <summary>
    /// TCP RPC Server Handler to listen for and serve RPC Clients
    /// </summary>
    public class TCPServer : IDisposable
    {
        private TcpListener _tcpListenerIpv6, _tcpListenerIpv4;
        private readonly RpcMessageHandler _messageHandler;
        private readonly List<Client> _clients = new List<Client>();
        private readonly ILogger _logger;
        internal bool Running;

        /// <summary>
        /// Create a new Instance of TCPServer
        /// </summary>
        /// <param name="messageHandler">RPC Message Handler to handle and format request and response messages</param>
        /// <param name="logger">Log handler that implements ILogger</param>
        public TCPServer(RpcMessageHandler messageHandler, ILogger logger)
        {
            _messageHandler = messageHandler;
            _logger = logger;
        }

        /// <summary>
        /// Start TCP Server and listen for clients
        /// </summary>
        /// <param name="port">TCP Port to listen on</param>
        public void Start(int port)
        {
            bool ipv6Failure = false;

            try
            {
                _tcpListenerIpv6 = new TcpListener(IPAddress.IPv6Any, port);
                _tcpListenerIpv6.Server.SendBufferSize = 1024;
                _tcpListenerIpv6.Server.ReceiveBufferSize = 1024;

                try
                {
                    _tcpListenerIpv6.Server.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
                }
                catch (SocketException) // XP/2003 doesn't support IPv4 mapped addresses
                {
                    ipv6Failure = true;
                }

                // Start TCP Server listening and listener event
                _tcpListenerIpv6.Start();
                _tcpListenerIpv6.BeginAcceptTcpClient(AcceptTcpClientCallback, _tcpListenerIpv6);
            }
            catch (SocketException) // If anything else is wrong with IPv6, don't fail but try IPv4
            {
                ipv6Failure = true;
                _tcpListenerIpv6 = null;
                _logger.LogMessage("IPv6 failure. Trying IPv4 only");
            }

            if (ipv6Failure)
            {
                _tcpListenerIpv4 = new TcpListener(IPAddress.Any, port);
                _tcpListenerIpv4.Server.SendBufferSize = 1024;
                _tcpListenerIpv4.Server.ReceiveBufferSize = 1024;
                _tcpListenerIpv4.Start();
                _tcpListenerIpv4.BeginAcceptTcpClient(AcceptTcpClientCallback, _tcpListenerIpv4);
            }
            Running = true;
        }

        /// <summary>
        /// Stop TCP Server when this object is disposed
        /// </summary>
        public void Dispose()
        {
            Stop();
        }

        /// <summary>
        /// Stops the TCP Server listening for new clients and disconnects
        /// any currently connected clients.
        /// </summary>
        public void Stop()
        {
            // Stop TCP Server
            if (_tcpListenerIpv6 != null) _tcpListenerIpv6.Stop();
            if (_tcpListenerIpv4 != null) _tcpListenerIpv4.Stop();

            // Disconnect Clients and Clear Active Client List
            lock (_clients)
            {
                foreach (Client client in _clients)
                {
                    try
                    {
                        client.TcpClient.Client.Disconnect(false);
                    }
                    catch { }
                }
                _clients.Clear();
            }

            // Mark TCP Server as Stopped
            Running = false;
        }

        /// <summary>
        /// Callback for the accept tcp client operation.
        /// </summary>
        /// <param name="result">The async result object</param>
        private void AcceptTcpClientCallback(IAsyncResult result)
        {
            TcpListener tcpListener = result.AsyncState as TcpListener;
            try
            {
                if (tcpListener != null)
                {
                    TcpClient tcpClient = tcpListener.EndAcceptTcpClient(result);
                    byte[] buffer = new byte[tcpClient.ReceiveBufferSize];

                    // Add Client to Client List
                    Client client = new Client(tcpClient, buffer);
                    lock (_clients)
                    {
                        _clients.Add(client);
                    }

                    // Read Client Message
                    NetworkStream networkStream = client.TcpClient.GetStream();
                    networkStream.BeginRead(client.Buffer, 0, client.Buffer.Length, ReadCallback, client);

                    tcpListener.BeginAcceptTcpClient(AcceptTcpClientCallback, tcpListener);
                }
            }
            catch (ObjectDisposedException)
            {
                // Log Client Listener Shutdown Error
                _logger.LogMessage(string.Format("{0} client listener shut down", tcpListener != null && tcpListener.Equals(_tcpListenerIpv6) ? "IPv6" : "IPv4"));
            }
            catch (SocketException)
            {
                // Ignore aborted connections during accept and keep listening
                try
                {
                    if (tcpListener != null && Running)
                    {
                        tcpListener.BeginAcceptTcpClient(AcceptTcpClientCallback, tcpListener);
                    }
                }
                catch { }
            }
            catch (Exception ex)
            {
                _logger.LogMessage("Accept error: " + ex.Message);
            }
        }

        /// <summary>
        /// Callback for the read operation.
        /// </summary>
        /// <param name="result">The async result object</param>
        private void ReadCallback(IAsyncResult result)
        {
            Client client = result.AsyncState as Client;
            if (client == null) return;

            try
            {
                // Read Client Request Message
                NetworkStream networkStream = client.TcpClient.GetStream();

                // Client Request Message Read Complete
                if (networkStream.EndRead(result) == 0)
                {
                    return;
                }

                // Send Server Response Message (RPC Bind)
                byte[] response = _messageHandler.HandleRequest(client.Buffer);
                if (response != null && response.Length > 0)
                {
                    client.TcpClient.Client.Send(response);
                }

                // Read and Respond to Second Message (KMS Activation Request)
                byte[] request2 = GetRequestArray(client);
                if (request2 != null && request2.Length > 0)
                {
                    byte[] response2 = _messageHandler.HandleRequest(request2);
                    if (response2 != null && response2.Length > 0)
                    {
                        client.TcpClient.Client.Send(response2);
                    }
                }

                // Close Client Connection
                client.TcpClient.Client.Shutdown(SocketShutdown.Receive);
            }
            catch (SocketException)
            {
                // Safely ignore abrupt client disconnects / TCP resets (e.g., port scanners)
            }
            catch (IOException)
            {
                // Safely ignore network stream read/write interruptions
            }
            catch (ObjectDisposedException)
            {
                // Safely ignore already disposed sockets
            }
            catch (Exception ex)
            {
                _logger.LogMessage("Client handling error: " + ex.Message);
            }
            finally
            {
                // Always clean up socket and remove client from active list
                try
                {
                    if (client.TcpClient != null)
                    {
                        client.TcpClient.Close();
                    }
                }
                catch { }

                lock (_clients)
                {
                    _clients.Remove(client);
                }
            }
        }

        /// <summary>
        /// Get Client Request Message as Byte Array and store it with the Client Object
        /// </summary>
        /// <param name="client">TCP Client Object Handle</param>
        /// <returns>Byte Array of Client Request Message</returns>
        private static byte[] GetRequestArray(Client client)
        {
            // Read Client Request Message into Buffer
            byte[] requestBuffer = new byte[1024];
            int requestSize = client.TcpClient.Client.Receive(requestBuffer);

            if (requestSize <= 0)
            {
                return new byte[0];
            }

            // Clear old buffer data before copying new request
            Array.Clear(client.Buffer, 0, client.Buffer.Length);
            Array.Copy(requestBuffer, client.Buffer, Math.Min(requestSize, client.Buffer.Length));
            return client.Buffer;
        }

        /// <summary>
        /// Internal class to join the TCP client and buffer together 
        /// for easy management in the server
        /// </summary>
        private class Client
        {
            internal readonly TcpClient TcpClient;
            internal readonly byte[] Buffer;

            /// <summary>
            /// Create a new Client Object
            /// </summary>
            /// <param name="tcpClient">TCP Client Object Handle</param>
            /// <param name="buffer">Buffered Client Request Message</param>
            internal Client(TcpClient tcpClient, byte[] buffer)
            {
                TcpClient = tcpClient;
                Buffer = buffer;
            }
        }
    }
}
