using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Survival
{
    public class TargetCamera : CameraBase
    {
        public Vector3 Target { get; set; }
        public float Distance { get; set; }

        public TargetCamera(GraphicsDevice device, Vector3 target, float distance) : base(device)
        {
            Target = target;
            Distance = distance;
            Position = target - new Vector3(0, 0, distance); // Startposition hinter dem Ziel
            Rotation = Quaternion.Identity;
        }

        public override Matrix GetViewMatrix()
        {
            Vector3 direction = Vector3.Transform(new Vector3(0, 0, -Distance), Rotation);
            Position = Target + direction;
            _viewMatrix = Matrix.CreateLookAt(Position, Target, Vector3.Up);
            return _viewMatrix;
        }
    }
}
