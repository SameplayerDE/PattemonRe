namespace Pattemon.Engine;

public interface ITransition
{
    public bool Init();
    public bool Update();
    public bool Exit();
}