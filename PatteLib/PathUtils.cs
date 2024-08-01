namespace PatteLib;

public class PathUtils
{
    public static string GetCorrectPath(string parentPath, string childPath)
    {
        return Path.IsPathRooted(childPath) ? childPath : Path.Combine(Path.GetDirectoryName(parentPath), childPath);

    }
}