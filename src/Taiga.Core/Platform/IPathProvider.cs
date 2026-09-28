namespace Taiga.Core.Platform;

/// <summary>
/// Resuelve directorios de datos/configuración por SO.
/// Portable (fichero <c>.portable</c> junto al binario) &gt; XDG en Linux &gt; %AppData% en Windows.
/// Port de <c>src/taiga/path.cpp</c> + <c>src/base/file.cpp</c> + <c>TAIGA_PORTABLE</c>.
/// </summary>
public interface IPathProvider
{
    string GetAppDataDirectory();
    string GetConfigDirectory();
    bool IsPortableMode();
}
