using Taiga.Core.Media;

namespace Taiga.Sync;

/// <summary>
/// Actualización pendiente de enviar al proveedor.
/// Port de <c>library::QueueItem</c> (src/media/library/queue.h).
/// </summary>
public sealed class SyncQueueItem
{
    public int AnimeId { get; set; }
    public int Episode { get; set; }
    public MyStatus Status { get; set; } = MyStatus.Watching;
    public int Score { get; set; }
    public DateTimeOffset QueuedAt { get; set; } = DateTimeOffset.UtcNow;
    public int Attempts { get; set; }
    public string? LastError { get; set; }
}
