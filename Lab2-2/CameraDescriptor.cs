using Silk.NET.Maths;
using System;

namespace Szeminarium
{
    internal class CameraDescriptor
    {
        /// <summary>
        /// A kamera aktuális pozíciója a térben.
        /// </summary>
        public Vector3D<float> Position { get; private set; } = new Vector3D<float>(5.0f, 5.0f, 6.0f);

        /// <summary>
        /// Vízszintes elfordulási szög (balra-jobbra forgás, radiánban).
        /// Kezdetben pontosan az origó (0, 0, 0) felé néz.
        /// </summary>
        private double yaw = Math.Atan2(-6.0, -5.0);

        /// <summary>
        /// Függőleges billenési szög (fel-le nézés, radiánban).
        /// Kezdetben pontosan az origó (0, 0, 0) felé néz.
        /// </summary>
        private double pitch = Math.Atan2(-5.0, Math.Sqrt(5.0 * 5.0 + 6.0 * 6.0));

        private const float MoveStep = 0.25f;
        private const double AngleStep = Math.PI / 180.0 * 3.0; // 3 fok lépésenként

        /// <summary>
        /// A kamera előre mutató irányvektora az orientáció (yaw, pitch) alapján.
        /// </summary>
        public Vector3D<float> Forward
        {
            get
            {
                float x = (float)(Math.Cos(pitch) * Math.Cos(yaw));
                float y = (float)(Math.Sin(pitch));
                float z = (float)(Math.Cos(pitch) * Math.Sin(yaw));
                return Vector3D.Normalize(new Vector3D<float>(x, y, z));
            }
        }

        /// <summary>
        /// A kamera jobbra mutató irányvektora.
        /// </summary>
        public Vector3D<float> Right
        {
            get
            {
                return Vector3D.Normalize(Vector3D.Cross(Forward, Vector3D<float>.UnitY));
            }
        }

        /// <summary>
        /// A kamera felfelé mutató vektora.
        /// </summary>
        public Vector3D<float> UpVector
        {
            get
            {
                return Vector3D.Normalize(Vector3D.Cross(Right, Forward));
            }
        }

        /// <summary>
        /// A pont, amire a kamera éppen néz (tetszőleges orientációban).
        /// </summary>
        public Vector3D<float> Target
        {
            get
            {
                return Position + Forward;
            }
        }

        // Mozgás (pozíció változtatása)
        public void MoveForward()
        {
            Position += Forward * MoveStep;
        }

        public void MoveBackward()
        {
            Position -= Forward * MoveStep;
        }

        public void StrafeLeft()
        {
            Position -= Right * MoveStep;
        }

        public void StrafeRight()
        {
            Position += Right * MoveStep;
        }

        public void MoveUp()
        {
            Position += UpVector * MoveStep;
        }

        public void MoveDown()
        {
            Position -= UpVector * MoveStep;
        }

        // Forgás / billenés (orientáció változtatása)
        public void TurnLeft()
        {
            yaw -= AngleStep;
        }

        public void TurnRight()
        {
            yaw += AngleStep;
        }

        public void PitchUp()
        {
            pitch += AngleStep;
            double maxPitch = Math.PI / 2.0 - 0.05;
            if (pitch > maxPitch) pitch = maxPitch;
        }

        public void PitchDown()
        {
            pitch -= AngleStep;
            double minPitch = -Math.PI / 2.0 + 0.05;
            if (pitch < minPitch) pitch = minPitch;
        }
    }
}
