/*
 * Copyright (C) RyuSOCKS
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License version 2,
 * as published by the Free Software Foundation.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <https://www.gnu.org/licenses/>.
 */

using NetCoreServer;
using RyuSocks.Auth;
using RyuSocks.Commands;
using RyuSocks.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Xunit;

namespace RyuSocks.Test
{
    public class SocksClientTests : IDisposable, IClassFixture<SocksServerFixture>
    {
        private readonly SocksServerFixture _fixture;
        private readonly SocksClient _client;

        public SocksClientTests(SocksServerFixture fixture)
        {
            _fixture = fixture;
            _client = new SocksClient((IPEndPoint)_fixture.Server.Endpoint)
            {
                OfferedAuthMethods = new Dictionary<AuthMethod, IProxyAuth>
                {
                    { AuthMethod.NoAuth, new NoAuth() },
                },
            };
        }

        public void Dispose()
        {
            _client.Dispose();
            GC.SuppressFinalize(this);
        }

        [Fact]
        public void Authenticate_Succeeds()
        {
            _client.Authenticate();
            Guid sessionId = _fixture.Server.Sessions.Keys.First();

            Assert.Equal(1, _fixture.Server.ConnectedSessions);
            Assert.True(_fixture.Server.GetSession(sessionId).IsAuthenticated);
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    public class SocksServerFixture : IDisposable
    {
        public TestSocksServer Server { get; }

        public SocksServerFixture()
        {
            Server = new TestSocksServer(IPAddress.Loopback, 0)
            {
                AcceptableAuthMethods = new HashSet<AuthMethod>
                {
                    AuthMethod.NoAuth,
                },
                OfferedCommands = new HashSet<ProxyCommand>
                {
                    ProxyCommand.Connect,
                    ProxyCommand.Bind,
                    ProxyCommand.UdpAssociate,
                },
            };

            Server.Start();
        }

        public void Dispose()
        {
            Server.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    public class TestSocksServer : SocksServer
    {
        public TestSocksServer(IPAddress address, ushort port = ProxyConsts.DefaultPort) : base(address, port) { }
        public TestSocksServer(string address, ushort port = ProxyConsts.DefaultPort) : base(address, port) { }
        public TestSocksServer(DnsEndPoint endpoint) : base(endpoint) { }
        public TestSocksServer(IPEndPoint endpoint) : base(endpoint) { }

        public new ConcurrentDictionary<Guid, TcpSession> Sessions => base.Sessions;

        public SocksSession GetSession(Guid id)
        {
            return (SocksSession)base.Sessions[id];
        }
    }
}
