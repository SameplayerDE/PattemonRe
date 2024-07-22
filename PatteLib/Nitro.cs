namespace PatteLib;

public static class Nitro
{
    public const int Fx32Shift = 12;
    public const int Fx32IntSize = 19;
    public const int Fx32DecSize = 12;
    public const uint Fx32IntMask = 0x7ffff000;
    public const uint Fx32DecMask = 0x00000fff;
    public const uint Fx32SignMask = 0x80000000;
    public const uint Fx32Max = 0x7fffffff;
    public const uint Fx32Min = 0x80000000;

    public const int Fx16Shift = 12;
    public const int Fx16IntSize = 3;
    public const int Fx16DecSize = 12;
    public const ushort Fx16IntMask = 0x7000;
    public const ushort Fx16DecMask = 0x0fff;
    public const ushort Fx16SignMask = 0x8000;
    public const ushort Fx16Max = 0x7fff;
    public const ushort Fx16Min = 0x8000;
}