namespace Taiga.Core.Infrastructure;

/// <summary>
/// Reloj abstraído para testear expiraciones y colas. Port de <c>base/time.cpp</c>.
/// </summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
