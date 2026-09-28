using System.Text.Json;

namespace Taiga.Sync;

/// <summary>
/// Cola persistente de actualizaciones (modo offline + reintentos con backoff).
/// Port de <c>sync/sync.cpp</c> + <c>media/library/queue.cpp</c>.
/// La persistencia es JSON en el directorio de datos (SQLite en fase posterior).
/// </summary>
public sealed class SyncQueue(string storagePath)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly List<SyncQueueItem> _items = [];
    private readonly object _lock = new();

    public int Count
    {
        get { lock (_lock) { return _items.Count; } }
    }

    public void Enqueue(SyncQueueItem item)
    {
        lock (_lock)
        {
            var existing = _items.FirstOrDefault(i => i.AnimeId == item.AnimeId);
            if (existing is not null)
            {
                existing.Episode = Math.Max(existing.Episode, item.Episode);
                existing.Status = item.Status;
                existing.Score = item.Score != 0 ? item.Score : existing.Score;
                existing.QueuedAt = DateTimeOffset.UtcNow;
            }
            else
            {
                _items.Add(item);
            }

            SaveLocked();
        }
    }

    public IReadOnlyList<SyncQueueItem> Pending()
    {
        lock (_lock)
        {
            return _items.OrderBy(i => i.QueuedAt).ToList();
        }
    }

    public void Complete(SyncQueueItem item)
    {
        lock (_lock)
        {
            _items.Remove(item);
            SaveLocked();
        }
    }

    public void Fail(SyncQueueItem item, string error)
    {
        lock (_lock)
        {
            item.Attempts++;
            item.LastError = error;
            SaveLocked();
        }
    }

    public void Load()
    {
        lock (_lock)
        {
            _items.Clear();
            if (!File.Exists(storagePath))
            {
                return;
            }

            try
            {
                var items = JsonSerializer.Deserialize<List<SyncQueueItem>>(
                    File.ReadAllText(storagePath), JsonOptions);
                if (items is not null)
                {
                    _items.AddRange(items);
                }
            }
            catch
            {
                // Cola corrupta: se empieza vacía para no bloquear el arranque.
            }
        }
    }

    private void SaveLocked()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(storagePath) ?? ".");
        File.WriteAllText(storagePath, JsonSerializer.Serialize(_items, JsonOptions));
    }
}
