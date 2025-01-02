using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedHaloM2B
{
    internal class LightingConvert
    {
        /// <summary>
        /// Convert Lumen to Candel
        /// https://www.gophotonics.com/calculators/lumens-to-candela-conversion-calculator
        /// lumionous Intensity(cd) = Lumionous Flux(lm) / (2*PI * (1 - cos(θ/2)))
        /// </summary>
        /// <param name="lumen">Lumen Intensity</param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static float Lumen2Candel(float lumen, float angle = 26.9303f)
        {
            // var cd = lumen * 2 * Math.PI * (1 - Math.Cos(angle / 2));
            var cd = lumen / (2 * Math.PI * (1 - Math.Cos(angle / 2)));
            return (float)cd;
        }

        public static float Lumen2Watt(float lumen)
        {
            return lumen / 683.00f;
        }
        public static float Default2Watt(float baseimg)
        {
            float lumens = baseimg / 1.488634f;
            float watt = Lumen2Watt(lumens);
            return (float)watt;
        }
        public static float Watt2Lumen(float watt)
        {
            return watt * 683.0f;
        }
        public static float Lumens2Default(float lumen)
        {
            var img = lumen * 1.488634;
            return (float)img;
        }
        public static float Luminance2Lumen(float luminance)
        {
            return luminance / 28.9462f;
        }

        public static float Default2Lumen(float image)
        {
            return image / 1.488634f;
        }
        public static float Candel2Lumen(float candel, float angle = 26.9303f)
        {
            var lumen = candel * (2 * Math.PI * (1 - Math.Cos(angle / 2)));
            return (float)lumen;
        }
        public static float Candel2Watt(float candel)
        {
            var lumen = Candel2Lumen(candel);
            var watt = Lumen2Watt(lumen);
            return watt;
        }
        public static float Radiance2Lumen(float radius)
        {
            return radius / 0.042381f;
        }
        public static float Radiance2Watt(float radius)
        {
            var lm = Radiance2Lumen(radius);
            var watt = Lumen2Watt(lm);
            return watt;
        }
    }
}
