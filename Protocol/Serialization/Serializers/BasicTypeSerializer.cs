using System;
using DotNetty.Buffers;

namespace Neptunium.Protocol.Serialization.Serializers;

/// <summary>
/// The basic type serializer is a <see cref="ITypeSerializer"/> which serializes every basic type (e.g. int, string, etc.).
/// </summary>
public class BasicTypeSerializer : ITypeSerializer
{
    /// <summary>
    /// The current instance of the <see cref="BasicTypeSerializer"/>.
    /// </summary>
    public static BasicTypeSerializer Instance { get; } = new();
    
    private static readonly Type UShortType = typeof(ushort);
    private static readonly Type IntType = typeof(int);
    private static readonly Type LongType = typeof(long);
    private static readonly Type ShortType = typeof(short);
    private static readonly Type ByteType = typeof(byte);
    private static readonly Type CharType = typeof(char);
    private static readonly Type FloatType = typeof(float);
    private static readonly Type DoubleType = typeof(double);
    private static readonly Type BoolType = typeof(bool);
    private static readonly Type StringType = typeof(string);
    
    public bool CanSerialize(Type type)
    {
        return type == UShortType || type == IntType || type == LongType || type == ShortType 
               || type == ByteType || type == CharType || type == FloatType || type == DoubleType 
               || type == BoolType || type == StringType;
    }

    public void Serialize(Type type, object obj, IByteBuffer buffer)
    {
        if (type == UShortType)
        {
            buffer.WriteUnsignedShort((ushort) obj);
        }
        else if (type == IntType)
        {
            buffer.WriteInt((int) obj);
        }
        else if (type == LongType)
        {
            buffer.WriteLong((long) obj);
        }
        else if (type == ShortType)
        {
            buffer.WriteShort((short) obj);
        }
        else if (type == ByteType)
        {
            buffer.WriteByte((byte) obj);
        }
        else if (type == CharType)
        {
            buffer.WriteChar((char) obj);
        }
        else if (type == FloatType)
        {
            buffer.WriteFloat((float) obj);
        }
        else if (type == DoubleType)
        {
            buffer.WriteDouble((double) obj);
        }
        else if (type == BoolType)
        {
            buffer.WriteBoolean((bool) obj);
        }
        else if (type == StringType)
        {
            buffer.WriteStringUTF8((string) obj);
        }
    }

    public object? Deserialize(Type type, IByteBuffer buffer)
    {
        if (type == UShortType)
        {
            return buffer.ReadUnsignedInt();
        }
        
        if (type == IntType)
        {
            return buffer.ReadInt();
        }
        
        if (type == LongType)
        {
            return buffer.ReadLong();
        }
        
        if (type == ShortType)
        {
            return buffer.ReadShort();
        }
        
        if (type == ByteType)
        {
            return buffer.ReadByte();
        }
        
        if (type == CharType)
        {
            return buffer.ReadChar();
        }
        
        if (type == FloatType)
        {
            return buffer.ReadFloat();
        }
        
        if (type == DoubleType)
        {
            return buffer.ReadDouble();
        }
        
        if (type == BoolType)
        {
            return buffer.ReadBoolean();
        }
        
        if (type == StringType)
        {
            return buffer.ReadStringUTF8();
        }

        return null;
    }
}