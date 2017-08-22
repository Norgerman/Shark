﻿using Microsoft.Extensions.Logging;
using Shark.Constants;
using Shark.Data;
using Shark.Internal;
using System;
using System.Collections.Generic;
using System.Net;

namespace Shark
{
    public abstract class SharkServer : ISharkServer
    {
        public bool Disposed => _disposed;
        public IDictionary<Guid, ISocketClient> Clients => _clients;
        public virtual ILoggerFactory LoggerFactory => _loggerFactory;
        public abstract ILogger Logger { get; }

        public event Action<ISharkClient> OnConnected
        {
            add
            {
                _onConnected += value;
            }
            remove
            {
                _onConnected -= value;
            }
        }

        protected Action<ISharkClient> _onConnected;
        protected Dictionary<Guid, ISocketClient> _clients = new Dictionary<Guid, ISocketClient>();
        private bool _disposed = false;
        private ILoggerFactory _loggerFactory;

        protected SharkServer()
        {
            _loggerFactory = new LoggerFactory();
        }

        public ISharkServer OnClientConnected(Action<ISharkClient> onConnected)
        {
            _onConnected += onConnected;
            return this;
        }

        public ISharkServer ConfigureLogger(Action<ILoggerFactory> configure)
        {
            configure?.Invoke(LoggerFactory);
            return this;
        }


        public ISharkServer Bind(IPAddress address, int port)
        {
            return Bind(new IPEndPoint(address, port));
        }

        public ISharkServer Bind(string address, int port)
        {
            return Bind(IPAddress.Parse(address), port);
        }

        public void RemoveClient(Guid id)
        {
            _clients.Remove(id);
        }

        #region IDisposable Support
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        public abstract ISharkServer Bind(IPEndPoint endPoint);
        public abstract void Start();

        public static ISharkServer Create()
        {
            return new UvSharkServer();
        }
    }
}
