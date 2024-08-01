using Newtonsoft.Json.Linq;

namespace PatteLib.Data;

public static class ExperienceTableHandler
{

    public static Dictionary<string, Dictionary<int, int>> ExperienceTables = [];

   
    public static void LoadData(string path)
    {
        JArray jExpTables = JArray.Parse(File.ReadAllText(path));

        foreach (var jExpTableEntry in jExpTables)
        {
            string expTableId = JsonUtils.GetValue<string>(jExpTableEntry["id"]);
            string expTablePath = JsonUtils.GetValue<string>(jExpTableEntry["path"]);
            string combinedPath = PathUtils.GetCorrectPath(path, expTablePath);
            
            JObject jExpTableData = JObject.Parse(File.ReadAllText(combinedPath));
            Dictionary<int, int> experienceTable = [];
            for (int i = 0; i <= 100; i++)
            {
                int totalExp = JsonUtils.GetValue<int>(jExpTableData[Convert.ToString(i)]);
                experienceTable.Add(i, totalExp);
            }
            ExperienceTables.Add(expTableId, experienceTable);
        }
    }

    public static int GetLevel(string expRate, int currentExp)
    {
        if (!ExperienceTables.TryGetValue(expRate, out var experienceTable))
        {
            throw new ArgumentException($"Experience table with ID '{expRate}' not found.");
        }
        int lastLevel = 0;
        foreach ((int level, int totalExp) in experienceTable)
        {
            if (currentExp < totalExp)
            {
                break;
            }
            lastLevel = level;
        }
        return lastLevel;
    }

    
    public static int GetExp(string expRate, int level)
    {
        if (!ExperienceTables.TryGetValue(expRate, out var experienceTable))
        {
            throw new ArgumentException($"Experience table with ID '{expRate}' not found.");
        }
        return experienceTable[level];
    }
}