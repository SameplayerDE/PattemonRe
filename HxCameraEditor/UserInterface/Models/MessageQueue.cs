using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace HxCameraEditor.UserInterface.Models;

public class MessageQueue
{
    public List<Message> Messages = [];
    public float AnimationSpeed = 1.0f;
    public Vector2 Position = Vector2.Zero;
    public SpriteFont Font;
    public Texture2D Pixel;
    
    public void Enqueue(string message)
    {
        Messages.Add(new Message()
        {
            Text = message
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
            
            spriteBatch.Draw(Pixel, new Rectangle((int)Position.X, (int)(Position.Y - messageSize.Y - offsetY), (int)messageSize.X, (int)messageSize.Y), Color.Red);
            spriteBatch.DrawString(Font, message.Text, Position - new Vector2(0, messageSize.Y + offsetY), Color.White);
            offsetY += messageSize.Y + padding;
        }
    }
}