using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PatteLib.Graphics;

public enum ImageFontRenderCommandType
{
    Space,
    ChangeScale,
    ChangeTint,
    Reset,
    Character
}

public class ImageFontRenderCommand
{
    public ImageFontRenderCommandType Type;
    public char Character;
    public int Sx, Sy;
    public Color Tint;
}

public class ImageFontRenderer
{
    private ImageFont _font;
    private GraphicsDevice _graphicsDevice;
    private SpriteBatch _spriteBatch;

    private Queue<ImageFontRenderCommand> _renderQueue = [];
    
    public ImageFontRenderer(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, ImageFont font)
    {
        _graphicsDevice = graphicsDevice;
        _spriteBatch = spriteBatch;
        SetFont(font);
    }

    public void SetFont(ImageFont font)
    {
        _font = font;
    }
    
    public void DrawText(string text, Vector2 position)
    {
        DrawText(text, position, Color.White, 1, 1);
    }

    public void DrawText(string text, Vector2 position, Color tint)
    {
        DrawText(text, position, tint, 1, 1);
    }

    public void DrawText(string text, Vector2 position, Color tint, int sx, int sy)
    {
        Vector2 currentPosition = position;
        Color currentTint = tint;
        int scaleX = sx;
        int scaleY = sy;
        int currentSX = 1;
        int currentSY = 1;

        _renderQueue.Clear();
        ProcessText(text);

        while (_renderQueue.Count > 0)
        {
            var command = _renderQueue.Dequeue();

            if (command.Type == ImageFontRenderCommandType.Character)
            {
                var charInfo = _font.GetChar(command.Character);
                if (charInfo.HasValue)
                {
                    DrawCharacter(_spriteBatch, command.Character, currentPosition, currentTint, currentSX * scaleX, currentSY * scaleY);
                    currentPosition.X += charInfo.Value.X * currentSX * scaleX; // Advance position based on character width and scale
                }
            }
            if (command.Type == ImageFontRenderCommandType.Space)
            {
                currentPosition.X += _font.Meta.Space * currentSX * scaleX;
            }
            if (command.Type == ImageFontRenderCommandType.ChangeTint)
            {
                currentTint = command.Tint;
            }
            if (command.Type == ImageFontRenderCommandType.ChangeScale)
            {
                currentSX = command.Sx;
                currentSY = command.Sy;
            }
            if (command.Type == ImageFontRenderCommandType.Reset)
            {
                currentSX = command.Sx;
                currentSY = command.Sy;
                currentTint = command.Tint;
            }
        }
    }

    private void ProcessText(string text)
    {
        for (var i = 0; i < text.Length; i++)
        {
            var character = text[i];
            switch (character)
            {
                case '§' when i + 1 < text.Length:
                {
                    i++;
                    var command = text[i];

                    switch (command)
                    {
                        case 'l':
                            _renderQueue.Enqueue(new ImageFontRenderCommand()
                            {
                                Type = ImageFontRenderCommandType.ChangeScale,
                                Sx = 1,
                                Sy = 2
                            });
                            break;
                        case 'r':
                            _renderQueue.Enqueue(new ImageFontRenderCommand()
                            {
                                Type = ImageFontRenderCommandType.Reset,
                                Sx = 1,
                                Sy = 1,
                                Tint = Color.White
                            });
                            break;
                        case 'a':
                            _renderQueue.Enqueue(new ImageFontRenderCommand()
                            {
                                Type = ImageFontRenderCommandType.ChangeTint,
                                Tint = new Color(170, 255, 0)
                            });
                            break;
                        case 'b':
                            _renderQueue.Enqueue(new ImageFontRenderCommand()
                            {
                                Type = ImageFontRenderCommandType.ChangeTint,
                                Tint = new Color(0, 255, 255)
                            });
                            break;
                        case 'c':
                            _renderQueue.Enqueue(new ImageFontRenderCommand()
                            {
                                Type = ImageFontRenderCommandType.ChangeTint,
                                Tint = new Color(255, 85, 85)
                            });
                            break;
                        case 'd':
                            _renderQueue.Enqueue(new ImageFontRenderCommand()
                            {
                                Type = ImageFontRenderCommandType.ChangeTint,
                                Tint = new Color(255, 85, 255)
                            });
                            break;
                        case 'e':
                            _renderQueue.Enqueue(new ImageFontRenderCommand()
                            {
                                Type = ImageFontRenderCommandType.ChangeTint,
                                Tint = new Color(255, 255, 85)
                            });
                            break;
                        case 'f':
                            _renderQueue.Enqueue(new ImageFontRenderCommand()
                            {
                                Type = ImageFontRenderCommandType.ChangeTint,
                                Tint = new Color(255, 255, 255)
                            });
                            break;
                        case '0':
                            _renderQueue.Enqueue(new ImageFontRenderCommand()
                            {
                                Type = ImageFontRenderCommandType.ChangeTint,
                                Tint = new Color(0, 0, 0)
                            });
                            break;
                        case '1':
                            _renderQueue.Enqueue(new ImageFontRenderCommand()
                            {
                                Type = ImageFontRenderCommandType.ChangeTint,
                                Tint = new Color(0, 0, 170)
                            });
                            break;
                        case '2':
                            _renderQueue.Enqueue(new ImageFontRenderCommand()
                            {
                                Type = ImageFontRenderCommandType.ChangeTint,
                                Tint = new Color(0, 170, 0)
                            });
                            break;
                        case '3':
                            _renderQueue.Enqueue(new ImageFontRenderCommand()
                            {
                                Type = ImageFontRenderCommandType.ChangeTint,
                                Tint = new Color(0, 170, 170)
                            });
                            break;
                        case '4':
                            _renderQueue.Enqueue(new ImageFontRenderCommand()
                            {
                                Type = ImageFontRenderCommandType.ChangeTint,
                                Tint = new Color(170, 0, 0)
                            });
                            break;
                        case '5':
                            _renderQueue.Enqueue(new ImageFontRenderCommand()
                            {
                                Type = ImageFontRenderCommandType.ChangeTint,
                                Tint = new Color(170, 0, 170)
                            });
                            break;
                        case '6':
                            _renderQueue.Enqueue(new ImageFontRenderCommand()
                            {
                                Type = ImageFontRenderCommandType.ChangeTint,
                                Tint = new Color(255, 170, 0)
                            });
                            break;
                        case '7':
                            _renderQueue.Enqueue(new ImageFontRenderCommand()
                            {
                                Type = ImageFontRenderCommandType.ChangeTint,
                                Tint = new Color(170, 170, 170)
                            });
                            break;
                        case '8':
                            _renderQueue.Enqueue(new ImageFontRenderCommand()
                            {
                                Type = ImageFontRenderCommandType.ChangeTint,
                                Tint = new Color(85, 85, 85)
                            });
                            break;
                        case '9':
                            _renderQueue.Enqueue(new ImageFontRenderCommand()
                            {
                                Type = ImageFontRenderCommandType.ChangeTint,
                                Tint = new Color(85, 85, 255)
                            });
                            break;
                    }

                    break;
                }
                case ' ':
                    _renderQueue.Enqueue(new ImageFontRenderCommand()
                    {
                        Type = ImageFontRenderCommandType.Space
                    });
                    break;
                default:
                    _renderQueue.Enqueue(new ImageFontRenderCommand()
                    {
                        Type = ImageFontRenderCommandType.Character,
                        Character = character
                    });
                    break;
            }
        }
    }
    
    private void DrawCharacter(SpriteBatch spriteBatch, char character, Vector2 position, Color tint, int sx, int sy)
    {
        if (_font.HasChar(character))
        {
            var charInfo = _font.GetChar(character) ?? throw new Exception();
            Rectangle sourceRect = new Rectangle(charInfo.X, charInfo.Y, charInfo.Width, charInfo.Height);
            spriteBatch.Draw(_font.Texture, position, sourceRect, tint, 0f, Vector2.Zero, new Vector2(sx, sy), SpriteEffects.None, 0f);
        }
    }
}