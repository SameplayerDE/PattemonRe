namespace PatteLib.Data;

public struct Options
{
    private ushort _textSpeed = 1; // slow, medium, fast
    private ushort _soundMode = 0; // stereo, mono
    private ushort _animations = 0; // on, off
    private ushort _battleStyle = 0; // switch, follow
    private ushort _buttonMapping = 0; // normal, start = x, L = A
    private ushort _textBoxStyle = 0; // 0-19

    public Options(ushort textSpeed, ushort soundMode, ushort animations, ushort battleStyle, ushort buttonMapping, ushort textBoxStyle)
    {
        _textSpeed = textSpeed;
        _soundMode = soundMode;
        _animations = animations;
        _battleStyle = battleStyle;
        _buttonMapping = buttonMapping;
        _textBoxStyle = textBoxStyle;
    }

    public ushort TextSpeed
    {
        get => _textSpeed;
        set => _textSpeed = (ushort)(value % 3); // 0, 1, 2 (slow, medium, fast)
    }

    public ushort SoundMode
    {
        get => _soundMode;
        set => _soundMode = (ushort)(value % 2); // 0, 1 (stereo, mono)
    }

    public ushort Animations
    {
        get => _animations;
        set => _animations = (ushort)(value % 2); // 0, 1 (off, on)
    }

    public ushort BattleStyle
    {
        get => _battleStyle;
        set => _battleStyle = (ushort)(value % 2); // 0, 1 (switch, follow)
    }

    public ushort ButtonMapping
    {
        get => _buttonMapping;
        set => _buttonMapping = (ushort)(value % 3); // 0, 1, 2 (normal, start = x, L = A)
    }

    public ushort TextBoxStyle
    {
        get => _textBoxStyle;
        set => _textBoxStyle = (ushort)(value % 20); // 0-19 (various styles)
    }
    
    public static Options Default { get; } = new Options();
}