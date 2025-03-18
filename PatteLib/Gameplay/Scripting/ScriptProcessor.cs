using PatteLib.Data;

namespace PatteLib.Gameplay.Scripting;

public class ScriptProcessor
{
    private Dictionary<string, ScriptSection> _sections = [];

    private bool _comparisonResult;
    private Stack<(string section, int pointer)> _callStack = [];
    private int _pointer;
    private string _section;
    private bool _sectionEnded;
    
    public void ParseScript(string[] script)
    {
        List<ICommand> currentSectionCommands = [];

        foreach (var line in script)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }
            if (line.EndsWith(":"))
            {
                string sectionName = line.TrimEnd(':');
                currentSectionCommands = [];
                _sections[sectionName] = new ScriptSection(currentSectionCommands);
            }
            else if (currentSectionCommands != null)
            {
                try
                {
                    if (CommandFactory.TryCreateCommand(line, out ICommand? command))
                    {
                        currentSectionCommands.Add(command!);
                    }
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }

    public void ExecuteSection(string sectionName)
    {
        sectionName = sectionName.Replace("#", " ");
        _section = sectionName;
        if (_sections.TryGetValue(sectionName, out var section))
        {
            _sectionEnded = false;
            while (_pointer < section.Commands.Count && !_sectionEnded)
            {
                section.Commands[_pointer].Execute(this);
                _pointer++;
            }
        }
        else
        {
            Console.WriteLine($"Section {sectionName} not found");
        }
    }

    public int GetVariable(int address)
    {
        return MemoryContext.GetVariable(address);
    }

    public void SetVariable(int address, int value)
    {
        MemoryContext.SetVariable(address, value);
    }

    public bool GetComparisonResult()
    {
        return _comparisonResult;
    }

    public void SetComparisonResult(bool result)
    {
        _comparisonResult = result;
    }

    public void EndCurrentSection()
    {
        _sectionEnded = true;
    }

    public void JumpToSection(string sectionName)
    {
        _pointer = 0;
        ExecuteSection(sectionName);
    }

    public void CallSection(string sectionName)
    {
        _callStack.Push((_section, _pointer));
        _pointer = 0;
        ExecuteSection(sectionName);
    }

    public void ReturnFromFunction()
    {
        if (_callStack.Count > 0)
        {
            var (returnSection, returnPointer) = _callStack.Pop();
            _pointer = returnPointer;
            _pointer++;
            ExecuteSection(returnSection);
        }
        else
        {
            Console.WriteLine("Error: Call stack is empty, cannot return from function.");
        }
    }
}