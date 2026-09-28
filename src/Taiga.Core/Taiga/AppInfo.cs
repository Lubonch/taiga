using Taiga.Core.Media;

namespace Taiga.Core.Taiga;

/// <summary>
/// Versión de la app .NET. El salto a 3.0.0 marca la reescritura multiplataforma.
/// Port de <c>taiga/version.cpp</c> + <c>taiga/config.h</c>.
/// </summary>
public static class AppInfo
{
    public const string Version = "3.0.0-net10-preview.1";
    public const string Name = "Taiga";
}

/// <summary>
/// Sesión actual: usuario + servicio activo. Port de <c>taiga/session</c> + <c>taiga/accounts</c>.
/// </summary>
public sealed class Session
{
    public string Username { get; set; } = string.Empty;
    public ServiceId Service { get; set; } = ServiceId.MyAnimeList;
    public bool IsAuthenticated { get; set; }
}
