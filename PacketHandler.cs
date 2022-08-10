using System;
using System.Collections.Generic;
using System.Reflection;
using Neptunium.Protocol;

namespace Neptunium;

[AttributeUsage(AttributeTargets.Method)]
public class PacketHandler : Attribute
{
    /// <summary>
    /// Marks a method as packet handler which takes cover of the given packet type.
    /// If a packet is being received the marked method will be called for handling the packet.
    /// Generally the only argument is the incoming <see cref="IPacket"/> but if the packet is
    /// coming from the client and the handler is on the server, the first argument of the handler
    /// must be the <see cref="ClientConnection"/>.
    /// </summary>
    private static readonly Dictionary<Type, PacketHandlerExecutor> RegisteredHandlers = new();
    
    private Type Type { get; }
    /// <summary>
    /// Creates a packet handler.
    /// </summary>
    /// <param name="type">The type of the packet which gets managed by the marking packet handler method.</param>
    public PacketHandler(Type type)
    {
        Type = type;
    }

    public static void Scan<T>()
    {
        foreach (var method in typeof(T).GetRuntimeMethods())
        {
            if (!method.IsStatic) continue;
            var attribute = method.GetCustomAttribute<PacketHandler>();
            if (attribute == null) continue;
            if (RegisteredHandlers.ContainsKey(attribute.Type))
                throw new Exception(
                    $"A packet handler for the type {attribute.Type.FullName} is already registered (existing: {RegisteredHandlers[attribute.Type].Name})");
            RegisteredHandlers.Add(attribute.Type, new PacketHandlerExecutor(method));
        }
    }

    public static void Scan(object obj)
    {
        foreach (var method in obj.GetType().GetRuntimeMethods())
        {
            if (method.IsStatic) continue;
            var attribute = method.GetCustomAttribute<PacketHandler>();
            if (attribute == null) continue;
            if (RegisteredHandlers.ContainsKey(attribute.Type))
                throw new Exception(
                    $"A packet handler for the type {attribute.Type.FullName} is already registered (existing: {RegisteredHandlers[attribute.Type].Name})");
            RegisteredHandlers.Add(attribute.Type, new PacketHandlerExecutor(method, obj));
        }
    }

    internal static void CallHandlerForClient(IPacket packet)
    {
        var type = packet.GetType();
        if (!RegisteredHandlers.ContainsKey(type)) return;
        RegisteredHandlers[type].Execute(packet);
    }

    internal static void CallHandlerForServer(ClientConnection client, IPacket packet)
    {
        var type = packet.GetType();
        if (!RegisteredHandlers.ContainsKey(type)) return;
        RegisteredHandlers[type].Execute(client, packet);
    }
}