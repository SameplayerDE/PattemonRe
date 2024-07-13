namespace PatteLib.Gameplay;

public enum MovementPattern
{
    None = 0x00,
    LookAny = 0x02,
    WalkAny = 0x03,
    WalkUpDown = 0x04,  
    WalkLeftRight = 0x05,
    LookUpLeft = 0x06,
    LookUpRight = 0x07,
    LookDownLeft = 0x08,
    LookDownRight = 0x09,
    LookUpLeftDown = 0x0A,
    LookDownRightUp = 0x0B,
    LookRightLeftUp = 0x0C,
    LookRightLeftDown = 0x0D,
    FaceUp = 0x0E,
    FaceDown = 0x0F,
    FaceLeft = 0x10,
    FaceRight = 0x11,
    CCWSpin = 0x12,
    CWSpin = 0x13,
    RunUpDown = 0x14
    //ToDo: add other movement patterns
}