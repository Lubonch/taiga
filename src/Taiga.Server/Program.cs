using System.Net;
using Taiga.Core.Platform;
using Taiga.Server;

if (args.Contains("--self-test"))
{
    return ServerSelfTest.Run();
}

// TAIGA_DATA_DIR: base portable para tests/empaquetado (con `.portable`).
// TAIGA_TOKEN: token fijo (tests, Electron lo lee de stdout si es efímero).
// --port N: puerto (Electron elige uno libre).
var baseDir = Environment.GetEnvironmentVariable("TAIGA_DATA_DIR");
var state = new AppState(baseDir is null ? null : new PathProvider(baseDir));
var token = Environment.GetEnvironmentVariable("TAIGA_TOKEN")
    ?? Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    // El sidecar corre con cualquier CWD (Electron): wwwroot va junto al binario.
    ContentRootPath = AppContext.BaseDirectory,
});
builder.Services.AddSingleton(state);
var app = builder.Build();

// Sirve el front Angular embebido (frontend/dist copiado a wwwroot en Release).
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseWebSockets();

// Token para todo /api.
app.Use(async (ctx, next) =>
{
    if (ctx.Request.Path.StartsWithSegments("/api") &&
        ctx.Request.Headers["X-Taiga-Token"] != token)
    {
        ctx.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        await ctx.Response.WriteAsync("missing or invalid X-Taiga-Token").ConfigureAwait(false);
        return;
    }

    await next().ConfigureAwait(false);
});

Endpoints.Map(app, state);

var portArg = args.SkipWhile(a => a != "--port").Skip(1).FirstOrDefault();
var port = int.TryParse(portArg, out var p) ? p : 6599;
Console.WriteLine($"TAIGA_TOKEN={token}");
Console.Out.Flush();

app.Run($"http://127.0.0.1:{port}/");
return 0;

public partial class Program
{
}
