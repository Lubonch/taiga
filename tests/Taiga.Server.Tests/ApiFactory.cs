using Microsoft.AspNetCore.Mvc.Testing;

namespace Taiga.Server.Tests;

/// <summary>
/// Factoría con directorio portable temporal y token fijo.
/// Siembra una biblioteca con un anime (id 1, Naruto).
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IDisposable
{
    public const string Token = "test-token-123";
    private readonly string _dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    public ApiFactory()
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(Path.Combine(_dir, ".portable"), string.Empty);
        var data = Path.Combine(_dir, "data");
        Directory.CreateDirectory(data);
        File.WriteAllText(Path.Combine(data, "library.json"), """
            [{"id":1,"title":"Naruto","episodeCount":220,"isInList":true,
              "myStatus":1,"watchedEpisodes":11}]
            """);
        Environment.SetEnvironmentVariable("TAIGA_DATA_DIR", _dir);
        Environment.SetEnvironmentVariable("TAIGA_TOKEN", Token);
    }

    public new void Dispose()
    {
        base.Dispose();
        try
        {
            Directory.Delete(_dir, recursive: true);
        }
        catch
        {
        }
    }
}
