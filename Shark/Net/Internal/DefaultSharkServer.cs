﻿using Microsoft.Extensions.Logging;
using Shark.Logging;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Shark.Net.Internal
{
    internal class DefaultSharkServer : SharkServer
    {
        private TcpListener _listener;

        public override ILogger Logger
        {
            get
            {
                if (_logger == null)
                {
                    _logger = LoggerManager.LoggerFactory.CreateLogger<DefaultSharkServer>();
                }
                return _logger;
            }
        }

        private ILogger _logger;

        public override ISharkServer Bind(IPEndPoint endPoint)
        {
            _listener = new TcpListener(endPoint);
            if (endPoint.Address.Equals(IPAddress.IPv6Any))
            {
                _listener.Server.DualMode = true;
            }
            return this;
        }

        internal DefaultSharkServer()
            : base()
        {

        }

        public override async Task Start(int backlog = (int)SocketOptionName.MaxConnections)
        {
            _listener.Start(backlog);
            Logger.LogInformation($"Server started, listening on {_listener.LocalEndpoint}, backlog: {backlog}");
            while (true)
            {
                var client = await _listener.AcceptTcpClientAsync();
                var sharkClient = new DefaultSharkClient(client, this);
                client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                OnClientConnect(sharkClient);
            }
        }
    }
}
