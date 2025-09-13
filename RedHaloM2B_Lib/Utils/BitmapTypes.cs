namespace RedHaloM2B.Utils
{
    public class BITMAP_TYPES
    {
        public enum BitmapType
        {
            BMM_NO_TYPE         = 0, //Not allocated yet.
            BMM_LINE_ART        = 1, //1-bit monochrome image 
            BMM_PALETTED        = 2, //8-bit paletted image. 
            BMM_GRAY_8          = 3, //8-bit grayscale bitmap. 
            BMM_GRAY_16         = 4, //16-bit grayscale bitmap. 
            BMM_TRUE_16         = 5, //16-bit true color image.
            BMM_TRUE_32         = 6, //32-bit color: 8 bits each for Red, Green, Blue, and Alpha.
            BMM_TRUE_64         = 7, //64-bit color: 16 bits each for Red, Green, Blue, and Alpha.
            BMM_LOGLUV_32       = 13, //This format uses a logarithmic encoding of luminance and U' and V' in the CIE perceptively uniform space.
            BMM_LOGLUV_24       = 14, //This format is similar to BMM_LOGLUV_32 except is uses smaller values to give a span of 5 order of magnitude from 1/4096 to 16 in 1.1% luminance steps.
            BMM_LOGLUV_24A      = 15, //This format is similar to BMM_LOGLUV_24, except the 8 bit alpha value is kept with the 24 bit color value in a single 32 bit word.
            BMM_REALPIX_32      = 16,  //The "Real Pixel" format.
            BMM_FLOAT_RGBA_32   = 17, //32-bit floating-point per component (non-compressed), RGB with or without alpha
            BMM_FLOAT_GRAY_32   = 18, //32-bit floating-point(non-compressed), monochrome/grayscale
            BMM_TRUE_24         = 8, //24-bit color: 8 bits each for Red, Green, and Blue.
            BMM_TRUE_48         = 9, //48-bit color: 16 bits each for Red, Green, and Blue.
            BMM_YUV_422         = 10, //This is the YUV format - CCIR 601.
            BMM_BMP_4           = 11, //Windows BMP 16-bit color bitmap.
            BMM_PAD_24          = 12, //Padded 24-bit (in a 32 bit register).
            BMM_FLOAT_RGB_32    = 19, //ONLY returned by the GetStoragePtr() method of BMM_FLOAT_RGBA_32 storage, NOT an actual storage type! When GetStoragePtr() returns this type, the data should be interpreted as three floating-point values, corresponding to Red, Green, and Blue (in this order).
            BMM_FLOAT_A_32      = 20, //ONLY returned by the GetAlphaStoragePtr() method of BMM_FLOAT_RGBA_32 or BMM_FLOAT_GRAY_32 storage, NOT an actual storage type! When GetStorageAlphaPtr() returns this type, the data should be interpreted as floating-point values one value per pixel, corresponding to Alpha.
        }
    }
}
