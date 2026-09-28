using Taiga.Core.Settings;

namespace Taiga.Core.Compat;

/// <summary>
/// Importador solo-lectura de ajustes legacy v1 (formato INI <c>clave=valor</c>).
/// Port del concepto <c>taiga::Settings::HandleCompatibility</c>
/// (src/compat/settings.cpp): migra lo recuperable y deja el resto por defecto.
/// </summary>
public static class V1Importer
{
    public static int Import(string legacyIniPath, AppSettings target)
    {
        if (!File.Exists(legacyIniPath))
        {
            return 0;
        }

        var imported = 0;
        foreach (var line in File.ReadAllLines(legacyIniPath))
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith('#') || !trimmed.Contains('='))
            {
                continue;
            }

            var key = trimmed[..trimmed.IndexOf('=')].Trim().ToLowerInvariant();
            var value = trimmed[(trimmed.IndexOf('=') + 1)..].Trim();

            switch (key)
            {
                case "username":
                    target.Username = value;
                    imported++;
                    break;
                case "service":
                    target.Service = value;
                    imported++;
                    break;
                case "library_folder":
                    target.LibraryFolders.Add(value);
                    imported++;
                    break;
            }
        }

        return imported;
    }
}
