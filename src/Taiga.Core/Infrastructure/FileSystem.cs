namespace Taiga.Core.Infrastructure;

/// <summary>
/// Sistema de ficheros abstraído para tests. Port de <c>base/file.cpp</c>.
/// </summary>
public interface IFileSystem
{
    bool Exists(string path);
    string ReadAllText(string path);
    void WriteAllText(string path, string contents);
    void CreateDirectory(string path);
}

public sealed class SystemFileSystem : IFileSystem
{
    public bool Exists(string path) => File.Exists(path);
    public string ReadAllText(string path) => File.ReadAllText(path);

    public void WriteAllText(string path, string contents)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
        File.WriteAllText(path, contents);
    }

    public void CreateDirectory(string path) => Directory.CreateDirectory(path);
}
