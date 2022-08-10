using System;

namespace Neptunium.Protocol.Serialization;

/// <summary>
/// Marks the given property as protocol field which will be serialized when the packet is being sent.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class Field : Attribute
{
    /// <summary>
    /// The order of the field in the packet buffer.
    /// </summary>
    public int Order { get; set; }

    public Field(int order)
    {
        Order = order;
    }
}