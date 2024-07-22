namespace PatteLib;

public static class NitroUtils
{
    
    public static readonly float[] AngleTable = [ushort.MaxValue];

    static NitroUtils()
    {
        //for (ushort i = 0; i < ushort.MaxValue; i++)
        //{
        //    AngleTable[i] = GetAngleFromU16Int(i);
        //}
    }
    
    public static decimal Fx16ToDecimal(short fx16)
    {
        // Check for negative values
        bool isNegative = (fx16 & Nitro.Fx16SignMask) != 0;

        // Extract integer and fractional parts
        int integerPart = fx16 >> Nitro.Fx16Shift;
        int fractionalPart = fx16 & Nitro.Fx16DecMask;

        // Convert integer part to float
        decimal integerFloat = integerPart;

        // Handle overflow for large positive values
        if (integerPart == Nitro.Fx16Max && (fractionalPart & Nitro.Fx16SignMask) == 0)
        {
            // If the integer part is the maximum positive value and the fractional part is non-negative,
            // the number is too large to represent in a single-precision float.
            throw new OverflowException("Value too large for single-precision float");
        }

        const decimal fractionalScale = (decimal)(1.0 / (1 << Nitro.Fx16Shift));
        decimal scaledFractionalPart = fractionalPart * fractionalScale;
        decimal result = integerFloat + scaledFractionalPart;

        // Apply sign
        if (isNegative)
            result *= -1;

        return result;
    }

    public static long DecimalToFx16(decimal value)
    {
        // Check for overflow and underflow
        if (value > (decimal)Nitro.Fx16Max || value < -(decimal)Nitro.Fx16Max)
            throw new OverflowException("Value out of range for FX32");

        // Handle negative values
        bool isNegative = value < 0;
        value = Math.Abs(value);

        // Extract integer and fractional parts
        long integerPart = (long)value;
        decimal fractionalPart = value - integerPart;

        const decimal fractionalScale = (decimal)(1.0 / (1 << Nitro.Fx16Shift));
        // Convert fractional part to integer
        long fractionalInt = (long)(fractionalPart / fractionalScale);

        // Combine integer and fractional parts
        long result = (int)(integerPart << Nitro.Fx16Shift) | (int)fractionalInt;

        // Apply sign
        if (isNegative)
            result |= Nitro.Fx16SignMask;

        return result;
    }

    public static decimal Fx32ToDecimal(int fx32)
    {
        // Check for negative values
        bool isNegative = (fx32 & Nitro.Fx32SignMask) != 0;

        // Extract integer and fractional parts
        long integerPart = fx32 >> Nitro.Fx32Shift;
        long fractionalPart = fx32 & Nitro.Fx32DecMask;

        // Convert integer part to float
        decimal integerFloat = integerPart;

        // Handle overflow for large positive values
        if (integerPart == Nitro.Fx32Max && (fractionalPart & Nitro.Fx32SignMask) == 0)
        {
            // If the integer part is the maximum positive value and the fractional part is non-negative,
            // the number is too large to represent in a single-precision float.
            throw new OverflowException("Value too large for single-precision float");
        }

        const decimal fractionalScale = (decimal)(1.0 / (1 << Nitro.Fx32Shift));
        decimal scaledFractionalPart = fractionalPart * fractionalScale;
        decimal result = integerFloat + scaledFractionalPart;

        // Apply sign
        if (isNegative)
            result *= -1;

        return result;
    }

    public static long DecimalToFx32(decimal value)
    {
        // Check for overflow and underflow
        if (value > (decimal)Nitro.Fx32Max || value < -(decimal)Nitro.Fx32Max)
            throw new OverflowException("Value out of range for FX32");

        // Handle negative values
        bool isNegative = value < 0;
        value = Math.Abs(value);

        // Extract integer and fractional parts
        long integerPart = (long)value;
        decimal fractionalPart = value - integerPart;

        const decimal fractionalScale = (decimal)(1.0 / (1 << Nitro.Fx32Shift));
        // Convert fractional part to integer
        long fractionalInt = (long)(fractionalPart / fractionalScale);

        // Combine integer and fractional parts
        long result = (int)(integerPart << Nitro.Fx32Shift) | (int)fractionalInt;

        // Apply sign
        if (isNegative)
            result |= Nitro.Fx32SignMask;

        return result;
    }
    public static float GetAngleFromU16Int(ushort angleIndex)
    {
        double normalizedAngleIndex = angleIndex / 65535d;
        double angleInDegrees = normalizedAngleIndex * 360;

        return (float)angleInDegrees;
    }
    public static ushort GetU16IntFromAngle(float angleDegrees)
    {
        angleDegrees %= 360;

        double angleInRadians = angleDegrees * Math.PI / 180;
        double normalizedAngleIndex = angleInRadians / (2 * Math.PI);
        double angleIndex = normalizedAngleIndex * 65535;

        return (ushort)angleIndex;
    }
}