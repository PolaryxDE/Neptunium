using System;
using System.Reflection;

namespace Neptunium;

internal class PacketHandlerExecutor
{
    internal string Name => _method.Name;
    
    private readonly object? _owner;
    private readonly MethodInfo _method;

    internal PacketHandlerExecutor(MethodInfo method, object? owner = null)
    {
        _method = method;
        _owner = owner;
    }

    internal void Execute(params object[] args)
    {
        try
        {
            _method.Invoke(_owner, args);
        }
        catch (Exception e)
        {
            LoggingInterface.ThrowError(e);
        }
    }
}