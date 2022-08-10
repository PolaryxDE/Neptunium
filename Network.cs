using System;
using System.Threading.Tasks;
using Neptunium.Client;
using Neptunium.Server;

namespace Neptunium;

/// <summary>
/// The network class is used to utilize the existing components and bringing them together to one entry point.
/// </summary>
public static class Network
{
    /// <summary>
    /// Whether to use libuv or not.
    /// </summary>
    public static bool UseLibuv { get; set; } = false;
    
    /// <summary>
    /// How many event loops should be sued for the <see cref="DotNetty.Transport.Channels.MultithreadEventLoopGroup"/>
    /// </summary>
    internal static int EventLoopCount { get; set; } = 1;
    
    /// <summary>
    /// Tries to connect to the server of th given ip and port.
    /// If a password is given, the password will be used for authentication on the server side.
    /// If the password is secured with the server and no or the wrong password was passed,
    /// the connection will be rejected.
    /// </summary>
    /// <param name="ip">The ip of the remote server.</param>
    /// <param name="port">The port of the remote server.</param>
    /// <param name="password">The password which is maybe needed.</param>
    /// <param name="callback">Gets called when the connection was successfully established. It passes the created connection object.</param>
    /// <returns>An object representing the connection or null on failure.</returns>
    public static async Task ConnectAsync<T>(string ip, int port, string password, Action<T> callback) where T : BaseClient
    {
        try
        {
            var client = Activator.CreateInstance<T>();
            await client.ConnectServerAsync(ip, port, password, callback);
        }
        catch (Exception e)
        {
            LoggingInterface.ThrowError(e);
        }
    }

    /// <summary>
    /// Creates a new instance of a server which will run on the given ip and port.
    /// If a password is given, the password will be needed in order to authenticate a client.
    /// If a client can't given the right password on connection, the connection will be immediately destroyed.
    /// The password authentication happens instantly after the connection was established.
    /// </summary>
    /// <param name="isLocal">If true, the server will be hosted on the loopback address, otherwise on the 0.0.0.0 interface.</param>
    /// <param name="port">The port of the new server.</param>
    /// <param name="password">The optional password.</param>
    /// <returns>The newly created server or null on failure.</returns>
    public static async Task<IServer<T>?> CreateAsync<T>(bool isLocal, int port, string? password = null) where T : ClientConnection
    {
        try
        {
            var server = new ServerImpl<T>(isLocal, port, password);
            await server.StartAsync();
            return server;
        }
        catch (Exception e)
        {
            LoggingInterface.ThrowError(e);
            return null;
        }
    }
}