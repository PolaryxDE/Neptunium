using Neptunium.Protocol.Serialization;

namespace Neptunium.Protocol.Packets;

internal class AuthFinishPacket : IPacket
{
    [Field(0)]
    public long ClientID { get; set; }
}