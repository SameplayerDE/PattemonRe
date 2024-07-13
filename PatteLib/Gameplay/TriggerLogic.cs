namespace PatteLib.Gameplay;

public struct TriggerLogic
{
    public int VariableWatched;
    public int ExpectedValue;
    public int ScriptToTrigger;

    public TriggerLogic(int variableWatched, int expectedValue, int scriptToTrigger)
    {
        VariableWatched = variableWatched;
        ExpectedValue = expectedValue;
        ScriptToTrigger = scriptToTrigger;
    }

    public override string ToString()
    {
        return $"VariableWatched: {VariableWatched}, ExpectedValue: {ExpectedValue}, ScriptToTrigger: {ScriptToTrigger}";
    }
}