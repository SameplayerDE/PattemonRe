using Newtonsoft.Json.Linq;

namespace PatteLib.World;

public class ChunkHeader
{
    public int Id;
    
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

    public static ChunkHeader Load(JToken jHeader)
    {
        //var header = new ChunkHeader();
        //
        //var headerIdToken = jHeader["headerId"];
        //if (headerIdToken == null)
        //{
        //    throw new Exception();
        //}
        //header.Id = headerIdToken.Value<int>();
        
        var headerId = jHeader["headerId"]?.ToObject<int>() ?? 0;
        var chunkHeader = new ChunkHeader
        {
            Id = headerId,
            LocationName = jHeader["locationName"]?.ToString(),
            ShowNameTag = jHeader["showNameTag"]?.ToObject<bool>() ?? false,
            AreaIcon = jHeader["musicDayId"]?.ToObject<int>() ?? 0,
            InternalName = jHeader["internalName"]?.ToString(),
            MusicDayId = jHeader["musicDayId"]?.ToObject<int>() ?? 0,
            MusicNightId = jHeader["musicNightId"]?.ToObject<int>() ?? 0,
            WeatherId = jHeader["weatherId"]?.ToObject<int>() ?? 0,
            CanUseFly = jHeader["canUseFly"]?.ToObject<bool>() ?? false,
            CanUseRope = jHeader["canUseRope"]?.ToObject<bool>() ?? false,
            CanUseRun = jHeader["canUseRun"]?.ToObject<bool>() ?? false,
            CanUseBicycle = jHeader["canUseBicycle"]?.ToObject<bool>() ?? false,
            MatrixId = jHeader["matrixId"]?.ToObject<int>() ?? 0
        };

        return chunkHeader;
    }
}