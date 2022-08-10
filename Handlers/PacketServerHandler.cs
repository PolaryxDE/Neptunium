using System;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Neptunium.Protocol.Packets;
using Neptunium.Protocol.Serialization;
using Neptunium.Server;

namespace Neptunium.Handlers;

internal class PacketServerHandler<T> : ChannelHandlerAdapter where T : ClientConnection
{
    private bool _isAuthenticated;
    private readonly ServerImpl<T> _server;
    private readonly string? _password;

    internal PacketServerHandler(ServerImpl<T> server, string? password)
    {
        _server = server;
        _password = password;
        _isAuthenticated = false;
    }

    public override void ChannelInactive(IChannelHandlerContext context)
    {
        var client = _server.GetClientByChannel(context.Channel);
        if (client != null)
        {
            _server.TriggerClientDisconnect(client);
        }
    }

    public override void ChannelRead(IChannelHandlerContext context, object message)
    {
        if (message is IByteBuffer buffer)
        {
            var packet = PacketSerializer.Deserialize(buffer);
            if (packet != null)
            {
                if (!_isAuthenticated)
                {
                    if (packet is RequestAuthPacket authPacket)
                    {
                        if (string.IsNullOrEmpty(_password) || _password == authPacket.Password)
                        {
                            _isAuthenticated = true;
                            _server.TriggerClientConnect(context.Channel);
                            return;
                        }
                    }

                    context.CloseAsync();
                }
                else
                {
                    var client = _server.GetClientByChannel(context.Channel);
                    if (client != null)
                    {
                        PacketHandler.CallHandlerForServer(client, packet);
                    }
                }
            }
        }
    }

    public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

    public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
    {
        LoggingInterface.ThrowError(exception);
        context.CloseAsync();
    }
}