namespace PatteLib.Data;

public static class MemoryContext
{
    private static  Dictionary<int, int> _variables = [];
    private static Dictionary<int, int> _flags = [];
    
    public static int GetVariable(int address)
    {
        return _variables.TryGetValue(address, out var value) ? value : 0;
    }

    public static void SetVariable(int address, int value)
    {
        _variables[address] = value;
    }
    
    public static int GetFlag(int address)
    {
        return _flags.TryGetValue(address, out var value) ? value : 0;
    }

    public static void SetFlag(int address, int value)
    {
        _flags[address] = value;
    }
    
}