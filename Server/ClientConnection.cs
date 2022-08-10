using System.Threading.Tasks;
using DotNetty.Transport.Channels;
using Neptunium.Protocol;
using Neptunium.Protocol.Serialization;

namespace Neptunium;

/// <summary>
/// The client connection describes a connection from a client to the server on the server-side. This abstract class
/// must be extended by the custom client class which the server will use as client class.
/// </summary>
public abstract class ClientConnection
{
    /// <summary>
    /// The unique ID given by the server which identifies the client.
    /// </summary>
    public long ID { get; }
    
    /// <summary>
    /// The netty channel which is used to communicate with the client.
    /// </summary>
    public IChannel Channel { get; }

    protected ClientConnection(long id, IChannel channel)
    {
        ID = id;
        Channel = channel;
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
}