namespace Taiga.Core.Infrastructure;

/// <summary>
/// Registro en fichero con rotación simple. Port de <c>base/log.h</c>.
/// </summary>
public sealed class FileLogger(string logPath) : ILogger
{
    private readonly object _lock = new();

    public void Info(string message) => Write("INFO", message);
    public void Warning(string message) => Write("WARN", message);
    public void Error(string message) => Write("ERROR", message);

    private void Write(string level, string message)
    {
        lock (_lock)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(logPath) ?? ".");
            File.AppendAllText(logPath, $"{DateTimeOffset.UtcNow:u} [{level}] {message}{Environment.NewLine}");
        }
    }
}

/// <summary>
/// Registro nulo para tests.
/// </summary>
public sealed class NullLogger : ILogger
{
    public void Info(string message) { }
    public void Warning(string message) { }
    public void Error(string message) { }
}

public interface ILogger
{
    void Info(string message);
    void Warning(string message);
    void Error(string message);
}
