using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HxCameraEditor.UserInterface.Models;

public enum MessageType
{
    Info,
    Error,
    Warning,
    Success,
}

public class MessageQueue
{
    public List<Message> Messages = [];
    public float AnimationSpeed = 1.0f;
    public Vector2 Position = Vector2.Zero;
    public SpriteFont Font;
    public Texture2D Pixel;
    
    public void Enqueue(string message, MessageType type = MessageType.Info)
    {
        switch (type)
        {
            case MessageType.Error:
                Enqueue(message, Color.DarkRed, Color.White);
                break;
            case MessageType.Warning:
                Enqueue(message, Color.DarkOrange, Color.Black);
                break;
            case MessageType.Success:
                Enqueue(message, Color.DarkGreen, Color.White);
                break;
            case MessageType.Info:
            default:
                Enqueue(message, Color.DarkBlue, Color.White);
                break;
        }
    }
    
    public void Enqueue(string message, Color backgroundColor, Color messageColor)
    {
        Messages.Add(new Message
        {
            Text = message,
            BackgroundColor = backgroundColor,
            MessageColor = messageColor,
        });
    }
    
    public void Update(GameTime gameTime)
    {
        var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
        for (var i = Messages.Count - 1; i >= 0; i--)
        {
            var message = Messages[i];
            if (message.LifeTimeInSeconds <= 0)
            {
                if (message.AnimationProgress >= 1f)
                {
                    Messages.RemoveAt(i);
                }
                message.AnimationProgress = MathF.Min(1f, message.AnimationProgress + (AnimationSpeed * delta));
            }
            else
            {
                message.LifeTimeInSeconds -= delta;
            }
        }
    }
    
    public void Draw(SpriteBatch spriteBatch)
    {
        float offsetY = 0f;
        var padding = 8f;
        for (int i = 0; i < Messages.Count; i++)
        {
            var message = Messages[i];
            var messageSize = Font.MeasureString(message.Text);
            
            spriteBatch.Draw(Pixel, new Rectangle((int)Position.X, (int)(Position.Y - messageSize.Y - offsetY), (int)messageSize.X, (int)messageSize.Y), message.BackgroundColor);
            spriteBatch.DrawString(Font, message.Text, Position - new Vector2(0, messageSize.Y + offsetY), message.MessageColor);
            offsetY += messageSize.Y + padding;
        }
    }
}