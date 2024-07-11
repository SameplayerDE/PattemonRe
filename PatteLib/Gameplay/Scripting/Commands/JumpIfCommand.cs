namespace PatteLib.Gameplay.Scripting.Commands;

public class JumpIfCommand : ICommand
{
    private string _condition;
    private string _label;

    public JumpIfCommand(string condition, string label)
    {
        _condition = condition;
        _label = label;
    }

    public void Execute(ScriptProcessor processor)
    {
        if ((_condition == "EQUAL" && processor.GetComparisonResult()) ||
            (_condition == "NOT_EQUAL" && !processor.GetComparisonResult()))
        {
            processor.ExecuteSection(_label);
        }
    }
}