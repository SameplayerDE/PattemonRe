using System.Net.Mime;

namespace PatteLib.Data;

public struct TextArchive(int id, string[] content)
{
    public int Id = id;
    public string[] Content { get; private set; } = content;
    public int Lines => Content.Length;

    public static TextArchive Load(string root, int index)
    {
        if (string.IsNullOrEmpty(root))
        {
            throw new Exception();
        }
        var combinedPath = Path.Combine(root, $"{index}.txt");
        if (!File.Exists(combinedPath))
        {
            throw new FileNotFoundException();
        }
        var content = File.ReadAllLines(combinedPath);
        return new TextArchive(index, content);
    }
    
    public string Get(int index)
    {
        if (index < 0 || index >= Lines)
        {
            throw new IndexOutOfRangeException("Index is out of range.");
        }

        return Content[index];
    }
}