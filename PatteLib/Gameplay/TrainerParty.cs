namespace PatteLib.Gameplay;

public class TrainerParty
{
    //ToDo: duo battle settings
    private const int MaxCount = 6;
    public TrainerPartyMember[] PartyMembers = new TrainerPartyMember[MaxCount];
    public int Count { get; private set; }
    public bool HasDifferentMoves = false;
}