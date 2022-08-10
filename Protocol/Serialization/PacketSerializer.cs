using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotNetty.Buffers;
using Neptunium.Protocol.Serialization.Serializers;

namespace Neptunium.Protocol.Serialization;

/// <summary>
/// The packet serializer takes care of serialization and deserialization of packets.
/// </summary>
public static class PacketSerializer
{
    /// <summary>
    /// A list containing all the registered <see cref="ITypeSerializer"/>s.
    /// </summary>
    public static List<ITypeSerializer> RegisteredTypeSerializers { get; } = new()
    {
        BasicTypeSerializer.Instance, EnumTypeSerializer.Instance, ClassTypeSerializer.Instance, 
        ListTypeSerializer.Instance, ArrayTypeSerializer.Instance
    };

    internal static IByteBuffer Serialize(IPacket packet)
    {
        var packetId = PacketRegistry.GetPacketId(packet.GetType());
        if (packetId == null)
        {
            throw new ArgumentException("The packet type is not registered in the packet registry.", nameof(packet));
        }

        var buffer = Unpooled.Buffer();
        buffer.WriteInt(packetId.Value);
        SerializeClass(buffer, packet.GetType(), packet);
        return buffer;
    }

    internal static IPacket? Deserialize(IByteBuffer buffer)
    {
        var packetId = buffer.ReadInt();
        var packetType = PacketRegistry.GetPacketType(packetId);
        if (packetType == null)
        {
            return null;
        }
        
        return DeserializeClass(buffer, packetType) as IPacket;
    }

    internal static void SerializeClass(IByteBuffer buffer, Type @class, object instance)
    {
        foreach (var property in @class.GetProperties().Where(p => p.GetCustomAttribute<Field>() != null)
                     .OrderBy(p => p.GetCustomAttribute<Field>()?.Order ?? 0))
        {
            var serializer = GetTypeSerializer(property.PropertyType);
            if (serializer == null)
            {
                continue;
            }

            var value = property.GetValue(instance);
            if (value == null)
            {
                continue;
            }
            
            serializer.Serialize(property.PropertyType, value, buffer);
        }
    }

    internal static object? DeserializeClass(IByteBuffer buffer, Type @class)
    {
        var instance = Activator.CreateInstance(@class);

        foreach (var property in @class.GetProperties().Where(p => p.GetCustomAttribute<Field>() != null)
                     .OrderBy(p => p.GetCustomAttribute<Field>()?.Order ?? 0))
        {
            var deserializer = GetTypeSerializer(property.PropertyType);

            var value = deserializer?.Deserialize(property.PropertyType, buffer);
            if (value == null)
            {
                continue;
            }
            
            property.SetValue(instance, value);
        }
        
        return instance;
    }
    
    internal static ITypeSerializer? GetTypeSerializer(Type type)
    {
        return RegisteredTypeSerializers.FirstOrDefault(s => s.CanSerialize(type));
    }
}