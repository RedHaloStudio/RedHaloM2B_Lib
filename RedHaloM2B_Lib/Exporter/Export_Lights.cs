using Autodesk.Max;
using RedHaloM2B.Nodes;
using System;
using System.Diagnostics;

namespace RedHaloM2B
{
    partial class Exporter
    {
        public static RedHaloLight ExportLights(IINode light, int lightIndex)
        {
            string lightName = $"light_{lightIndex:D5}";
            string lightSourName = light.Name;
            //string gid = Guid.NewGuid().ToString();

            //Set new name
            light.Name = lightName;

            float[] _diffuseColor = [1, 1, 1, 1];
            float _strength = 0.0f;
            string _lighttype = "AREA";
            float _length = 0.0f;
            float _width = 0.0f;
            bool _portal = false;
            float _angle = 0.0f;
            float _angleBlend = 0.0f;
            float _directional = (float)Math.PI;
            bool _diffuse = true;
            bool _Shadow = true;
            bool _specular = true;
            bool _reflection = true;
            bool _invisible = false;
            string _iesfile = "";
            bool _volume = true;

            int color_mode = 0;
            int light_type = 0;

            IPoint3 maxColor = RedHaloCore.Global.Point3.Create(0.9, 0.9, 0.9);
            IColor filterColor = RedHaloCore.Global.Color.Create(0.9, 0.9, 0.9);

            ILightObject maxLight = light.ObjectRef as ILightObject;
            ILightState maxlightState = RedHaloCore.Global.LightState.Create();
            maxLight.EvalLightState(0, RedHaloCore.Forever, maxlightState);
            IIParamBlock2 lightPB = maxLight.SubAnim(0) as IIParamBlock2;

            var matrix = light.GetObjectTM(0, RedHaloCore.Forever);

            int _diff = 1, _shd = 1, _spec = 1, _ref = 1, _invis = 0, _vol = 1;
            float kelvin = 6500;
            int intensityType = 0, useMultiplier = 0;
            float multiplier = 1, intensity = 0;
            switch (maxLight.ClassID.PartA)
            {
                #region VRay Light
                // VrayLight
                case 0x3C5575A1:
                    #region DiffuseColor
                    lightPB.GetValue(lightPB.IndextoID(6), 0, filterColor, RedHaloCore.Forever, 0);

                    lightPB.GetValue(lightPB.IndextoID(4), 0, ref color_mode, RedHaloCore.Forever, 0);

                    if (color_mode == 0)
                    {
                        _diffuseColor = [filterColor.R, filterColor.G, filterColor.B, 1];
                    }
                    else
                    {
                        var kv = 2500.0f;
                        lightPB.GetValue(lightPB.IndextoID(5), 0, ref kv, RedHaloCore.Forever, 0);
                        var clr = RedHaloCore.Global.Color.FromKelvinTemperature(kv, 1);
                        _diffuseColor = [clr.R, clr.G, clr.B, 1];
                    }
                    #endregion

                    #region Strength
                    /// Light Units
                    /// 0:Default(Image) 1:Luminous power(lm) 2:Luminous(lm/m^2/sr) 3:Radiant power(W) 4:Radiance(W/m^2/sr)                        
                    int lightUnit = 0;
                    lightPB.GetValue(lightPB.IndextoID(8), 0, ref lightUnit, RedHaloCore.Forever, 0);
                    if (lightUnit != 0)
                    {
                        lightPB.SetValue(lightPB.IndextoID(8), 0, 0, 0);
                    }
                    
                    float lightStrength = 0f;
                    lightPB.GetValue(lightPB.IndextoID(7), 0, ref lightStrength, RedHaloCore.Forever, 0);

                    switch (lightUnit)
                    {
                        case 0:
                            _strength = LightingConvert.Default2Watt(lightStrength);
                            break;
                        case 1:
                            _strength = LightingConvert.Lumen2Watt(lightStrength);
                            break;
                        case 2:
                            _strength = LightingConvert.Lumen2Watt(lightStrength);
                            break;
                        case 3:
                            _strength = lightStrength;
                            break;
                        case 4:
                            _strength = LightingConvert.Radiance2Watt(lightStrength);
                            break;
                        default:
                            break;
                    }
                    #endregion

                    // Size
                    lightPB.GetValue(lightPB.IndextoID(9), 0, ref _length, RedHaloCore.Forever, 0);
                    lightPB.GetValue(lightPB.IndextoID(10), 0, ref _width, RedHaloCore.Forever, 0);

                    #region Light Shape                        
                    lightPB.GetValue(lightPB.IndextoID(1), 0, ref light_type, RedHaloCore.Forever, 0);

                    // SQUARE, RECTANGLE, DISK, ELLIPSE
                    // POINT, SUN, SPOT, AREA,
                    switch (light_type)
                    {
                        case 0:
                            if (_length == _width)
                            {
                                _lighttype = "SQUARE";
                            }
                            else
                            {
                                _lighttype = "RECTANGLE";
                            }
                            break;
                        case 2:
                            _lighttype = "POINT";
                            break;
                        case 4:
                            _lighttype = "DISK";
                            break;
                        default:
                            break;
                    }
                    #endregion

                    // Direction
                    lightPB.GetValue(lightPB.IndextoID(63), 0, ref _directional, RedHaloCore.Forever, 0);
                    _directional = (1 - _directional) * (float)Math.PI;

                    // Portal
                    int portal = 0;
                    lightPB.GetValue(lightPB.IndextoID(26), 0, ref portal, RedHaloCore.Forever, 0);
                    _portal = Convert.ToBoolean(portal);

                    #region Affect Pass                        
                    lightPB.GetValue(lightPB.IndextoID(17), 0, ref _diff, RedHaloCore.Forever, 0);
                    lightPB.GetValue(lightPB.IndextoID(14), 0, ref _shd, RedHaloCore.Forever, 0);
                    lightPB.GetValue(lightPB.IndextoID(19), 0, ref _spec, RedHaloCore.Forever, 0);
                    lightPB.GetValue(lightPB.IndextoID(21), 0, ref _ref, RedHaloCore.Forever, 0);
                    lightPB.GetValue(lightPB.IndextoID(16), 0, ref _invis, RedHaloCore.Forever, 0);
                    lightPB.GetValue(lightPB.IndextoID(28), 0, ref _vol, RedHaloCore.Forever, 0);

                    _diffuse = Convert.ToBoolean(_diff);
                    _specular = Convert.ToBoolean(_spec);
                    _Shadow = Convert.ToBoolean(_shd);
                    _reflection = Convert.ToBoolean(_ref);
                    _invisible = Convert.ToBoolean(_invis);
                    _volume = Convert.ToBoolean(_vol);
                    #endregion

                    break;

                // "VRayIES"
                case 0x2EAF2F07:
                    #region Diffuse Color
                    lightPB.GetValue(lightPB.IndextoID(28), 0, filterColor, RedHaloCore.Forever, 0);
                    lightPB.GetValue(lightPB.IndextoID(27), 0, ref color_mode, RedHaloCore.Forever, 0);

                    if (color_mode == 0)
                    {
                        _diffuseColor = [filterColor.R, filterColor.G, filterColor.B, 1];
                    }
                    else
                    {
                        float kv = 2500.0f;
                        lightPB.GetValue(lightPB.IndextoID(29), 0, ref kv, RedHaloCore.Forever, 0);
                        var clr = RedHaloTools.GetRgbFromKelvin(kv);
                        _diffuseColor = [clr.R, clr.G, clr.B, 1];
                    }
                    #endregion

                    #region Size and Shape
                    _width = _length = 0.01f;
                    _lighttype = "DISK";
                    #endregion

                    #region IES Filename
                    lightPB.GetValue(lightPB.IndextoID(5), 0, ref _iesfile, RedHaloCore.Forever, 0);
                    _iesfile = RedHaloTools.GetActualPath(_iesfile);
                    #endregion

                    #region Affect Pass
                    lightPB.GetValue(lightPB.IndextoID(12), 0, ref _diff, RedHaloCore.Forever, 0);
                    lightPB.GetValue(lightPB.IndextoID(11), 0, ref _shd, RedHaloCore.Forever, 0);
                    lightPB.GetValue(lightPB.IndextoID(14), 0, ref _spec, RedHaloCore.Forever, 0);
                    lightPB.GetValue(lightPB.IndextoID(14), 0, ref _ref, RedHaloCore.Forever, 0);
                    //lightPB.GetValue(lightPB.IndextoID(21), 0, ref _invis, RedHaloCore.Forever, 0);
                    lightPB.GetValue(lightPB.IndextoID(16), 0, ref _vol, RedHaloCore.Forever, 0); // Volume

                    _diffuse = Convert.ToBoolean(_diff);
                    _specular = Convert.ToBoolean(_spec);
                    _Shadow = Convert.ToBoolean(_shd);
                    _reflection = Convert.ToBoolean(_ref);
                    //_invisible = Convert.ToBoolean(_invis);
                    _volume = Convert.ToBoolean(_vol);
                    #endregion

                    break;

                // "VRaySun"
                case 0x732C0383:

                    #region Diffuse Color
                    lightPB.GetValue(lightPB.IndextoID(4), 0, filterColor, RedHaloCore.Forever, 0);
                    _diffuseColor = [filterColor.R, filterColor.G, filterColor.B, 1];
                    #endregion

                    #region Strength
                    lightPB.GetValue(lightPB.IndextoID(2), 0, ref _strength, RedHaloCore.Forever, 0);
                    #endregion

                    #region Light Shape / Size
                    lightPB.GetValue(lightPB.IndextoID(3), 0, ref _width, RedHaloCore.Forever, 0);
                    _lighttype = "SUN";
                    #endregion

                    #region Affect Pass
                    lightPB.GetValue(lightPB.IndextoID(14), 0, ref _diff, RedHaloCore.Forever, 0);
                    // lightPB.GetValue(lightPB.IndextoID(11), 0, ref _shd, RedHaloCore.Forever, 0);
                    lightPB.GetValue(lightPB.IndextoID(16), 0, ref _spec, RedHaloCore.Forever, 0);
                    lightPB.GetValue(lightPB.IndextoID(16), 0, ref _ref, RedHaloCore.Forever, 0);
                    lightPB.GetValue(lightPB.IndextoID(13), 0, ref _invis, RedHaloCore.Forever, 0);
                    lightPB.GetValue(lightPB.IndextoID(18), 0, ref _vol, RedHaloCore.Forever, 0); // Volume

                    _diffuse = Convert.ToBoolean(_diff);
                    _specular = Convert.ToBoolean(_spec);
                    // _Shadow = Convert.ToBoolean(_shd);
                    _reflection = Convert.ToBoolean(_ref);
                    _invisible = Convert.ToBoolean(_invis);
                    _volume = Convert.ToBoolean(_vol);
                    #endregion

                    break;

                #endregion

                #region Corona Light
                // "CoronaLight"
                case 0x423045E5:
                    #region Color
                    lightPB.GetValue(lightPB.IndextoID(6), 0, filterColor, RedHaloCore.Forever, 0);
                    lightPB.GetValue(lightPB.IndextoID(8), 0, ref color_mode, RedHaloCore.Forever, 0);

                    switch (color_mode)
                    {
                        case 0:
                            _diffuseColor = [filterColor.R, filterColor.G, filterColor.B, 1];
                            break;
                        case 1:
                            var kv = 2500.0f;
                            lightPB.GetValue(lightPB.IndextoID(10), 0, ref kv, RedHaloCore.Forever, 0);
                            var clr = RedHaloTools.GetRgbFromKelvin(kv);
                            _diffuseColor = [clr.R, clr.G, clr.B, 1];
                            break;
                        default:
                            break;
                    }
                    #endregion

                    #region Strength
                    int intensityUnits = 0;
                    lightPB.GetValue(lightPB.IndextoID(9), 0, ref intensityUnits, RedHaloCore.Forever, 0);
                    if (intensityUnits == 0)
                    {
                        lightPB.SetValue(lightPB.IndextoID(9), 0, 0, 0);
                    }

                    float cr_strength = 0;
                    lightPB.GetValue(lightPB.IndextoID(7), 0, ref cr_strength, RedHaloCore.Forever, 0);

                    switch (intensityUnits)
                    {
                        case 0: // W/(sr*m^2)
                            _strength = cr_strength;
                            break;
                        case 1: // Candel
                            _strength = LightingConvert.Candel2Watt(cr_strength);
                            break;
                        case 2: // Lumen
                            _strength = LightingConvert.Lumen2Watt(cr_strength);
                            break;
                        case 3: // Lx
                            _strength = LightingConvert.Luminance2Watt(cr_strength);
                            break;
                        default:
                            break;
                    }
                    #endregion

                    #region Light Size
                    lightPB.GetValue(lightPB.IndextoID(15), 0, ref _length, RedHaloCore.Forever, 0);
                    lightPB.GetValue(lightPB.IndextoID(16), 0, ref _width, RedHaloCore.Forever, 0);
                    #endregion

                    #region Light Shape
                    lightPB.GetValue(lightPB.IndextoID(1), 0, ref light_type, RedHaloCore.Forever, 0); /// 0:Sphere 1:Rectangle 2:Disk 3:Cylinder

                    // SQUARE, RECTANGLE, DISK, ELLIPSE
                    // POINT, SUN, SPOT, AREA,
                    switch (light_type)
                    {
                        case 0:
                            _lighttype = "POINT";
                            break;
                        case 1:
                            if (_length == _width)
                            {
                                _lighttype = "SQUARE";
                            }
                            else
                            {
                                _lighttype = "RECTANGLE";
                            }
                            break;
                        case 2:
                        case 3:
                            _lighttype = "DISK";
                            break;
                        default:
                            break;
                    }
                    #endregion

                    #region Direconal
                    float dir = 0f;
                    lightPB.GetValue(lightPB.IndextoID(17), 0, ref dir, RedHaloCore.Forever, 0);
                    _directional = (float)((1 - dir) * Math.PI);
                    #endregion

                    #region IES Filename
                    lightPB.GetValue(lightPB.IndextoID(20), 0, ref _iesfile, RedHaloCore.Forever, 0);
                    _iesfile = RedHaloTools.GetActualPath(_iesfile);
                    #endregion

                    #region Affect Pass
                    lightPB.GetValue(lightPB.IndextoID(3), 0, ref _spec, RedHaloCore.Forever, 0);
                    lightPB.GetValue(lightPB.IndextoID(3), 0, ref _ref, RedHaloCore.Forever, 0);
                    lightPB.GetValue(lightPB.IndextoID(2), 0, ref _invis, RedHaloCore.Forever, 0);

                    _specular = Convert.ToBoolean(_spec);
                    _reflection = Convert.ToBoolean(_ref);
                    _invisible = Convert.ToBoolean(_invis);
                    #endregion

                    break;

                // "CoronaSun"
                case 0x7C3A3C80:
                    #region Color
                    lightPB.GetValue(lightPB.IndextoID(6), 0, filterColor, RedHaloCore.Forever, 0);
                    lightPB.GetValue(lightPB.IndextoID(5), 0, ref color_mode, RedHaloCore.Forever, 0);

                    switch (color_mode)
                    {
                        case 2:
                            _diffuseColor = [1, 1, 1, 1];
                            break;
                        case 0:
                            _diffuseColor = [filterColor.R, filterColor.G, filterColor.B, 1];
                            break;
                        case 1:
                            var kv = 2800.0f;
                            lightPB.GetValue(lightPB.IndextoID(7), 0, ref kv, RedHaloCore.Forever, 0);
                            var clr = RedHaloTools.GetRgbFromKelvin(kv);
                            _diffuseColor = [clr.R, clr.G, clr.B, 1];
                            break;
                        default:
                            break;
                    }
                    #endregion

                    #region Strength
                    lightPB.GetValue(lightPB.IndextoID(4), 0, ref _strength, RedHaloCore.Forever, 0);
                    #endregion

                    #region Light Size
                    lightPB.GetValue(lightPB.IndextoID(8), 0, ref _width, RedHaloCore.Forever, 0);
                    #endregion

                    #region Light Shape
                    _lighttype = "SUN";
                    #endregion

                    #region Affect Pass
                    lightPB.GetValue(lightPB.IndextoID(2), 0, ref _spec, RedHaloCore.Forever, 0);
                    lightPB.GetValue(lightPB.IndextoID(2), 0, ref _ref, RedHaloCore.Forever, 0);
                    lightPB.GetValue(lightPB.IndextoID(1), 0, ref _invis, RedHaloCore.Forever, 0);

                    _specular = Convert.ToBoolean(_spec);
                    _reflection = Convert.ToBoolean(_ref);
                    _invisible = Convert.ToBoolean(_invis);
                    #endregion

                    break;

                #endregion

                #region Max inside light
                // "Omnilight"
                case 0x00001011:
                    #region Color

                    maxColor = maxLight.GetRGBColor(0, RedHaloCore.Forever);
                    _diffuseColor = [maxColor.X, maxColor.Y, maxColor.Z, 1];
                    #endregion

                    #region Strength
                    _strength = maxLight.GetIntensity(0);
                    #endregion

                    #region Light Shape
                    _lighttype = "POINT";
                    #endregion

                    #region Affect Pass
                    _diffuse = maxlightState.AffectDiffuse;
                    _specular = maxlightState.AffectSpecular;
                    _Shadow = maxlightState.Shadow;
                    _reflection = maxlightState.AffectSpecular;
                    #endregion

                    break;

                // "Directionallight"
                case 0x00001013:
                // "TargetDirectionallight"
                case 0x00001015:
                    #region Color
                    maxColor = maxLight.GetRGBColor(0, RedHaloCore.Forever);
                    _diffuseColor = [maxColor.X, maxColor.Y, maxColor.Z, 1];
                    #endregion

                    #region Strength
                    _strength = maxLight.GetIntensity(0);
                    #endregion

                    #region Light Shape
                    _lighttype = "SUN";
                    #endregion

                    #region Affect Pass
                    _diffuse = maxlightState.AffectDiffuse;
                    _specular = maxlightState.AffectSpecular;
                    _Shadow = maxlightState.Shadow;
                    _reflection = maxlightState.AffectSpecular;
                    #endregion
                    break;

                // "freeSpot"
                case 0x00001014:
                // "targetSpot"
                case 0x00001012:
                    #region Color
                    maxColor = maxLight.GetRGBColor(0, RedHaloCore.Forever);
                    _diffuseColor = [maxColor.X, maxColor.Y, maxColor.Z, 1];
                    #endregion

                    #region Strength
                    _strength = maxLight.GetIntensity(0);
                    #endregion

                    #region Light Shape
                    _lighttype = "SPOT";
                    #endregion

                    #region Hotspot
                    float falloff = maxLight.GetFallsize(0) + 0.5f;
                    float hotspot = maxLight.GetHotspot(0) + 0.5f;
                    _angle = Convert.ToSingle(falloff * Math.PI / 180);
                    _angleBlend = (falloff - hotspot) / falloff;
                    #endregion

                    #region Affect Pass
                    _diffuse = maxlightState.AffectDiffuse;
                    _specular = maxlightState.AffectSpecular;
                    _Shadow = maxlightState.Shadow;
                    _reflection = maxlightState.AffectSpecular;
                    #endregion
                    break;

                // Free_Light
                case 0x32375FCC:
                // Target_Light
                case 0x658D4F97:
                //Free_Linear
                case 0x78207401:
                //Target_Linear
                case 0x45076885:
                //Free_Area
                case 0x36507D92:
                //Target_Area
                case 0x71794F9D:
                //Free_Disc
                case 0x5BCC6D42:
                //Target_Disc
                case 0x38732348:
                //Free_Sphere
                case 0x7CA93582:
                //Target_Sphere
                case 0x33FC7AE9:
                //Free_Cylinder
                case 0x46F634E3:
                //Target_Cylinder
                case 0x7C8B5B10:
                    #region Color
                    var useKelvin = 0;
                    lightPB.GetValue(lightPB.IndextoID(8), 0, ref useKelvin, RedHaloCore.Forever, 0);

                    //Kelvin Color
                    lightPB.GetValue(lightPB.IndextoID(7), 0, ref kelvin, RedHaloCore.Forever, 0);
                    var kelvinClr = RedHaloTools.GetRgbFromKelvin(kelvin);

                    // lightPB.GetValue(lightPB.IndextoID(3), 0, filterColor, RedHaloCore.Forever, 0);
                    filterColor = useKelvin != 1 ? filterColor : kelvinClr;

                    lightPB.GetValue(lightPB.IndextoID(4), 0, filterColor, RedHaloCore.Forever, 0);
                    var finalColor = filterColor.Multiply(filterColor);
                    _diffuseColor = [finalColor.R, finalColor.G, finalColor.B, 1];
                    #endregion

                    #region Strength                        
                    lightPB.GetValue(lightPB.IndextoID(14), 0, ref useMultiplier, RedHaloCore.Forever, 0);
                    if (useMultiplier != 0)
                    {
                        lightPB.GetValue(lightPB.IndextoID(15), 0, ref multiplier, RedHaloCore.Forever, 0);
                    }

                    lightPB.GetValue(lightPB.IndextoID(5), 0, ref intensity, RedHaloCore.Forever, 0);

                    switch (intensityType)
                    {
                        // lm
                        case 0:
                            _strength = LightingConvert.Lumen2Watt(intensity * multiplier);
                            break;
                        //cd
                        case 1:
                            _strength = LightingConvert.Candel2Watt(intensity * multiplier);
                            break;
                        //lx(lux)
                        case 2:
                            _strength = LightingConvert.Radiance2Watt(intensity * multiplier);
                            break;
                        default:
                            break;
                    }

                    #endregion

                    #region Light Shape
                    //POINT
                    if (maxLight.ClassID.PartA == 842489804 || maxLight.ClassID.PartA == 1703759767)
                    {
                        _lighttype = "POINT";
                    }
                    //LINE
                    if (maxLight.ClassID.PartA == 2015392769 || maxLight.ClassID.PartA == 1158113413)
                    {
                        _lighttype = "AREA";
                        _width = 0.01f;
                        _length = 1;
                    }
                    //AREA
                    if (maxLight.ClassID.PartA == 911244690 || maxLight.ClassID.PartA == 1903775645)
                    {
                        _lighttype = "AREA";
                        _width = 1;
                        _length = 1;
                    }
                    //DISK
                    if (maxLight.ClassID.PartA == 1540123970 || maxLight.ClassID.PartA == 947069768)
                    {
                        _lighttype = "DISK";
                        _width = 1;
                    }
                    //SPHERE
                    if (maxLight.ClassID.PartA == 2091464066 || maxLight.ClassID.PartA == 872184553)
                    {
                        _lighttype = "POINT";
                        _width = 0.1f;
                    }
                    //CYLINDER
                    if (maxLight.ClassID.PartA == 1190540515 || maxLight.ClassID.PartA == 2089507600)
                    {
                        _lighttype = "POINT";
                        _width = 0.1f;
                    }
                    #endregion

                    #region Affect Pass
                    _diffuse = maxlightState.AffectDiffuse;
                    _specular = maxlightState.AffectSpecular;
                    _reflection = maxlightState.AffectSpecular;
                    #endregion
                    break;

                // Skylight
                case 0x7BF61478:

                    break;
                #endregion

                // Arnold Light
                case 0x6705F00D:
                    break;

                default:
                    break;
            }

            RedHaloLight redHaloLight = new()
            {
                Name = lightName,
                OriginalName = lightSourName,
                //ID = gid,
                Transform = RedHaloTools.ConvertMatrixToString(matrix),
                Color = _diffuseColor,
                Strength = _strength,
                Length = _length,
                Width = _width,
                LightType = _lighttype,
                IES = _iesfile,
                Portal = _portal,
                Angle = _angle,
                AngleBlend = _angleBlend,
                Directional = _directional,
                Shadow = _Shadow,
                Specular = _specular,
                Diffuse = _diffuse,
                Reflection = _reflection
            };

            return redHaloLight;
        }
    }
}
