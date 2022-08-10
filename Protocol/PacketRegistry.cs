using System;
using System.Collections.Generic;
using System.Text;
using DotNetty.Buffers;
using Neptunium.Protocol.Packets;

namespace Neptunium.Protocol;

/// <summary>
/// The packet registry keeps track of all the packets that are registered and can be sent through Neptunium.
/// A packet needs to have a unique ID so the network can identify it, this is the job of the packet registry.
/// When serializing the packet, the ID will be appended at the first byte of the packet.
/// </summary>
public static class PacketRegistry
{
    private static readonly Dictionary<int, Type> PacketIdToType = new();
    private static readonly Dictionary<Type, int> TypeToPacketId = new();
    private static int _currentPacketId = 0;

    static PacketRegistry()
    {
        Register<AuthFinishPacket>();
    }
    
    /// <summary>
    /// Registers the given type as packet type and connects it with the given id.
    /// </summary>
    /// <typeparam name="T">The type of the packet.</typeparam>
    /// <exception cref="NotSupportedException">If the id is already being used.</exception>
    /// <exception cref="ArgumentException">If the given type has no packet id.</exception>
    public static void Register<T>() where T : IPacket
    {
        var type = typeof(T);
        var packetId = _currentPacketId++;

        PacketIdToType.Add(packetId, type);
        TypeToPacketId.Add(type, packetId);
    }
    
    internal static int? GetPacketId(Type type)
    {
        return TypeToPacketId.ContainsKey(type) ? TypeToPacketId[type] : null;
    }
    
    internal static Type? GetPacketType(int packetId)
    {
        return PacketIdToType.ContainsKey(packetId) ? PacketIdToType[packetId] : null;
    }

    internal static void WriteStringUTF8(this IByteBuffer buffer, string data)
    {
        var bytes = Encoding.UTF8.GetBytes(data);
        buffer.WriteInt(bytes.Length);
        buffer.WriteBytes(bytes);
    }

    internal static string ReadStringUTF8(this IByteBuffer buffer)
    {
        var length = buffer.ReadInt();
        var bytes = new byte[length];
        buffer.ReadBytes(bytes);
        return Encoding.UTF8.GetString(bytes);
    }
}