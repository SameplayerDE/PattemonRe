using System;
using System.Collections.Generic;
using HxCameraEditor.UserInterface;
using InputLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PatteLib.Graphics;

namespace HxCameraEditor.Graphics;

public class UserInterfaceRenderer
{

    // Fields
    private Dictionary<string, Texture2D> _images;
    private Dictionary<string, SpriteFont> _fonts;

    private ImageFont _font;
    private ImageFontRenderer _fontRenderer;
    
    // Properties
    public SpriteFont Font { get; set; }
    public Texture2D ButtonTile { get; set; }
    
    public GraphicsDevice GraphicsDevice;
    public ContentManager ContentManager;
    public Texture2D Pixel;

    public UserInterfaceRenderer(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, IServiceProvider serviceProvider)
    {
        GraphicsDevice = graphicsDevice;
        ContentManager = new ContentManager(serviceProvider, "Content");
        
        _fontRenderer = new ImageFontRenderer(graphicsDevice, spriteBatch, _font = ImageFont.Load(graphicsDevice, "Assets/Font.json"));
        _images = new Dictionary<string, Texture2D>();
        _fonts = new Dictionary<string, SpriteFont>();
        _fonts.Add("default", ContentManager.Load<SpriteFont>("default"));
        Pixel = new Texture2D(GraphicsDevice, 1, 1);
        Pixel.SetData(new[] { Color.White });
    }

    public void CalculateLayout(UserInterfaceNode node)
    {
        if (node.Type == UserInterfaceNodeType.Label)
        {
            CalculateLabelLayout((Label)node);
        }

        if (node.Type == UserInterfaceNodeType.Button)
        {
            CalculateButtonLayout((Button)node);
        }

        if (node.Type == UserInterfaceNodeType.HStack)
        {
            CalculateHStackLayout((HStack)node);
        }
        
        if (node.Type == UserInterfaceNodeType.VStack)
        {
            CalculateVStackLayout((VStack)node);
        }
    }

        private void CalculateVStackLayout(VStack stack)
    {
        var currentY = stack.Y + stack.PaddingTop;
        var currentX = stack.X + stack.PaddingLeft;
        var maxWidth = 0f;
        var totalHeight = stack.PaddingTop + stack.PaddingBottom;

        for (var index = 0; index < stack.Children.Count; index++)
        {
            var child = stack.Children[index];

            if (!child.IsVisible)
            {
                continue;
            }

            child.X = currentX;
            child.Y = currentY;

            CalculateLayout(child);

            currentY += child.Height + stack.Spacing;
            totalHeight += child.Height;
            if (index != stack.Children.Count - 1)
            {
                totalHeight += stack.Spacing;
            }

            if (child.Width > maxWidth)
            {
                maxWidth = child.Width;
            }
        }

        stack.Width = maxWidth + stack.PaddingLeft + stack.PaddingRight;
        stack.Height = totalHeight;

        for (var index = 0; index < stack.Children.Count; index++)
        {
            var child = stack.Children[index];

            if (stack.Alignment == Alignment.Center)
            {
                if (child.Width < maxWidth)
                {
                    child.X = stack.X + stack.Width / 2 - child.Width / 2;
                    CalculateLayout(child);
                }
            }
        }
    }

    private void CalculateHStackLayout(HStack stack)
    {
        var currentY = stack.Y + stack.PaddingTop;
        var currentX = stack.X + stack.PaddingLeft;
        var maxHeight = 0f;
        var totalWidth = stack.PaddingLeft + stack.PaddingRight;

        for (var index = 0; index < stack.Children.Count; index++)
        {
            var child = stack.Children[index];

            if (!child.IsVisible)
            {
                continue;
            }

            child.X = currentX;
            child.Y = currentY;

            CalculateLayout(child);

            currentX += child.Width + stack.Spacing;
            totalWidth += child.Width;
            if (index != stack.Children.Count - 1)
            {
                totalWidth += stack.Spacing;
            }

            if (child.Height > maxHeight)
            {
                maxHeight = child.Height;
            }
        }

        stack.Height = maxHeight + stack.PaddingTop + stack.PaddingBottom;
        stack.Width = totalWidth;

        for (var index = 0; index < stack.Children.Count; index++)
        {
            var child = stack.Children[index];

            if (stack.Alignment == Alignment.Center)
            {
                if (child.Height < maxHeight)
                {
                    child.Y = stack.Y;
                    child.Y += stack.Height / 2;
                    child.Y -= child.Height / 2;
                    CalculateLayout(child);
                }
            }
        }
    }
    
    private void CalculateButtonLayout(Button node)
    {
        var button = (Button)node;
        var currentY = button.Y + button.PaddingTop;
        var currentX = button.X + button.PaddingLeft;
        var totalWidth = button.PaddingLeft + button.PaddingRight;
        var totalHeight = button.PaddingTop + button.PaddingBottom;

        for (var index = 0; index < button.Children.Count; index++)
        {
            var child = button.Children[index];

            // Setze die Position des Kindes auf die aktuelle Y-Position des Buttons
            child.X = currentX;
            child.Y = currentY;

            // Rendere das Kind und aktualisiere die Abmessungen des Buttons
            CalculateLayout(child);

            totalHeight += child.Height;
            totalWidth += child.Width;
        }

        // Setze die Breite und Höhe des Buttons auf die berechneten Werte
        button.Width = totalWidth;
        button.Height = totalHeight;
    }

    public void HandleInput(UserInterfaceNode node)
    {
        if (node == null)
        {
            return;
        }
        
        if (node.Type == UserInterfaceNodeType.Button)
        {
            var button = (Button)node;
            var buttonRect = new Rectangle((int)button.X, (int)button.Y, (int)button.Width, (int)button.Height);

            if (buttonRect.Contains(MouseHandler.Position) && MouseHandler.IsButtonDownOnce(MouseButton.Left))
            {
                button.Invoke();
            }
        }

        if (node is UserInterfaceNodeContainer container)
        {
            foreach (var child in container.Children)
            {
                HandleInput(child);
            }
        }
        
    }
    
    private void CalculateLabelLayout(Label node)
    {
        Label label = node;
        string text = "";
        text = Convert.ToString(label.Text) ?? "";
        Vector2 dimensions = _fonts[label.Font].MeasureString(text);
        //Vector2 dimensions = _font.MeasureString(text).ToVector2();
        label.Height = dimensions.Y;
        label.Width = dimensions.X;
    }
    
    public void DrawNode(SpriteBatch spriteBatch, GameTime gameTime, UserInterfaceNode node)
    {
        if (node.Type == UserInterfaceNodeType.Label)
        {
            Label label = (Label)node;
            //_fontRenderer.DrawText(Convert.ToString(label.Text) ?? "", new Vector2(label.X, label.Y));
            spriteBatch.DrawString(_fonts[label.Font], Convert.ToString(label.Text) ?? "", new Vector2(label.X, label.Y), Color.White);
        }
        
        if (node.Type == UserInterfaceNodeType.HStack)
        {
            var stack = (HStack)node;
            //spriteBatch.Draw(Context.Pixel, new Rectangle((int)stack.X, (int)stack.Y, (int)stack.Width, (int)stack.Height), stack.Tint * stack.Alpha); // Anpassen der Zeichenroutine für die Gesamtbreite
            foreach (var child in stack.Children)
            {
                DrawNode(spriteBatch, gameTime, child);
            }
        }


        if (node.Type == UserInterfaceNodeType.VStack)
        {
            var stack = (VStack)node;
            //spriteBatch.Draw(Context.Pixel, new Rectangle((int)stack.X, (int)stack.Y, (int)stack.Width, (int)stack.Height), stack.Tint * stack.Alpha); // Anpassen der Zeichenroutine für die Gesamtbreite

            foreach (var child in stack.Children)
            {
                DrawNode(spriteBatch, gameTime, child);
            }
        }
        
        if (node.Type == UserInterfaceNodeType.Button)
        {
            var button = (Button)node;
            spriteBatch.Draw(Pixel, new Rectangle((int)button.X, (int)button.Y, (int)button.Width, (int)button.Height), Color.Black * 0.8f);

            foreach (var child in button.Children)
            {
                DrawNode(spriteBatch, gameTime, child);
            }
        }
        //if (node.Type == UserInterfaceNodeType.Slider)
        //{
        //    var stack = (Slider)node;
        //    //spriteBatch.Draw(Context.Pixel, new Rectangle((int)stack.X, (int)stack.Y, (int)stack.Width, (int)stack.Height), stack.Tint * stack.Alpha); // Anpassen der Zeichenroutine für die Gesamtbreite
        //}
    }
}