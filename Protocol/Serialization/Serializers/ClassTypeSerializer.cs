using System;
using DotNetty.Buffers;

namespace Neptunium.Protocol.Serialization.Serializers;

/// <summary>
/// The class type serializer serializes classes into the byte buffer.
/// </summary>
public class ClassTypeSerializer : ITypeSerializer
{
    /// <summary>
    /// The current instance of the <see cref="ClassTypeSerializer"/>.
    /// </summary>
    public static ClassTypeSerializer Instance { get; } = new();
    
    public bool CanSerialize(Type type)
    {
        return type.IsClass;
    }

    public void Serialize(Type type, object obj, IByteBuffer buffer)
    {
        PacketSerializer.SerializeClass(buffer, type, obj);
    }

    public object? Deserialize(Type type, IByteBuffer buffer)
    {
        return PacketSerializer.DeserializeClass(buffer, type);
    }
}