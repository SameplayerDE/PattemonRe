using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ChunkEditor;

public struct Command
{
    public string Primary;
    public string[] Arguments;
    
    public override string ToString()
    {
        return $"{Primary} {string.Join(" ", Arguments)}";
    }
}

public struct ConsoleState
{
    public List<Command> History;
    public int Cursor;
    public List<string> Log;
    public List<string> Suggestions;
    public bool IsActive;
    public string Content;
}

public static class VirtualConsole
{
    private static bool _isActive = false;
    private static readonly StringBuilder _current = new();
    
    private static int _cursor = 0; // for the current line
    
    private static int _suggetionCursor = 0; // for the suggestion
    private static int _historyCursor = 0; // for the history
    
    private static List<Command> _history = [];
    private static List<string> _log = [];
    private static List<string> _suggestions = [];
    
    private static KeyboardState _previousState;
    private static KeyboardState _currentState;
    
    private static readonly Dictionary<string, ICommandExecutor> _registeredExecutors = [];
    private static readonly Dictionary<string, ICommandSuggester> _registeredSuggestors = [];
    
    public static bool IsActive => _isActive;
    public static event Action<Command> OnCommandCall;

    public static void Register(string primary, ICommandExecutor executor)
    {
        _registeredExecutors[primary] = executor;
    }
    
    public static void Register(string primary, ICommandExecutor executor, ICommandSuggester suggester)
    {
        _registeredExecutors[primary] = executor;
        _registeredSuggestors[primary] = suggester;
    }
    
    public static ConsoleState GetState()
    {
        return new ConsoleState
        {
            IsActive = _isActive,
            History = _history,
            Log = _log,
            Suggestions = _suggestions,
            Cursor = _cursor,
            Content = _current.ToString(),
        };
    }
        
    public static void Activate()
    {
        _isActive = true;
    }

    public static void Deactivate()
    {
        _isActive = false;
    }

    public static void Clear()
    {
        _current.Clear();
        _cursor = 0;
    }

    public static void Write(char c)
    {
        _log.Add($"{c}");
    }
    
    public static void Write(string s)
    {
        _log.Add($"{s}");
    }

    public static void WriteLine(string s)
    {
        _log.Add($"{s}\n");
    }
    
    public static void Update(GameTime gameTime)
    {
        _previousState = _currentState;
        _currentState = Keyboard.GetState();
            
        if (_currentState.IsKeyDown(Keys.Tab) && _previousState.IsKeyUp(Keys.Tab))
        {
            _isActive = !_isActive;
        }
            
        if (!_isActive)
        {
            return;
        }
            
        var pressedKeys = _currentState.GetPressedKeys();
        var previousKeys = _previousState.GetPressedKeys();

        foreach (var key in pressedKeys)
        {
            bool isNewPress = !Array.Exists(previousKeys, k => k == key);

            if (isNewPress)
            {
                HandleKeyPress(key);
            }
        }
    }

    private static void HandleKeyPress(Keys key)
    {
        switch (key)
        {
            case Keys.Back when _cursor > 0:
                _current.Remove(--_cursor, 1);
                break;
            case Keys.Enter:
                ParseCommand(_current.ToString());
                _current.Clear();
                _historyCursor = 0;
                _cursor = 0;
                break;
            case Keys.Left:
                _cursor = Math.Max(0, _cursor - 1);
                break;
            case Keys.Right:
                _cursor = Math.Min(_current.Length, _cursor + 1);
                break;
            default:
                InsertChar(key);
                break;
        }
        _suggestions = FindSuggestions(_current.ToString());
    }

    private static void InsertChar(Keys key)
    {
        if (!IsAllowedKey(key))
        {
            return;
        }
        _current.Insert(_cursor, KeyToString(key));
        _cursor++;
    }
    
    private static bool IsAllowedKey(Keys key)
    {
        return key is >= Keys.A and <= Keys.Z or >= Keys.D0 and <= Keys.D9 or Keys.Space or Keys.OemMinus;
    }

    private static void AddToHistory(Command command)
    {
        if (!string.IsNullOrWhiteSpace(command.ToString()))
        {
            _history.Add(command);
            if (_history.Count > 100)
            {
                _history.RemoveAt(0);
            }
        }
    }
    
    private static string KeyToString(Keys key)
    {
        return key switch
        {
            >= Keys.A and <= Keys.Z => key.ToString().ToLower(),
            >= Keys.D0 and <= Keys.D9 => ((int)key - (int)Keys.D0).ToString(),
            Keys.Space => " ",
            Keys.OemMinus => "-",
            _ => ""
        };
    }
    
    private static void ParseCommand(string input)
    {
        var args = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (args.Length == 0) return;

        var command = new Command
        {
            Primary = args[0].ToLower(),
            Arguments = args.Length > 1 ? args[1..] : []
        };

        foreach (var commandKey in _registeredExecutors.Keys.Where(commandKey => commandKey.Equals(command.Primary, StringComparison.OrdinalIgnoreCase)))
        {
            _registeredExecutors[commandKey].OnCommand(command);
        }
        AddToHistory(command);
    }
    
    private static List<string> FindSuggestions(string input)
    {
        var args = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (args.Length == 0)
        {
            return null;
        }

        var command = new Command
        {
            Primary = args[0].ToLower(),
            Arguments = args.Length > 1 ? args[1..] : []
        };

        foreach (var commandKey in _registeredSuggestors.Keys.Where(commandKey => commandKey.Equals(command.Primary, StringComparison.OrdinalIgnoreCase)))
        {
            return _registeredSuggestors[commandKey].OnSuggest(command);
        }
        return null;
    }
}