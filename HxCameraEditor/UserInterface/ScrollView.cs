using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace HxCameraEditor.UserInterface;

public class ScrollView : UserInterfaceNodeContainer
{
    
    public Vector2 ScrollOffset { get; private set; } = Vector2.Zero;
    public float MaxScrollY = float.MaxValue;
    public float ScrollSpeed { get; set; } = 0.1f;
    
    public float PaddingLeft { get; private set; } = 25;
    public float PaddingTop { get; private set; } = 5;
    public float PaddingRight { get; private set; } = 25;
    public float PaddingBottom { get; private set; } = 5;
    
    public ScrollView(params UserInterfaceNode[] nodes) : base(UserInterfaceNodeType.ScrollView)
    {
        IsClickable = true;
        Children.AddRange(nodes);
    }
        
    public ScrollView(IEnumerable<UserInterfaceNode> nodes) : base(UserInterfaceNodeType.ScrollView)
    {
        IsClickable = true;
        Children.AddRange(nodes);
    }
    
    public void UpdateScroll(float deltaY)
    {
        // Berechne die neuen Grenzen für das Scrollen
        float maxScroll = Math.Max(0, MaxScrollY);
        float newScrollY = ScrollOffset.Y + deltaY;

        // Begrenze das Scrollen zwischen 0 und maxScroll
        ScrollOffset = new Vector2(
            ScrollOffset.X, 
            Math.Clamp(newScrollY, 0, maxScroll)
        );
    }
}