using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using DotNetty.Codecs;
using DotNetty.Handlers.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Libuv;
using Neptunium.Handlers;
using Neptunium.Protocol;
using Neptunium.Protocol.Packets;

namespace Neptunium.Server;

internal class ServerImpl<T> : IServer<T> where T : ClientConnection
{
    public List<T> Clients { get; }
    
    public int Port { get; }
    
    public event Action<T>? Disconnect;
    public event Action<T>? Connect;

    private readonly Type _clientType;
    private readonly bool _isLocal;
    private readonly string? _password;
    private readonly IEventLoopGroup _bossGroup;
    private readonly IEventLoopGroup _workerGroup;
    
    private IChannel _boundChannel = null!;
    private long _currentId;

    internal ServerImpl(bool local, int port, string? password)
    {
        Clients = new List<T>();
        Port = port;
        _currentId = 0;
        _isLocal = local;
        _password = password;
        _clientType = typeof(T);

        if (Network.UseLibuv)
        {
            var dispatcher = new DispatcherEventLoopGroup();
            _bossGroup = dispatcher;
            _workerGroup = new WorkerEventLoopGroup(dispatcher);
        }
        else
        {
            _bossGroup = new MultithreadEventLoopGroup(Network.EventLoopCount);
            _workerGroup = new MultithreadEventLoopGroup();
        }
    }

    public async Task StartAsync()
    {
        var bootstrap = new ServerBootstrap();
        bootstrap.Group(_bossGroup, _workerGroup);

        if (Network.UseLibuv)
        {
            bootstrap.Channel<TcpServerChannel>();
        }
        else
        {
            bootstrap.Channel<TcpServerSocketChannel>();
        }

        bootstrap.Option(ChannelOption.SoBacklog, 100)
            .Handler(new LoggingHandler("SRV-LSTN"))
            .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
            {
                channel.Pipeline.AddLast(new LoggingHandler());
                channel.Pipeline.AddLast("framing-enc", new LengthFieldPrepender(2));
                channel.Pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(ushort.MaxValue, 0, 2, 0, 2));
                channel.Pipeline.AddLast("packet-handler", new PacketServerHandler<T>(this, _password));
            }));
        _boundChannel = await bootstrap.BindAsync(!_isLocal ? IPAddress.Any : IPAddress.Loopback, Port);
    }

    public async Task StopAsync()
    {
        await _boundChannel.CloseAsync();
        await _bossGroup.ShutdownGracefullyAsync();
        await _workerGroup.ShutdownGracefullyAsync();
    }

    public void BroadcastPacket(IPacket packet)
    {
        lock (Clients)
        {
            for (int i = Clients.Count - 1; i >= 0; i--)
            {
                Clients[i].SendPacket(packet);
            }
        }
    }

    public T? GetClientByChannel(IChannel channel)
    {
        lock (Clients)
        {
            return Clients.FirstOrDefault(client => Equals(client.Channel.Id, channel.Id));
        }
    }

    public T? GetClient(long id)
    {
        lock (Clients)
        {
            return Clients.FirstOrDefault(client => Equals(client.ID, id));
        }
    }

    internal void TriggerClientDisconnect(T client)
    {
        lock (Clients)
        {
            Clients.Remove(client);
            Disconnect?.Invoke(client);
            client.OnDisconnect();
        }
    }

    internal void TriggerClientConnect(IChannel channel)
    {
        var client = CreateClient(channel);
        if (client == null)
        {
            channel.CloseAsync();
            throw new ArgumentException(_clientType.FullName + " has no ctor with only the ID and the channel!");
        }
        
        lock (Clients)
        {
            Clients.Add(client);
            Connect?.Invoke(client);
        }
        
        client.SendPacket(new AuthFinishPacket{ClientID = client.ID});
    }

    private T? CreateClient(IChannel channel)
    {
        var id = _currentId++;
        return Activator.CreateInstance(_clientType,
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, new object[] {id, channel}) as T;
    }
}