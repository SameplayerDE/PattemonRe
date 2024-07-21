using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PatteLib;

public class Utils
{
    
    
    
    // Funktion zum Konvertieren von 3D-Weltkoordinaten zu 2D-Bildschirmkoordinaten
    public static Vector2 WorldToScreen(Vector3 worldPosition, Matrix viewMatrix, Matrix projectionMatrix, Viewport viewport)
    {
        // Transformiere die Weltposition in Clip-Space
        Vector4 clipSpacePosition = Vector4.Transform(worldPosition, Matrix.Multiply(viewMatrix, projectionMatrix));
    
        // Normalisiere die Clip-Space-Koordinaten
        clipSpacePosition /= clipSpacePosition.W;
    
        // Transformiere die normalisierten Clip-Space-Koordinaten in Bildschirmkoordinaten
        Vector3 screenPosition = new Vector3(
            (clipSpacePosition.X + 1) / 2 * viewport.Width,
            (1 - clipSpacePosition.Y) / 2 * viewport.Height,
            clipSpacePosition.Z
        );
    
        return new Vector2(screenPosition.X, screenPosition.Y);
    }
    
    public static double Q412ToDouble(int q412Value)
    {
        bool isNegative = (q412Value & (1 << 15)) != 0;
        
        int absoluteValue = isNegative ? ~q412Value + 1 : q412Value;
        
        int integerPart = absoluteValue >> 12;
        int fractionalPart = absoluteValue & ((1 << 12) - 1);
        
        const double fractionalScale = 1.0 / (1 << 12);
        double scaledFractionalPart = fractionalPart * fractionalScale;
        
        double usableNumber = integerPart + scaledFractionalPart;
        
        return isNegative ? -usableNumber : usableNumber;
    }
    
}