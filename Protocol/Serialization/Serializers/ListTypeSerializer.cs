using System;
using System.Collections;
using System.Collections.Generic;
using DotNetty.Buffers;

namespace Neptunium.Protocol.Serialization.Serializers;

/// <summary>
/// The serializer for serializing any list of objects.
/// </summary>
public class ListTypeSerializer : ITypeSerializer
{
    /// <summary>
    /// The current instance of the <see cref="ListTypeSerializer"/>.
    /// </summary>
    public static ListTypeSerializer Instance { get; } = new();
    
    private static readonly Type ListType = typeof(List<>);

    public bool CanSerialize(Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == ListType;
    }

    public void Serialize(Type type, object obj, IByteBuffer buffer)
    {
        var itemType = type.GetGenericArguments()[0];
        var itemSerializer = PacketSerializer.GetTypeSerializer(itemType);
        if (itemSerializer == null)
        {
            return;
        }
        
        var itemsCount = ((IList) obj).Count;
        buffer.WriteInt(itemsCount);
        
        foreach (var item in (IList) obj)
        {
            itemSerializer.Serialize(item.GetType(), item, buffer);
        }
    }

    public object? Deserialize(Type type, IByteBuffer buffer)
    {
        var itemType = type.GetGenericArguments()[0];
        var itemDeserializer = PacketSerializer.GetTypeSerializer(itemType);
        if (itemDeserializer == null)
        {
            return null;
        }
        
        var itemsCount = buffer.ReadInt();
        var constructedListType = ListType.MakeGenericType(itemType);

        if (Activator.CreateInstance(constructedListType) is not IList list)
        {
            return null;
        }
        
        for (int i = 0; i < itemsCount; i++)
        {
            var item = itemDeserializer.Deserialize(itemType, buffer);
            if (item == null)
            {
                continue;
            }
            
            list.Add(item);
        }

        return list;
    }
}