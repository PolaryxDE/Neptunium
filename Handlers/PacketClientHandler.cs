using System;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Neptunium.Client;
using Neptunium.Protocol.Packets;
using Neptunium.Protocol.Serialization;

namespace Neptunium.Handlers;

internal class PacketClientHandler<T> : ChannelHandlerAdapter where T : BaseClient
{
    private bool _isFinished;
    private readonly T _client;
    private readonly string _password;
    private readonly Action<T> _promise;

    internal PacketClientHandler(T client, Action<T> promise, string password)
    {
        _client = client;
        _promise = promise;
        _password = password;
        _isFinished = false;
    }

    public override void ChannelInactive(IChannelHandlerContext context) => _client.OnDisconnect();

    public override void ChannelActive(IChannelHandlerContext context) =>
        context.WriteAndFlushAsync(PacketSerializer.Serialize(new RequestAuthPacket{Password = _password}));

    public override void ChannelRead(IChannelHandlerContext context, object message)
    {
        if (message is IByteBuffer buffer)
        {
            var packet = PacketSerializer.Deserialize(buffer);
            if (packet != null)
            {
                if (packet is AuthFinishPacket finishPacket)
                {
                    _client.ID = finishPacket.ClientID;
                    _isFinished = true;
                    _promise.Invoke(_client);
                }
                else if (_isFinished)
                {
                    PacketHandler.CallHandlerForClient(packet);
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