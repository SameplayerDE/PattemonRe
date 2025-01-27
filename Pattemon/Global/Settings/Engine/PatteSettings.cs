using System;
using System.IO;

namespace Pattemon.Global.Settings.Engine;

public enum AspectRatio
{
    Original,
    Full,
}

public enum ScreenSwapAnimation
{
    None,
    BottomInOut,
}

public class SettingsData
{
    public AspectRatio AspectRatio { get; set; }
    public ScreenSwapAnimation ScreenSwapAnimation { get; set; }
}

public static class PatteSettings
{
    private const string Path = "Data/Engine/Settings.json";
    private static readonly SettingsBase<SettingsData> _settings = new(Path);
    
    private static SettingsData _data = new();
    
    public static event Action<AspectRatio> OnAspectRatioChanged;
    public static event Action<ScreenSwapAnimation> OnScreenSwapAnimationChanged;
    
    public static AspectRatio AspectRatio
    {
        get => _data.AspectRatio;
        set
        {
            if (_data.AspectRatio == value)
            {
                return;
            }
            _data.AspectRatio = value;
            OnAspectRatioChanged?.Invoke(_data.AspectRatio);
        }
    }
    public static ScreenSwapAnimation ScreenSwapAnimation
    {
        get => _data.ScreenSwapAnimation;
        set
        {
            if (_data.ScreenSwapAnimation == value)
            {
                return;
            }
            _data.ScreenSwapAnimation = value;
            OnScreenSwapAnimationChanged?.Invoke( _data.ScreenSwapAnimation);
        }
    }
    
    public static void Load()
    {
        _data = _settings.Load();
    }

    public static void Save()
    {
        _settings.Save(_data);
    }

    public static void Reset()
    {
        _data = new SettingsData();
        Save();
    }
}