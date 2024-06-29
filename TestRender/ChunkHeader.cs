using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

namespace TestRender;

public class ChunkHeader
{
    public string Id;
    
    //Location Info
    public string LocationName;
    public bool ShowNameTag;
    public int AreaIcon;
    public string InternalName;
    
    //Appearance & Sound
    public int MusicDayId;
    public int MusicNightId;
    public int WeatherId;
    
    //Map Settings
    public bool CanUseFly;
    public bool CanUseRope;
    public bool CanUseRun;
    public bool CanUseBicycle;
    
    //Matrix
    public int MatrixId;

    public static Dictionary<string, ChunkHeader> Load(string path)
    {
        if (!File.Exists(path))
        {
            throw new Exception("");
        }
        
        using var reader = new StreamReader(path);
        var json = reader.ReadToEnd();
        
        var jArray = JArray.Parse(json);
        
        var result = new Dictionary<string, ChunkHeader>();

        foreach (var item in jArray)
        {
            var headerId = item["headerId"]?.ToString();
            var chunkHeader = new ChunkHeader
            {
                Id = headerId,
                LocationName = item["locationName"]?.ToString(),
                ShowNameTag = item["showNameTag"]?.ToObject<bool>() ?? false,
                AreaIcon = item["musicDayId"]?.ToObject<int>() ?? 0,
                InternalName = item["internalName"]?.ToString(),
                MusicDayId = item["musicDayId"]?.ToObject<int>() ?? 0,
                MusicNightId = item["musicNightId"]?.ToObject<int>() ?? 0,
                WeatherId = item["weatherId"]?.ToObject<int>() ?? 0,
                CanUseFly = item["canUseFly"]?.ToObject<bool>() ?? false,
                CanUseRope = item["canUseRope"]?.ToObject<bool>() ?? false,
                CanUseRun = item["canUseRun"]?.ToObject<bool>() ?? false,
                CanUseBicycle = item["canUseBicycle"]?.ToObject<bool>() ?? false,
                MatrixId = item["matrixId"]?.ToObject<int>() ?? 0
            };

            if (headerId != null)
            {
                result[headerId] = chunkHeader;
            }
            else
            {
                throw new Exception("HeaderId is null.");
            }
        }
        return result;
    }
}