using System;
using DotNetty.Buffers;

namespace Neptunium.Protocol.Serialization.Serializers;

/// <summary>
/// The enum type serializer serializes an enum into an integer and than into the buffer.
/// </summary>
public class EnumTypeSerializer : ITypeSerializer
{
    /// <summary>
    /// The current instance of the <see cref="EnumTypeSerializer"/>.
    /// </summary>
    public static EnumTypeSerializer Instance { get; } = new();
    
    public bool CanSerialize(Type type)
    {
        return type.IsEnum;
    }

    public void Serialize(Type type, object obj, IByteBuffer buffer)
    {
        buffer.WriteInt((int) obj);
    }

    public object Deserialize(Type type, IByteBuffer buffer)
    {
        return Enum.ToObject(type, buffer.ReadInt());
    }
}