using Neptunium.Protocol.Serialization;

namespace Neptunium.Protocol.Packets;

internal class RequestAuthPacket : IPacket
{
    [Field(0)]
    public string Password { get; set; }
}