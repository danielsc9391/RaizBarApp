using System.Diagnostics;

namespace RaizBarApp;

internal static class StartupDiagnostics
{
    public static void Log(string message)
    {
        var entry = $"[{DateTimeOffset.UtcNow:O}] {message}";
        Debug.WriteLine(entry);
        Trace.WriteLine(entry);
        Console.WriteLine(entry);
    }

    public static void LogException(string location, Exception exception)
    {
        var message = $"[{DateTimeOffset.UtcNow:O}] Failure in {location}: {exception}";
        Debug.WriteLine(message);
        Trace.WriteLine(message);
        Console.Error.WriteLine(message);
    }
}
