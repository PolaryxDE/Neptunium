using System;
using System.Net;
using System.Threading.Tasks;
using DotNetty.Codecs;
using DotNetty.Handlers.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Neptunium.Handlers;
using Neptunium.Protocol;
using Neptunium.Protocol.Serialization;

namespace Neptunium.Client;

/// <summary>
/// The base client is the base class for a client-sided client connected to a server allowing for communication
/// between the client and the server. It also adds a layer of abstraction to the underlying network connection.
/// </summary>
public abstract class BaseClient
{
    /// <summary>
    /// The ID of the client given by the server.
    /// </summary>
    public long ID { get; internal set; }

    /// <summary>
    /// The network channel associated to this client.
    /// </summary>
    public IChannel Channel { get; private set; } = null!;

    private readonly IEventLoopGroup _group;
    
    protected BaseClient()
    {
        _group = new MultithreadEventLoopGroup();
    }
    
    /// <summary>
    /// Gets called when the client is being disconnected.
    /// </summary>
    public virtual void OnDisconnect()
    {
        // do your own stuff here
    }

    /// <summary>
    /// Sends the given packet to the client.
    /// </summary>
    /// <param name="packet">The packet to be sent.</param>
    public void SendPacket(IPacket packet)
    {
        Channel.WriteAndFlushAsync(PacketSerializer.Serialize(packet));
    }

    /// <summary>
    /// Closes the connection and cleans up the garbage.
    /// </summary>
    public async Task CloseAsync()
    {
        await Channel.CloseAsync();
    }

    internal async Task ConnectServerAsync<T>(string ip, int port, string password, Action<T> promise) where T : BaseClient
    {
        var bootstrap = new Bootstrap();
        bootstrap.Group(_group)
            .Channel<TcpSocketChannel>()
            .Option(ChannelOption.TcpNodelay, true)
            .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
            {
                channel.Pipeline.AddLast(new LoggingHandler());
                channel.Pipeline.AddLast("framing-enc", new LengthFieldPrepender(2));
                channel.Pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(ushort.MaxValue, 0, 2, 0, 2));
                channel.Pipeline.AddLast("packet-handler", new PacketClientHandler<T>((T) this, promise, password));
            }));
        Channel = await bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse(ip), port));
    }
}