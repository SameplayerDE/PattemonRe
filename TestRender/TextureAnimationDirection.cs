using System;

namespace TestRender;

public enum TextureAnimationDirection : byte
{
    None = 0x00,
    Right = 0x01,
    Left = 0x02,
    Up = 0x03,
    Down = 0x04,
    RightUp = 0x05,
    RightDown = 0x06,
    LeftUp = 0x07,
    LeftDown = 0x08,
    DownLeft = 0x08,
    DownRight = 0x06,
    UpRight = 0x05,
    UpLeft = 0x07
}