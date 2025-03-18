using Newtonsoft.Json.Linq;

namespace PatteLib.World;

public record ChunkHeader
{
    public int Id;
    
    //Location Info
    //public string LocationName;
    public int LocationNameId;
    public bool ShowNameTag;
    public int AreaIcon;
    //public string InternalName;
    
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
    
    //Gameplay
    public int ScriptFileId;
    public int LevelScriptId;
    public int TextArchiveId;
    public int EventFileId;

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
            //LocationName = jHeader["locationName"]?.ToString(),
            LocationNameId = jHeader["locationNameId"]?.ToObject<int>() ?? 0,
            ShowNameTag = jHeader["showNameTag"]?.ToObject<bool>() ?? false,
            AreaIcon = jHeader["musicDayId"]?.ToObject<int>() ?? 0,
            //InternalName = jHeader["internalName"]?.ToString(),
            MusicDayId = jHeader["musicDayId"]?.ToObject<int>() ?? 0,
            MusicNightId = jHeader["musicNightId"]?.ToObject<int>() ?? 0,
            WeatherId = jHeader["weatherId"]?.ToObject<int>() ?? 0,
            CanUseFly = jHeader["canUseFly"]?.ToObject<bool>() ?? false,
            CanUseRope = jHeader["canUseRope"]?.ToObject<bool>() ?? false,
            CanUseRun = jHeader["canUseRun"]?.ToObject<bool>() ?? false,
            CanUseBicycle = jHeader["canUseBicycle"]?.ToObject<bool>() ?? false,
            MatrixId = jHeader["matrixId"]?.ToObject<int>() ?? 0,
            EventFileId = jHeader["eventFileId"]?.ToObject<int>() ?? 0,
            ScriptFileId = jHeader["scriptFileId"]?.ToObject<int>() ?? 0,
            LevelScriptId = jHeader["levelScriptId"]?.ToObject<int>() ?? 0,
            TextArchiveId = jHeader["textArchiveId"]?.ToObject<int>() ?? 0
        };

        return chunkHeader;
    }
    
    public override string ToString()
    {
        return string.Join(Environment.NewLine, new[]
        {
            "ChunkHeader:",
            $"  Id: {Id}",
            $"  Location: {LocationNameId}",
            //$"  Location: {LocationName} (Internal: {InternalName})",
            $"  Show Name Tag: {ShowNameTag}",
            $"  Area Icon: {AreaIcon}",
            $"  Music (Day/Night): {MusicDayId}/{MusicNightId}",
            $"  Weather: {WeatherId}",
            $"  Usable Features: Fly={CanUseFly}, Rope={CanUseRope}, Run={CanUseRun}, Bicycle={CanUseBicycle}",
            $"  Matrix Id: {MatrixId}",
            $"  Gameplay - Script: {ScriptFileId}, LevelScript: {LevelScriptId}, TextArchive: {TextArchiveId}, EventFile: {EventFileId}"
        });
    }
}