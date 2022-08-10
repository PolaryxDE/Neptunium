using System;
using DotNetty.Buffers;

namespace Neptunium.Protocol.Serialization.Serializers;

/// <summary>
/// The serializer for serializing any array of objects.
/// </summary>
public class ArrayTypeSerializer : ITypeSerializer
{
    /// <summary>
    /// The current instance of the <see cref="ArrayTypeSerializer"/>.
    /// </summary>
    public static ArrayTypeSerializer Instance { get; } = new();
    
    public bool CanSerialize(Type type)
    {
        return type.IsArray;
    }

    public void Serialize(Type type, object obj, IByteBuffer buffer)
    {
        var elementType = type.GetElementType();
        if (elementType == null)
        {
            return;
        }

        var elementSerializer = PacketSerializer.GetTypeSerializer(elementType);
        if (elementSerializer == null)
        {
            return;
        }
        
        var elementsCount = ((Array) obj).Length;
        buffer.WriteInt(elementsCount);

        foreach (var element in (Array) obj)
        {
            elementSerializer.Serialize(elementType, element, buffer);
        }
    }

    public object? Deserialize(Type type, IByteBuffer buffer)
    {
        var elementType = type.GetElementType();
        if (elementType == null)
        {
            return null;
        }

        var elementDeserializer = PacketSerializer.GetTypeSerializer(elementType);
        if (elementDeserializer == null)
        {
            return null;
        }
        
        var elementsCount = buffer.ReadInt();
        var array = Array.CreateInstance(elementType, elementsCount);
        
        for (int i = 0; i < elementsCount; i++)
        {
            var item = elementDeserializer.Deserialize(elementType, buffer);
            if (item == null)
            {
                continue;
            }

            array.SetValue(item, i);
        }

        return array;
    }
}