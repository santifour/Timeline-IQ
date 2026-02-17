using System.IO;

namespace ProjectTimeEstimator.Services;

public static class Logger
{
    private static readonly string LogPath = Path.Combine(AppContext.BaseDirectory, "log.txt");

    public static void Log(string message)
    {
        try
        {
            var logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {message}{Environment.NewLine}";
            File.AppendAllText(LogPath, logEntry);
        }
        catch
        {
            // Logging failed, suppress to avoid crashing the app
        }
    }

    public static void LogError(Exception ex, string context)
    {
        Log($"ERROR [{context}]: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
    }
}
