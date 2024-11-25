using System;

namespace ColourPaletteTry
{
    using System;
    
    public static class Program
    {
        

        
        public static float GetAngleFromU16Int(ushort angleIndex)
        {
            double normalizedAngleIndex = angleIndex / 65535f;
            double angleInRadians = normalizedAngleIndex * Math.PI * 2;
            double angleInDegrees = angleInRadians * 180 / Math.PI;

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
        
        [STAThread]
        static void Main()
        {
            var angle = 95;
            var game = new Game1();
            game.Run();
            Console.WriteLine("" + GetU16IntFromAngle(angle));
        }
    }
}