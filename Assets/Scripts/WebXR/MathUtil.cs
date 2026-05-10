namespace haw.pd20.webxr
{
    public class MathUtil
    {
        public static float Map(float oldMin, float oldMax, float newMin, float newMax, float value)
        {
            return ((value - oldMin) * (newMax - newMin) / (oldMax - oldMin)) + newMin;
        }
    }
}