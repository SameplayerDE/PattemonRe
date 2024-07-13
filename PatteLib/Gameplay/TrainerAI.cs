namespace PatteLib.Gameplay;

[Flags]
public enum TrainerAI
{
    Basic,
    Expert,
    Risk,
    Status,
    EvaluateHP,
    DamagePriority,
    BatonPass,
    TagStrategy,
    CheckHP,
    Weather,
    Unknown
}