using Newtonsoft.Json.Linq;
using PatteLib.Gameplay;

namespace PatteLib.Data;

public static class EventContainerLoader
{
    public static EventContainer Load(string path)
    {

        if (!File.Exists(path))
        {
            return null;
            //throw new FileNotFoundException();
        }
        
        var result = new EventContainer();

        var jsonData = File.ReadAllText(path);
        var jEventToken = JToken.Parse(jsonData);

        var jTriggers = jEventToken["triggers"];
        
        if (jTriggers != null)
        {
            foreach (var jTriggerToken in (JArray)jTriggers)
            {
                var id = jTriggerToken["id"].Value<int>();
                var cx = jTriggerToken["cx"].Value<int>();
                var cy = jTriggerToken["cy"].Value<int>();
                var cz = jTriggerToken["cz"].Value<int>();
                var mx = jTriggerToken["mx"].Value<int>();
                var my = jTriggerToken["my"].Value<int>();
                var wx = jTriggerToken["wx"].Value<int>();
                var wy = jTriggerToken["wy"].Value<int>();
                var watch = jTriggerToken["logic"]["watch"].Value<int>();
                var expect = jTriggerToken["logic"]["expect"].Value<int>();
                var execute = jTriggerToken["logic"]["execute"].Value<int>();

                result.Triggers.Add(new Trigger()
                {
                    Id = id,
                    MatrixX = mx,
                    MatrixY = my,
                    ChunkX = cx,
                    ChunkY = cy,
                    ChunkZ = cz,
                    WidthX = wx,
                    WidthY = wy,
                    Logic = new TriggerLogic()
                    {
                        VariableWatched = watch,
                        ExpectedValue = expect,
                        ScriptToTrigger = execute
                    }
                });

            }
        }

        foreach (var jEntityToken in (JArray)jEventToken["entities"])
        {
            var id = jEntityToken["id"].Value<int>();
            var cx = jEntityToken["cx"].Value<int>();
            var cy = jEntityToken["cy"].Value<int>();
            var cz = jEntityToken["cz"].Value<int>();
            var mx = jEntityToken["mx"].Value<int>();
            var my = jEntityToken["my"].Value<int>();
            
            var flag = jEntityToken["flag"].Value<int>();
            var sprite = jEntityToken["sprite"].Value<int>();
            var execute = jEntityToken["execute"].Value<int>();
            
            result.Overworlds.Add(new OverWorld()
            {
                Id = id,
                MatrixX = mx,
                MatrixY = my,
                ChunkX = cx,
                ChunkY = cy,
                ChunkZ = cz,
                Flag = flag,
                EntryId = sprite,
                Script = execute
            });
        }
        
        return result;
    }
}