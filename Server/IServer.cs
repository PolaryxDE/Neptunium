using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetty.Transport.Channels;
using Neptunium.Protocol;

namespace Neptunium.Server;

/// <summary>
/// The interface is representing the server with all its clients and underlying listeners.
/// From here you can manage the server's connections as well as the packet traffic.
/// </summary>
public interface IServer<T> where T : ClientConnection
{
    /// <summary>
    /// All clients which are connected to this instance of the server.
    /// </summary>
    List<T> Clients { get; }
    
    /// <summary>
    /// The port of the running server.
    /// </summary>
    int Port { get; }
    
    /// <summary>
    /// Gets called when a connection disconnects from the network.
    /// This event isn't get called when the client disconnects because of wrong password!
    /// </summary>
    event Action<T> Disconnect;

    /// <summary>
    /// Gets called when a connection was successfully opened.
    /// </summary>
    event Action<T> Connect;

    /// <summary>
    /// Starts the server and listens to incoming connection requests.
    /// </summary>
    Task StartAsync();

    /// <summary>
    /// Stops the server and the underlying listener.
    /// </summary>
    Task StopAsync();

    /// <summary>
    /// Broadcasts a packet to all connected clients of this instance of the server.
    /// </summary>
    /// <param name="packet">The packet to be sent.</param>
    void BroadcastPacket(IPacket packet);

    /// <summary>
    /// Returns the associated client connection to the given channel.
    /// </summary>
    /// <param name="channel">The network channel of the wanted connection.</param>
    /// <returns>The associated client connection to the given channel.</returns>
    T? GetClientByChannel(IChannel channel);

    /// <summary>
    /// Returns the associated client connection to the given id.
    /// </summary>
    /// <param name="id">The network id of the wanted connection.</param>
    /// <returns>The associated client connection to the given id.</returns>
    T? GetClient(long id);
}