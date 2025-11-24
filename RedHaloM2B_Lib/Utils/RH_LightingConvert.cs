namespace RedHaloM2B
{
    internal class LightingConvert
    {
        public static float Default2Watt(float baseimg)
        {
            return (float)baseimg * 0.790629632f;
        }

        public static float Lumen2Watt(float lumen)
        {
            return lumen / 683.0f;
        }

        public static float Radiance2Watt(float radiance)
        {
            return radiance * 0.88419408f;
        }

        public static float Luminance2Watt(float luminance)
        {
            return luminance * 603.904512f;
        }

        public static float Candel2Watt(float candel)
        {
            return candel / 245.880047f;
        }

    }
}