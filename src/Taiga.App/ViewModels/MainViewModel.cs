using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Taiga.App.Services;
using Taiga.Core.Media;
using Taiga.Sync;
using Taiga.Track;

namespace Taiga.App.ViewModels;

/// <summary>
/// ViewModel principal: biblioteca, reproducción actual y sincronización.
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    private readonly AppServices _services;
    private readonly SyncService _sync;

    public MainViewModel()
        : this(new AppServices())
    {
    }

    public MainViewModel(AppServices services)
    {
        _services = services;
        _sync = new SyncService(services.Queue, services.ActiveProvider());
        RefreshLibrary();
        Status = $"Biblioteca: {Library.Count} títulos · Cola: {services.Queue.Count} pendientes";
    }

    public ObservableCollection<AnimeItem> Library { get; } = [];

    [ObservableProperty]
    public partial AnimeItem? SelectedAnime { get; set; }

    [ObservableProperty]
    public partial string NowPlaying { get; set; } = "Sin reproducción detectada";

    [ObservableProperty]
    public partial string Status { get; set; } = string.Empty;

    [RelayCommand]
    public async Task ScanNowAsync()
    {
        Status = "Detectando reproducción...";
        foreach (var detector in _services.Detectors)
        {
            var state = await detector.DetectAsync().ConfigureAwait(false);
            if (state is null || !state.IsPlaying)
            {
                continue;
            }

            var source = string.IsNullOrEmpty(state.MediaPath) ? state.MediaTitle : state.MediaPath;
            var episode = FilenameParser.Parse(source, fromPath: !string.IsNullOrEmpty(state.MediaPath));
            episode.Player = state.Player;
            var outcome = _services.Decider.Decide(episode);
            NowPlaying = $"{state.Player}: {outcome.Reason}";
            if (outcome.ShouldUpdate)
            {
                _services.SaveAll();
                RefreshLibrary();
            }

            Status = $"Biblioteca: {Library.Count} títulos · Cola: {_services.Queue.Count} pendientes";
            return;
        }

        NowPlaying = "Sin reproducción detectada";
        Status = $"Biblioteca: {Library.Count} títulos · Cola: {_services.Queue.Count} pendientes";
    }

    [RelayCommand]
    public async Task SyncNowAsync()
    {
        Status = "Sincronizando...";
        var result = await _sync.FlushAsync().ConfigureAwait(false);
        Status = result.Note
            ?? $"Enviados: {result.Sent} · Fallidos: {result.Failed} · Pendientes: {result.Remaining}";
    }

    public void RefreshLibrary()
    {
        Library.Clear();
        foreach (var anime in _services.Library.CurrentlyWatching()
                     .Concat(_services.Library.All().Where(a => a.IsInList && a.MyStatus != MyStatus.Watching))
                     .OrderBy(a => a.Title))
        {
            Library.Add(anime);
        }
    }
}
