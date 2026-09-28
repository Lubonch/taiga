namespace Taiga.Sync;

/// <summary>
/// Contrato de proveedor de sincronización (AniList, Kitsu, MyAnimeList).
/// Port de <c>sync::Service</c> (src/sync/service.h).
/// </summary>
public interface ISyncProvider
{
    string Name { get; }
    bool IsConfigured { get; }
    Task<bool> UpdateAsync(SyncQueueItem item, CancellationToken ct = default);
}

/// <summary>
/// Orquesta la cola: intenta enviar cada pendiente; con backoff ante fallos
/// y permanencia en cola sin red. Port del bucle de <c>sync/sync.cpp</c>.
/// </summary>
public sealed class SyncService(SyncQueue queue, ISyncProvider provider)
{
    public async Task<SyncResult> FlushAsync(CancellationToken ct = default)
    {
        var sent = 0;
        var failed = 0;

        if (!provider.IsConfigured)
        {
            return new SyncResult(0, 0, queue.Count, "Proveedor no configurado: los cambios quedan en cola.");
        }

        foreach (var item in queue.Pending())
        {
            if (ct.IsCancellationRequested)
            {
                break;
            }

            var delay = TimeSpan.FromSeconds(Math.Min(300, Math.Pow(2, item.Attempts) * 5));
            if (DateTimeOffset.UtcNow - item.QueuedAt < delay && item.Attempts > 0)
            {
                continue;
            }

            try
            {
                if (await provider.UpdateAsync(item, ct).ConfigureAwait(false))
                {
                    queue.Complete(item);
                    sent++;
                }
                else
                {
                    queue.Fail(item, "El proveedor rechazó la actualización.");
                    failed++;
                }
            }
            catch (Exception ex)
            {
                queue.Fail(item, ex.Message);
                failed++;
            }
        }

        return new SyncResult(sent, failed, queue.Count, null);
    }
}

public sealed record SyncResult(int Sent, int Failed, int Remaining, string? Note);
