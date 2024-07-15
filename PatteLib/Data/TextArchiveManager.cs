using Microsoft.Xna.Framework.Graphics;

namespace PatteLib.Data;

public class TextArchiveManager : IDisposable
{
    private readonly Dictionary<int, TextArchive> _archives = new();
    public static string RootDirectory { get; set; } = string.Empty; //ToDo: Use a property for better access control

    public void Load(int index)
    {
        if (string.IsNullOrEmpty(RootDirectory))
        {
            throw new Exception("Root directory cannot be empty.");
        }

        var archive = TextArchive.Load(RootDirectory, index);
        _archives.Add(index, archive);
    }

    public string GetLine(int index, int line)
    {
        if (!_archives.TryGetValue(index, out var archive))
        {
            throw new KeyNotFoundException($"Text archive with index {index} not found.");
        }
        return archive.Get(line);
    }
    
    public TextArchive GetArchive(int index)
    {
        if (!_archives.TryGetValue(index, out var archive))
        {
            throw new KeyNotFoundException($"Text archive with index {index} not found.");
        }
        return archive;
    }

    public string GetAllLines(int index)
    {
        if (!_archives.TryGetValue(index, out var archive))
        {
            throw new KeyNotFoundException($"Text archive with index {index} not found.");
        }

        return string.Join(Environment.NewLine, archive.Content);
    }

    public void Unload(int index)
    {
        Console.WriteLine(_archives.Remove(index)
            ? $"Text archive with index {index} unloaded."
            : $"Text archive with index {index} not found for unloading.");
    }

    private void UnloadAll()
    {
        _archives.Clear();
        Console.WriteLine("All text archives unloaded.");
    }

    public void Dispose()
    {
        UnloadAll();
        GC.SuppressFinalize(this);
    }
}