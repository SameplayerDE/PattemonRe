namespace PatteLib.Gameplay;

[Flags]
public enum TrainerAI
{
    Basic = 0x1,
    Expert = 0x2,
    Risk = 0x4,
    Status = 0x8,
    EvaluateHP = 0x10,
    DamagePriority = 0x20,
    BatonPass = 0x40,
    TagStrategy = 0x80,
    CheckHP = 0x100,
    Weather = 0x200,
    Unknown = 0x400
}