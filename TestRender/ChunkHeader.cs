using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

namespace TestRender;

public class ChunkHeader
{
    //Location Info
    public string LocationName;
    public bool ShowNameTag;
    public string AreaIcon;
    public string InternalName;
    
    //Appearance & Sound
    public int MusicDayId;
    public string MusicDayName;
    
    public int MusicNightId;
    public string MusicNightName;

    public int WeatherId;
    public string WeatherName;
    
    //Map Settings
    public bool CanUseFly;
    public bool CanUseRope;
    public bool CanUseRun;
    public bool CanUseBicycle;

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
                LocationName = item["locationName"]?.ToString(),
                ShowNameTag = item["showNameTag"]?.ToObject<bool>() ?? false,
                AreaIcon = item["areaIcon"]?.ToString(),
                InternalName = item["internalName"]?.ToString(),
                MusicDayId = item["musicDayId"]?.ToObject<int>() ?? 0,
                MusicDayName = item["musicDayName"]?.ToString(),
                MusicNightId = item["musicNightId"]?.ToObject<int>() ?? 0,
                MusicNightName = item["musicNightName"]?.ToString(),
                WeatherId = item["weatherId"]?.ToObject<int>() ?? 0,
                WeatherName = item["weatherName"]?.ToString(),
                CanUseFly = item["canUseFly"]?.ToObject<bool>() ?? false,
                CanUseRope = item["canUseRope"]?.ToObject<bool>() ?? false,
                CanUseRun = item["canUseRun"]?.ToObject<bool>() ?? false,
                CanUseBicycle = item["canUseBicycle"]?.ToObject<bool>() ?? false
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