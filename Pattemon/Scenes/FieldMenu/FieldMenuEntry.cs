using System;

namespace Pattemon.Scenes.FieldMenu;

#nullable enable
public struct FieldMenuEntry
{
    public int IconIndex;
    public string Text;
    public Action? OnClick;
}