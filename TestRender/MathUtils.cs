using System;

namespace TestRender;

public class MathUtils
{
    public static bool NearlyEqual(float a, float b, float epsilon)
    {
        const float minNormal = 1.17549435E-38f;
        var absA = Math.Abs(a);
        var absB = Math.Abs(b);
        var diff = Math.Abs(a - b);

        if (a.Equals(b))
        { // shortcut, handles infinities
            return true;
        }

        if (a == 0 || b == 0 || absA + absB < minNormal)
        {
            // a or b is zero or both are extremely close to it
            // relative error is less meaningful here
            return diff < (epsilon * minNormal);
        }  // use relative error
        return diff / (absA + absB) < epsilon;
    }

}