﻿using Microsoft.Extensions.Logging;
using Shark.Data;
using Shark.Logging;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Shark.Net.Internal
{
    internal class DefaultSharkClient : SharkClient
    {
        public override event Action<ISocketClient> RemoteDisconnected;
        public override ILogger Logger
        {
            get
            {
                if (_logger == null)
                {
                    _logger = LoggerManager.LoggerFactory.CreateLogger<DefaultSharkClient>();
                }
                return _logger;
            }
        }

        private ILogger _logger;
        private TcpClient _tcp;
        private NetworkStream _stream;
        private object _syncRoot;

        internal DefaultSharkClient(TcpClient tcp, SharkServer server)
            : base(server)
        {
            _tcp = tcp;
            _stream = _tcp.GetStream();
            _syncRoot = new object();
        }

        public override async Task<ISocketClient> ConnectTo(IPEndPoint endPoint, RemoteType type = RemoteType.Tcp, Guid? id = null)
        {
            ISocketClient socket;
            if (type == RemoteType.Tcp)
            {
                socket = await DefaultSocketClient.ConnectTo(endPoint, id);
            }
            else
            {
                socket = await UdpSocketClient.ConnectTo(endPoint, id);
            }
            RemoteClients.Add(socket.Id, socket);
            return socket;
        }

        public override Task FlushAsync()
        {
            return _stream.FlushAsync();
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count)
        {
            var readed = await _stream.ReadAsync(buffer, offset, count);
            if (readed == 0)
            {
                CloseConnetion();
                CanRead = false;
            }
            return readed;
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count)
        {
            return _stream.WriteAsync(buffer, offset, count);
        }

        private void CloseConnetion()
        {
            try
            {
                _tcp.Client.Shutdown(SocketShutdown.Send);
            }
            catch (Exception)
            {
                Logger.LogWarning("Socket errored before shutdown and disconnect");
            }
            Logger.LogInformation("Shark no data to read, closed {0}", Id);
            RemoteDisconnected?.Invoke(this);
        }

        protected override void Dispose(bool disposing)
        {
            lock (_syncRoot)
            {
                if (!Disposed)
                {
                    if (disposing)
                    {
                        try
                        {
                            _tcp.Client.Shutdown(SocketShutdown.Both);
                            _tcp.Client.Disconnect(false);
                        }
                        catch (Exception)
                        {
                            Logger.LogWarning("Socket errored before shutdown and disconnect");
                        }
                        _stream.Dispose();
                        _tcp.Dispose();
                        RemoteDisconnected = null;
                        _tcp = null;
                        _stream = null;
                    }
                    base.Dispose(disposing);
                }
            }
        }
    }
}
