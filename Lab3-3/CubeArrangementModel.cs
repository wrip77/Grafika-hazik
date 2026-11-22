using System;

namespace Lab3_3
{
    internal class CubeArrangementModel
    {
        public double CurrentSliceAngle { get; private set; } = 0;

        private double targetSliceAngle = 0;

        private const double RotationSpeed = Math.PI;

        public void RotateSliceForward()
        {
            targetSliceAngle += Math.PI / 2.0;
        }

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
