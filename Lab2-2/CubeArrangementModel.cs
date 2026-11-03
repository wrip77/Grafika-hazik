using System;

namespace Szeminarium
{
    internal class CubeArrangementModel
    {
        /// <summary>
        /// A kiválasztott szelet jelenlegi elforgatási szöge (radiánban).
        /// </summary>
        public double CurrentSliceAngle { get; private set; } = 0;

        /// <summary>
        /// A cél szög, ameddig a szeletnek el kell fordulnia (90 fokos lépésekben).
        /// </summary>
        private double targetSliceAngle = 0;

        /// <summary>
        /// Forgási sebesség (radián / másodperc).
        /// </summary>
        private const double RotationSpeed = Math.PI; // 180 fok/mp -> 0.5 mp alatt fordul 90 fokot

        /// <summary>
        /// Elindít egy +90 fokos forgatást az egyik irányba (Space).
        /// </summary>
        public void RotateSliceForward()
        {
            targetSliceAngle += Math.PI / 2.0;
        }

        /// <summary>
        /// Elindít egy -90 fokos forgatást a fordított irányba (Backspace).
        /// </summary>
        public void RotateSliceBackward()
        {
            targetSliceAngle -= Math.PI / 2.0;
        }

        internal void AdvanceTime(double deltaTime)
        {
            double diff = targetSliceAngle - CurrentSliceAngle;
            if (Math.Abs(diff) < 0.0001)
            {
                CurrentSliceAngle = targetSliceAngle;
                return;
            }

            double step = RotationSpeed * deltaTime;
            if (Math.Abs(diff) <= step)
            {
                CurrentSliceAngle = targetSliceAngle;
            }
            else
            {
                CurrentSliceAngle += Math.Sign(diff) * step;
            }
        }
    }
}
