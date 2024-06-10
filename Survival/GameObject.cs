using Microsoft.Xna.Framework;

namespace Survival
{
    public abstract class GameObject
    {
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public Vector3 Scale { get; set; } = Vector3.One;

        public GameObject()
        {
            Position = Vector3.Zero;
            Rotation = Quaternion.Identity;
        }

        public virtual void Update(GameTime gameTime) { }

        // Position manipulieren
        public void Move(Vector3 translation)
        {
            Position += translation;
        }

        public void MoveTo(Vector3 position)
        {
            Position = position;
        }

        public void RotateTo(Vector3 rotation)
        {
            Rotation = Quaternion.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z);
        }

        public void RotateDeg(float xAngle, float yAngle, float zAngle)
        {
            var yaw = MathHelper.ToRadians(yAngle);
            var pitch = MathHelper.ToRadians(xAngle);
            var roll = MathHelper.ToRadians(zAngle);
            var q = Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll);
            Rotation *= q;
        }

        public void ScaleTo(Vector3 scale)
        {
            Scale = scale;
        }

        public Vector3 GetForward()
        {
            return Vector3.Transform(Vector3.Forward, Rotation);
        }

        public Vector3 GetUp()
        {
            return Vector3.Transform(Vector3.Up, Rotation);
        }

        public Vector3 GetRight()
        {
            return Vector3.Transform(Vector3.Right, Rotation);
        }
    }
}
