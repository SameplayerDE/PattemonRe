namespace PatteLib.Gameplay;

public class Trainer
{
    public int TrainerClassId;
    public string Name;
    public TrainerAI AIFlags;
    public TrainerParty Party;
    public bool DoubleBattle = false;

    public int PartySlots => Party.Count;
}