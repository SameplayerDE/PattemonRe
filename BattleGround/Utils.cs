namespace BattleGround;

public class Utils
{
    public static float Wrap(float value, float min, float max)
    {
        if (value < min)
        {
            return max - (min - value) % (max - min);
        }
        else if (value > max)
        {
            return min + (value - max) % (max - min);
        }
        else
        {
            return value;
        }
    }
}