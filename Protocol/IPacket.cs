using DotNetty.Buffers;

namespace Neptunium.Protocol;

/// <summary>
/// The packet is a container for data which gets transported over the network from client to server
/// or vice versa. Packets have two methods which will either load packet data from a <see cref="IByteBuffer"/>
/// or writes data to the packet buffer.
/// </summary>
public interface IPacket
{
    
}