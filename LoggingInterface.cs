using System;

namespace Neptunium;

/// <summary>
/// The logging interface is used for as a bridge to log specific things. Neptunium has no logger implemented and
/// can't log anything. This class is used to bridging this problem by delivering a solution to integration
/// layer via events.
/// </summary>
public static class LoggingInterface
{
    /// <summary>
    /// Gets called when Neptunium has thrown an exception.
    /// </summary>
    public static event Action<Exception>? Error;

    internal static void ThrowError(Exception exception)
    {
        Error?.Invoke(exception);
    }
}