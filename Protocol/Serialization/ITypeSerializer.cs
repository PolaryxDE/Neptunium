using System;
using DotNetty.Buffers;

namespace Neptunium.Protocol.Serialization;

/// <summary>
/// The type serializer takes a type and (de)serializes it to or from a byte array.
/// </summary>
public interface ITypeSerializer
{
    /// <summary>
    /// Can this serializer serialize the given type?
    /// </summary>
    bool CanSerialize(Type type);

    /// <summary>
    /// Serializes the given object and writes it to the given buffer.
    /// </summary>
    void Serialize(Type type, object obj, IByteBuffer buffer);
    
    /// <summary>
    /// Deserializes the wanted object from the given buffer.
    /// </summary>
    object? Deserialize(Type type, IByteBuffer buffer);
}