namespace PatteLib.Data;

public class TrainerType
{
    public string Name;
    public int EyeContactMusicId;
    //ToDo: preview images and count of that
    
    private static readonly Dictionary<int, TrainerType> TrainerTypes = new Dictionary<int, TrainerType>
    {
        { 0x02, new TrainerType { Name = "Gör", EyeContactMusicId = 1100 } }, // boy
        { 0x03, new TrainerType { Name = "Gör", EyeContactMusicId = 1110 } }, // girl
        { 0x07, new TrainerType { Name = "Aromalady", EyeContactMusicId = 1104 } },
        { 41, new TrainerType { Name = "Forscher", EyeContactMusicId = 1111 } },
        { 42, new TrainerType { Name = "Schwimmer", EyeContactMusicId = 1111 } }, // male swimmer
        { 43, new TrainerType { Name = "Schwimmer", EyeContactMusicId = 1104 } }, // female swimmer
    };
    
    public static TrainerType GetById(int id)
    {
        if (TrainerTypes.TryGetValue(id, out var trainerType))
        {
            return trainerType;
        }
        throw new KeyNotFoundException($"TrainerType with ID {id} not found.");
    }
    
}