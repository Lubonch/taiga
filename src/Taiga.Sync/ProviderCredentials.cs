using System.Text.Json;

namespace Taiga.Sync;

/// <summary>
/// Tokens OAuth por proveedor, persistidos en el directorio de configuración.
/// Nota: en esta fase se guardan en JSON local; el paso a Secret Service
/// (Linux) / Credential Manager (Windows) vía <c>ISecureStorage</c> está
/// previsto en la matriz de paridad.
/// </summary>
public sealed class ProviderCredentials
{
    public string AniListToken { get; set; } = string.Empty;
    public string KitsuToken { get; set; } = string.Empty;
    public string MyAnimeListToken { get; set; } = string.Empty;

    public static ProviderCredentials Load(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                return JsonSerializer.Deserialize<ProviderCredentials>(File.ReadAllText(path))
                    ?? new ProviderCredentials();
            }
        }
        catch
        {
        }

        return new ProviderCredentials();
    }

    public void Save(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
        File.WriteAllText(path, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
    }
}
