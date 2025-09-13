using Autodesk.Max;
using RedHaloM2B.Textures;
using RedHaloM2B.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Media;

namespace RedHaloM2B.RedHaloUtils
{
    internal class MaterialUtils
    {
        /// <summary>
        /// 获取场景中的所有材质，包括子材质
        /// </summary>
        /// <returns> 获取场景中的所有材质，包括子材质</returns>
        public static IEnumerable<IMtl> GetSceneMaterials()
        {
            var mtls = RedHaloCore.Core.SceneMtls;
            var sceneMaterials = new List<IMtl>();

            // 清除重复的材质并修复名字
            //List<IMtl> uniqueMaterials = new List<IMtl>();
            var uniqueMaterials = new HashSet<IMtl>();
            HashSet<IMtl> materialSet = new HashSet<IMtl>();

            for (int i = 0; i < mtls.Count; i++)
            {
                if (mtls[i] is IMtl material)
                {
                    sceneMaterials.Add(material);
                    // 如果材质有子材质，进入递归获取材质
                    if (material.NumSubMtls > 0)
                    {
                        sceneMaterials.AddRange(GetSceneMtlRecure(material));
                    }
                }
            }
            
            Dictionary<string, int> nameCounts = new Dictionary<string, int>();

            foreach (var mtl in sceneMaterials)
            {
                // 修复材质名字
                mtl.Name = RedHaloTools.FixSafeName(mtl.Name);

                if(uniqueMaterials.Add(mtl))
                {
                    //materialSet.Add(mtl);
                    if(nameCounts.ContainsKey(mtl.Name))
                    {
                        nameCounts[mtl.Name]++;
                        mtl.Name = $"{mtl.Name}_{nameCounts[mtl.Name]}";
                    }
                    else
                    {
                        nameCounts[mtl.Name] = 0;
                    }
                }
            }

            return uniqueMaterials;
        }

        // 使用递归方法取得材质的子材质
        public static IEnumerable<IMtl> GetSceneMtlRecure(IMtl mtl)
        {
            for (int i = 0; i < mtl.NumSubMtls; i++)
            {
                var subMtl = mtl.GetSubMtl(i);
                if (subMtl != null)
                {
                    yield return subMtl;
                    foreach (var child in GetSceneMtlRecure(subMtl))
                    {
                        yield return child;
                    }
                }
            }
        }

        /// <summary>
        /// 根据UVGen标志计算并添加 image_wrap 属性。
        /// </summary>
        private static void AddImageWrapProperty(TexmapInfo ti, IStdUVGen stdUVGen)
        {
            if (ti == null) return;
            if (stdUVGen == null) return;

            bool tileU = stdUVGen.GetFlag(1 << 0) == 1;
            bool tileV = stdUVGen.GetFlag(1 << 1) == 1;
            bool mirrorU = stdUVGen.GetFlag(1 << 2) == 1;
            bool mirrorV = stdUVGen.GetFlag(1 << 3) == 1;

            bool anyWrap = tileU || tileV;
            bool anyMirror = mirrorU || mirrorV;

            string imageWrap;
            if (anyMirror && !anyWrap)
            {
                imageWrap = "MIRROR";
            }
            else if (anyWrap || anyMirror)
            {
                imageWrap = "REPEAT";
            }
            else
            {
                imageWrap = "CLIP";
            }
            ti.Properties.Add("image_wrap", imageWrap);
        }

        /// <summary>
        /// 添加UV变换相关的属性（缩放、偏移、旋转）。
        /// </summary>
        private static void AddUvTransformProperties(TexmapInfo ti, IStdUVGen stdUVGen)
        {
            if (ti == null) return;
            if (stdUVGen == null) return;

            ti.Properties.Add("u_scale", stdUVGen.GetUScl(0));
            ti.Properties.Add("v_scale", stdUVGen.GetVScl(0));
            ti.Properties.Add("u_offset", stdUVGen.GetUOffs(0));
            ti.Properties.Add("v_offset", stdUVGen.GetVOffs(0));
            ti.Properties.Add("w_angle", stdUVGen.GetWAng(0));
            ti.Properties.Add("v_angle", stdUVGen.GetVAng(0));
            ti.Properties.Add("u_angle", stdUVGen.GetUAng(0));
        }

        /// <summary>
        /// 处理3ds Max标准的纹理/环境映射逻辑。
        /// </summary>
        private static void AddStandardMappingProperties(TexmapInfo ti, IStdUVGen stdUVGen, IUVGen uvgen)
        {
            if (uvgen == null || stdUVGen == null) return;

            // 0:TEXTURE, 1:ENVIRON
            if (uvgen.SlotType == 0)
            {
                ti.Properties.Add("mapping_type", "TEXTURE");
                // 使用C# 8.0 switch表达式，更简洁
                string mappingUvw = uvgen.UVWSource switch
                {
                    0 => "EXPLICIT",
                    1 => "OBJECT",
                    2 => "VERTEX",
                    3 => "WORLD",
                    _ => "EXPLICIT"
                };
                ti.Properties.Add("mapping_uvw", mappingUvw);
            }
            else
            {
                ti.Properties.Add("mapping_type", "ENVIRON");
                string mappingUvw = stdUVGen.GetCoordMapping(0) switch
                {
                    1 => "SPHERICAL", // UVMAP_SPHERE_ENV
                    2 => "CYLIND",    // UVMAP_CYL_ENV
                    3 => "SHRINK",    // UVMAP_SHRINK_ENV
                    4 => "SCREEN",    // UVMAP_SCREEN_ENV
                    _ => "UNKNOWN"    // 提供一个默认值
                };
                ti.Properties.Add("mapping_uvw", mappingUvw);
            }
        }

        /// <summary>
        /// 处理VRay特有的环境映射逻辑。
        /// </summary>
        private static void AddVRayEnvironmentMappingProperties(TexmapInfo ti, int environType)
        {
            ti.Properties.Add("mapping_type", "ENVIRON");
            string mappingUvw = environType switch
            {
                0 => "ANGULAR",
                1 => "CUBIC",
                2 => "SPHERICAL",
                3 => "MIRROR_BALL",
                _ => "UNKNOWN"
            };
            ti.Properties.Add("mapping_uvw", mappingUvw);
        }

        /// <summary>
        /// 渲染裁切的图片，并返回新图片。<br/>
        /// 暂时解决不了TheManager.Create(bitmapInfo)问题，就用Maxscript暂时实现
        /// </summary>
        /// <param name="inBitmap">IPBBitmap</param>
        /// <param name="clipu">裁切U</param>
        /// <param name="clipv">裁切V</param>
        /// <param name="clipw">裁切宽度</param>
        /// <param name="cliph">裁切高度</param>
        /// <returns></returns>
        public static IBitmapTex RenderAndCropImage(IPBBitmap inBitmap, float clipu, float clipv, float clipw, float cliph)
        {
            // 检查输入的IPBBitmap是否有效
            if (inBitmap == null || inBitmap.Bi == null)
            {
                Debug.Print("Invalid bitmap input.");
                return null;
            }
            // 获取图片的真实路径
            string filename = RedHaloTools.GetActualPath(inBitmap.Bi.Name);

            // 检查文件名是否为空或文件不存在
            if (string.IsNullOrEmpty(filename) || !File.Exists(filename))
                return null;

            // 获取filename的路径和后缀名
            string dirPath = Path.GetDirectoryName(filename);   //返回文件所在目录 "d:\test"
            string extension = Path.GetExtension(filename);     //扩展名 ".jpg"
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filename); // 没有扩展名的文件名 "default"

            string new_prex = Guid.NewGuid().ToString("N").Substring(8, 6);
            // 生成新的文件名
            string newfilename = Path.Combine(dirPath, $"{fileNameWithoutExtension}-{new_prex}{extension}");

            // 获取图片的宽度和高度
            int width = (int)(inBitmap.Bi.Width * clipw);
            int height = (int)(inBitmap.Bi.Height * cliph);

            string script = "bm = undefined\n";
            script += $"bm = Bitmaptexture filename:\"{filename}\" name:\"name_crop\" apply:on clipu:{clipu} clipv:{clipv} clipw:{clipw} cliph:{cliph}\n";
            script += $"new_bm = renderMap bm size:[{width}, {height}] filename:\"{newfilename}\"\n";
            script += @"save new_bm
                       close new_bm
                    ";

            IFPValue result = RedHaloCore.Global.FPValue.Create();
            bool runok = RedHaloCore.Global.ExecuteMAXScriptScript(script, Autodesk.Max.MAXScript.ScriptSource.Embedded, true, result, true);

            if (runok)
            {
                // 计算文件的Hash值，用于比较场景中的纹理是否相同
                // 如果不相同，使用新纹理，否则使用原纹理

                // Load new bitmap
                IBitmapTex newBitmap = RedHaloCore.Global.NewDefaultBitmapTex;
                newBitmap.SetMapName(newfilename, true);

                return newBitmap;
            }            

            return null;
        }


        // 纹理参数
        public static TexmapInfo ExportTexmap(ITexmap tex)
        {
            if (tex == null)
            {
                return null;
            }
            var texType = tex.ClassName(false);            

            IColor maxRGB = RedHaloCore.Global.Color.Create(0.8, 0.8, 0.8);
            IAColor maxRGBA = RedHaloCore.Global.AColor.Create(0.8, 0.8, 0.8, 1);
            float amount = 0.0f;
            IBitmapTex new_bm = null;

            //Debug.Print($"======{texType.ToUpper()}======");
            
            var ti = new TexmapInfo()
            {
                Name = tex.Name,
                Type = texType.ToLower(),
            };

            void AddSubtexmap(string key, ITexmap subtexmap)
            {
                if (subtexmap != null)
                {
                    ti.subTexmapInfo.Add(key, ExportTexmap(subtexmap));
                }
            }   

            switch (texType)
            {
                #region Standard Textures

                case "Bitmap":
                    // 处理图片，如果Apply启用，且Width或Height小于1，渲染一张新图片，并启用此图片
                    int croppintApply = RedHaloTools.GetValueByID<int>(tex, 0, RH_ParamID.Std_CroppingApply);
                    float ClipH = RedHaloTools.GetValueByID<float>(tex, 0, RH_ParamID.Std_ClipH);
                    float ClipU = RedHaloTools.GetValueByID<float>(tex, 0, RH_ParamID.Std_ClipU);
                    float ClipV = RedHaloTools.GetValueByID<float>(tex, 0, RH_ParamID.Std_ClipV);
                    float ClipW = RedHaloTools.GetValueByID<float>(tex, 0, RH_ParamID.Std_ClipW);

                    string _filename = RedHaloTools.GetActualPath(RedHaloTools.GetValueByID<string>(tex, 0, 16));

                    var fileExt = Path.GetExtension(_filename);

                    // 复制EXR文件到临时目录，原因是EXR文件在3ds Max中不支持中文路径
                    if (fileExt.ToLower() == ".exr")
                    {
                        var texmapPath = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), "RH_M2B_TEMP");
                        var newFilePath = Path.Combine(texmapPath, Path.GetFileName(_filename));
                        if (RedHaloTools.CopyFiles(_filename, texmapPath))
                        {
                            _filename = newFilePath;
                        }
                    }

                    // 如果开启了裁切，重新渲染一张新图
                    if (croppintApply == 1 && ((ClipH < 1) || (ClipW < 1)))
                    {
                        string orig_filename = _filename;
                        string filepath = Path.GetDirectoryName(orig_filename);
                        string filename = Path.GetFileNameWithoutExtension(orig_filename);
                        string fileext = Path.GetExtension(orig_filename);

                        // 渲染新贴图
                        //var bmInfo = RedHaloTools.GetValueByID<IPBBitmap>(tex, 0, 13);                        

                        //var bm_width = bmInfo.Bi.Width * ClipW > 2 ? bmInfo.Bi.Width * ClipW : 2;
                        //var bm_height = bmInfo.Bi.Height * ClipH > 2 ? bmInfo.Bi.Height * ClipH : 2;

                        //var newFileName = RenderAndCropImage(tex, ClipU, ClipV, ClipW, ClipH);
                    }

                    IStdUVGen stdUVGen = RedHaloTools.GetValueByID<IReferenceTarget>(tex, 0, RH_ParamID.Std_UVGen) as IStdUVGen;
                    IUVGen uvgen = RedHaloTools.GetValueByID<IReferenceTarget>(tex, 0, RH_ParamID.Std_UVGen) as IUVGen;

                    ti.Properties.Add("filename", _filename);

                    ti.Properties.Add("clipu", ClipU);
                    ti.Properties.Add("clipv", ClipV);
                    ti.Properties.Add("clipw", ClipW);
                    ti.Properties.Add("cliph", ClipH);

                    AddImageWrapProperty(ti, stdUVGen);
                    AddUvTransformProperties(ti, stdUVGen);

                    AddStandardMappingProperties(ti, stdUVGen, uvgen);
                    
                    ti.Properties.Add("alpha_source", RedHaloTools.GetValueByID<int>(tex, 0, RH_ParamID.Std_AlphaSource));
                    ti.Properties.Add("mono_output", RedHaloTools.GetValueByID<int>(tex, 0, RH_ParamID.Std_MonoOutput));
                    ti.Properties.Add("rgb_output", RedHaloTools.GetValueByID<int>(tex, 0, RH_ParamID.Std_RgbOutput));
                    ti.Properties.Add("premult_alpha", RedHaloTools.GetValueByID<int>(tex, 0, RH_ParamID.Std_PremultAlpha));

                    break;

                case "MultiTile":
                    Debug.Print($"======{texType}======");
                    //RedHaloTools.GetParams(tex);
                    break;

                case "Tiles":
                    //Debug.Print($"======{texType}======");
                    int numParamBlocks = tex.NumParamBlocks;
                    IAnimatable anim = tex as IAnimatable;

                    if (numParamBlocks == 0 && anim != null)
                    {
                        string paramName = anim.SubAnimName(1);
                        IAnimatable subAnim = anim.SubAnim(1);

                        if (paramName == "Parameters")
                        {
                            if (subAnim is IIParamBlock pb)
                            {
                                //float brickScale = 1f;
                                float brickWidth = 1.0f;
                                float brickHeight = 1.0f;

                                float lineShift = 0f;
                                float squash = 1f;
                                int brick_frequency = 2;
                                int squash_frequency = 2;

                                // 0 Custom Tiles       1 Running Bond  2:Common Flemish Bond   3:English Bond
                                // 4:1/2 Running Bond   5:Stack Bond    6:Fine Running Bond     7:Fine Stack Bond
                                int tile_type = pb.GetInt(21, 0);

                                // 0 Mortar Color: Type: Rgba
                                maxRGB = pb.GetColor(0, 0);
                                ti.Properties.Add("mortar_color", new[] { maxRGB.R, maxRGB.G, maxRGB.B });

                                // 1 Brick Color: Type: Rgba
                                maxRGB = pb.GetColor(1, 0);
                                ti.Properties.Add("brick_color", new[] { maxRGB.R, maxRGB.G, maxRGB.B });

                                // 2 Horizontal Count: Type: Float
                                float horizontal_count = pb.GetFloat(2, 0);
                                // 3 Vertical Count: Type: Float
                                float vertical_count = pb.GetFloat(3, 0);

                                brickWidth = 1 / horizontal_count;
                                brickHeight = 1 / vertical_count;

                                // 4 Color Variance: Type: Float
                                ti.Properties.Add("color_variance", pb.GetFloat(4, 0));

                                // 5 Vertical Gap: Type: Float
                                // 6 Horizontal Gap: Type: Float
                                float mortar_size = 1f;
                                mortar_size = pb.GetFloat(5, 0);

                                // 7 Line Shift: Type: Float
                                // Squash [Blender name]
                                switch (tile_type)
                                {
                                    case 0:
                                        lineShift = 1f - (pb.GetFloat(7, 0) + 0.5f) % 1f;
                                        break;

                                    case 1:
                                    case 2: // Combined cases with the same outcome
                                    case 6:
                                    case 7:
                                        lineShift = 0.5f;
                                        break;

                                    case 3:
                                        lineShift = 0.5f;
                                        squash = 0.5f;
                                        break;

                                    case 4:
                                        lineShift = 0.25f;
                                        break;

                                    case 5:
                                        lineShift = 0f;
                                        break;

                                    default:
                                        break;
                                }

                                ti.Properties.Add("brick_offset", lineShift);
                                ti.Properties.Add("brick_frequency", brick_frequency);
                                ti.Properties.Add("squash", squash);
                                ti.Properties.Add("squash_frequency", squash_frequency);
                                ti.Properties.Add("mortar_size", mortar_size);

                                ti.Properties.Add("brick_width", brickWidth);
                                ti.Properties.Add("brick_height", brickHeight);

                                // 8 Random Shift: Type: Float
                                ti.Properties.Add("random_shift", pb.GetFloat(8, 0));
                            }
                        }

                        // Mortar texmap
                        var mortarTexmap = anim.SubAnim(2);
                        if (mortarTexmap != null)
                        {
                            if (mortarTexmap is ITexmap mtex)
                            {
                                ti.subTexmapInfo.Add("mortar_texmap", ExportTexmap(mtex));
                            }
                        }

                        // Brick texmap
                        var brickTexmap = anim.SubAnim(3);
                        if (brickTexmap != null)
                        {
                            if (brickTexmap is ITexmap btex)
                            {
                                ti.subTexmapInfo.Add("brick_texmap", ExportTexmap(btex));
                            }

                        }

                    }
                    break;

                case "Checker":
                    // Color1
                    maxRGB = RedHaloTools.GetValueByID<IColor>(tex, 0, 1);
                    ti.Properties.Add("color1", new[] { maxRGB.R, maxRGB.G, maxRGB.B, 1 });

                    // Color2
                    maxRGB = RedHaloTools.GetValueByID<IColor>(tex, 0, 2);
                    ti.Properties.Add("color2", new[] { maxRGB.R, maxRGB.G, maxRGB.B, 1 });

                    //submap 1
                    var submap1 = RedHaloTools.GetValueByID<ITexmap>(tex, 0, 3);
                    AddSubtexmap("map1", submap1);

                    //submap 2
                    var submap2 = RedHaloTools.GetValueByID<ITexmap>(tex, 0, 4);
                    AddSubtexmap("map2", submap2);

                    break;
                
                case "Mix":
                    //color1
                    maxRGB = RedHaloTools.GetValueByID<IColor>(tex, 0, 4);
                    ti.Properties.Add("color1", new[] { maxRGB.R, maxRGB.G, maxRGB.B, 1 });
                    
                    //color2
                    maxRGB = RedHaloTools.GetValueByID<IColor>(tex, 0, 5);
                    ti.Properties.Add("color2", new[] { maxRGB.R, maxRGB.G, maxRGB.B, 1 });

                    // mix amnout
                    amount = RedHaloTools.GetValueByID<float>(tex, 0, 0);
                    ti.Properties.Add("multiplier", amount);

                    //ti.Properties.Add("mode", "MIX");

                    //map1
                    var mix_map1 = RedHaloTools.GetValueByID<ITexmap>(tex, 0, 6);
                    AddSubtexmap("map1", mix_map1);

                    // map2
                    var mix_map2 = RedHaloTools.GetValueByID<ITexmap>(tex, 0, 7);
                    AddSubtexmap("map2", mix_map2);

                    // mix mask
                    var mix_mask = RedHaloTools.GetValueByID<ITexmap>(tex, 0, 8);
                    AddSubtexmap("mask", mix_mask);

                    break;
                
                case "Falloff":
                    // color1
                    maxRGBA = RedHaloTools.GetValueByID<IAColor>(tex, 0, 0);
                    ti.Properties.Add("color1", new[] { maxRGB.R, maxRGB.G, maxRGB.B, 1 });

                    // color2
                    maxRGBA = RedHaloTools.GetValueByID<IAColor>(tex, 0, 4);
                    ti.Properties.Add("color2", new[] { maxRGB.R, maxRGB.G, maxRGB.B, 1 });

                    ti.Properties.Add("map1_amount", RedHaloTools.GetValueByID<float>(tex, 0, 1));
                    ti.Properties.Add("map2_amount", RedHaloTools.GetValueByID<float>(tex, 0, 5));

                    /*
                    Falloff Type:
                    0: Towards / Away
                    1: Perpendicular / Parallel
                    2: Fesnel
                    3: Shadow / Light
                    4: Distance Blend  -->  nearDistance == farDistance
                     */
                    ti.Properties.Add("type", RedHaloTools.GetValueByID<int>(tex, 0, 8));

                    /*            
                    Direction:
                    0: Cameram Z
                    1: Cameram X
                    2: Cameral Y
                    3: Object   -- Node Params
                    4: Local X
                    5: Local Y
                    6: Local Z
                    7: World X
                    8: World Y
                    9: World Z
                    */
                    ti.Properties.Add("direction", RedHaloTools.GetValueByID<int>(tex, 0, 9));
                    
                    ti.Properties.Add("ior", RedHaloTools.GetValueByID<int>(tex, 0, 12));

                    ti.Properties.Add("near_distance", RedHaloTools.GetValueByID<int>(tex, 0, 14));
                    ti.Properties.Add("far_distance", RedHaloTools.GetValueByID<int>(tex, 0, 15));
                    // map1
                    var falloff_map1 = RedHaloTools.GetValueByID<ITexmap>(tex, 0, 2);
                    AddSubtexmap("map1", falloff_map1);

                    // map2
                    var falloff_map2 = RedHaloTools.GetValueByID<ITexmap>(tex, 0, 6);
                    AddSubtexmap("map2", falloff_map2);

                    break;
                
                case "Color Correction":
                    ti.Type = "color_correction";
                    maxRGBA = RedHaloTools.GetValueByID<IAColor>(tex, 0, 0);
                    ti.Properties.Add("color", new[] { maxRGB.R, maxRGB.G, maxRGB.B, 1 });

                    // hue value : [-180, 180] / 360 = [-0.5, 0.5] + 0.5 = [0, 1]
                    ti.Properties.Add("hue", RedHaloTools.GetValueByID<float>(tex, 0, 7) / 360 + 0.5f);

                    // saturation : [-180, 180] /360 = [-0.5, 0.5] + 0.5 = [0, 1]                   
                    ti.Properties.Add("saturation", RedHaloTools.GetValueByID<float>(tex, 0, 8) / 360 + 0.5f);

                    // Color Mode
                    var clrMode = RedHaloTools.GetValueByID<int>(tex, 0, 2);
                    switch (clrMode)
                    {
                        case 1:
                            ti.Properties.Add("color_mode", "MONO");
                            break;
                        case 2:
                            ti.Properties.Add("color_mode", "INVERT");
                            break;
                        default:
                            ti.Properties.Add("color_mode", "NORMAL");
                            break;
                    }

                    // lightnessMode
                    var ccMode = RedHaloTools.GetValueByID<int>(tex, 0, 11);
                    if (ccMode == 0)
                    {
                        ti.Properties.Add("gamma", 1.0);

                        // brightness value : -100 <-> 100
                        ti.Properties.Add("brightness", RedHaloTools.GetValueByID<float>(tex, 0, 13) / 100);

                        // contrast value : -100 <-> 100
                        ti.Properties.Add("contrast", RedHaloTools.GetValueByID<float>(tex, 0, 12) / 100);
                    }
                    else
                    {
                        ti.Properties.Add("gamma", RedHaloTools.GetValueByID<float>(tex, 0, 22));
                        ti.Properties.Add("brightness", 0);
                        ti.Properties.Add("contrast", 0);
                    }
                    // Texmap
                    var ccTexmap = RedHaloTools.GetValueByID<ITexmap>(tex, 0, 1);
                    AddSubtexmap("texmap", ccTexmap);

                    break;
                
                case "RGB Multiply":
                    ti.Type = "mix";
                    //color1
                    maxRGB = RedHaloTools.GetValueByID<IColor>(tex, 0, 0);
                    ti.Properties.Add("color1", new[] { maxRGB.R, maxRGB.G, maxRGB.B, 1 });

                    // color2
                    maxRGB = RedHaloTools.GetValueByID<IColor>(tex, 0, 1);
                    ti.Properties.Add("color2", new[] { maxRGB.R, maxRGB.G, maxRGB.B, 1 });

                    // mix mode
                    ti.Properties.Add("mode", "MULTIPLY");

                    ti.Properties.Add("multiplier", 1);

                    // map1
                    var RgbMult_map1 = RedHaloTools.GetValueByID<ITexmap>(tex, 0, 2);
                    AddSubtexmap("map1", RgbMult_map1);

                    // map2
                    var RgbMult_map2 = RedHaloTools.GetValueByID<ITexmap>(tex, 0, 2);
                    AddSubtexmap("map2", RgbMult_map2);

                    // alphafrom
                    ti.Properties.Add("alpha_from", RedHaloTools.GetValueByID<int>(tex, 0, 6));

                    break;

                case "Gradient":
                    maxRGB = RedHaloTools.GetValueByID<IColor>(tex, 0, 0);
                    ti.Properties.Add("color1", new[] { maxRGB.R, maxRGB.G, maxRGB.B, 1 });

                    maxRGB = RedHaloTools.GetValueByID<IColor>(tex, 0, 1);
                    ti.Properties.Add("color2", new[] { maxRGB.R, maxRGB.G, maxRGB.B, 1 });

                    maxRGB = RedHaloTools.GetValueByID<IColor>(tex, 0, 2);
                    ti.Properties.Add("color3", new[] { maxRGB.R, maxRGB.G, maxRGB.B, 1 });

                    ti.Properties.Add("color2_pos", RedHaloTools.GetValueByID<float>(tex, 0, 9));

                    // Gradient type
                    // 0: Linear
                    // 1: Radial
                    var gradientType = RedHaloTools.GetValueByID<int>(tex, 0, 10);
                    switch (gradientType)
                    {
                        case 0:
                            ti.Properties.Add("type", "LINEAR");
                            break;
                        case 1:
                            ti.Properties.Add("type", "SPHERICAL");
                            break;
                        default:
                            break;
                    }

                    break;

                case "Color Map":
                    ti.Type = "color_map";
                    // Solid Color
                    maxRGBA = RedHaloTools.GetValueByID<IAColor>(tex, 0, 0);
                    ti.Properties.Add("color", new[] { maxRGB.R, maxRGB.G, maxRGB.B, 1 });

                    ti.Properties.Add("gamma", RedHaloTools.GetValueByID<float>(tex, 0, 3));
                    ti.Properties.Add("reverse_gamma", RedHaloTools.GetValueByID<int>(tex, 0, 5));

                    // Color Map
                    var colormap_texmap = RedHaloTools.GetValueByID<ITexmap>(tex, 0, 1);
                    var colormap_texmap_enable = RedHaloTools.GetValueByID<int>(tex, 0, 2) == 1;
                    if (colormap_texmap_enable)
                    {
                        AddSubtexmap("texmap", colormap_texmap);
                    }

                    break;
               
                case "Vertex Color":
                    ti.Type = "vertex_color";
                    /*
                     * 0-0	Int	map
                     * 0-1	Int	subid                     
                     */
                    ti.Properties.Add("map", RedHaloTools.GetValueByID<int>(tex, 0, 0));
                    break;

                #endregion

                #region VRay Textures
                // VRay Textures
                case "VRayDirt":
                    ti.Type = "ao";
                    // occuluded color
                    maxRGB = RedHaloTools.GetValueByID<IColor>(tex, 0, 2);
                    ti.Properties.Add("occluded_color", new[] { maxRGB.R, maxRGB.G, maxRGB.B, 1 });

                    // unoccluded color
                    maxRGB = RedHaloTools.GetValueByID<IColor>(tex, 0, 4);
                    ti.Properties.Add("unoccluded_color", new[] { maxRGB.R, maxRGB.G, maxRGB.B, 1 });

                    
                    // radius
                    ti.Properties.Add("radius", RedHaloTools.GetValueByID<float>(tex, 0, 0));

                    // subdivs
                    ti.Properties.Add("subdivs", RedHaloTools.GetValueByID<int>(tex, 0, 8));

                    // mode
                    // 0:OUTSIDE 1: 2: 3: 4:INSIDE
                    var vr_mode = RedHaloTools.GetValueByID<int>(tex,0, 23);
                    switch (vr_mode)
                    {
                        case 0:
                            ti.Properties.Add("mode", "OUTSIDE");
                            break;
                        case 4:
                            ti.Properties.Add("mode", "INSIDE");
                            break;
                        default:
                            ti.Properties.Add("mode", "BOTH");
                            break;
                    }
                    // only_sameobject
                    ti.Properties.Add("only_sameobject", RedHaloTools.GetValueByID<int>(tex, 0, 17));

                    // texmap_radius
                    var vrao_texmap_radius = RedHaloTools.GetValueByID<ITexmap>(tex, 0, 1);
                    if(vrao_texmap_radius != null && RedHaloTools.GetValueByID<int>(tex, 0, 34) == 1)
                    {
                        ti.subTexmapInfo.Add("texmap_radius", ExportTexmap(vrao_texmap_radius));
                    }

                    // texmap_occluded
                    var vrao_texmap_occluded = RedHaloTools.GetValueByID<ITexmap>(tex, 0, 3);
                    if(vrao_texmap_occluded != null && RedHaloTools.GetValueByID<int>(tex, 0, 37) == 1)
                    {
                        ti.subTexmapInfo.Add("texmap_occluded", ExportTexmap(vrao_texmap_occluded));
                    }

                    // texmap_unoccluded
                    var vrao_texmap_unoccluded = RedHaloTools.GetValueByID<ITexmap>(tex, 0, 5);
                    if (vrao_texmap_unoccluded != null && RedHaloTools.GetValueByID<int>(tex, 0, 40) == 1)
                    {
                        ti.subTexmapInfo.Add("texmap_unoccluded", ExportTexmap(vrao_texmap_unoccluded));
                    }

                    break;

                case "VRayNormalMap":
                    ti.Type = "normal_map";
                    ti.Properties.Add("normal_strength", RedHaloTools.GetValueByID<float>(tex, 0, 2));
                    ti.Properties.Add("bump_strength", RedHaloTools.GetValueByID<float>(tex, 0, 5));
                    ti.Properties.Add("channel", RedHaloTools.GetValueByID<int>(tex, 0, 6));

                    ti.Properties.Add("flip_red", RedHaloTools.GetValueByID<int>(tex, 0, 7));
                    ti.Properties.Add("flip_green", RedHaloTools.GetValueByID<int>(tex, 0, 8));
                    ti.Properties.Add("swap_red_green", RedHaloTools.GetValueByID<int>(tex, 0, 9));

                    ti.Properties.Add("map_rotation", RedHaloTools.GetValueByID<float>(tex, 0, 10));

                    // normal map
                    var vr_normalmap = RedHaloTools.GetValueByID<ITexmap>(tex, 0, 0);
                    if (vr_normalmap != null && RedHaloTools.GetValueByID<int>(tex, 0, 1) == 1)
                    {
                        ti.subTexmapInfo.Add("normal_map", ExportTexmap(vr_normalmap));
                    }

                    // bump map
                    var vr_bumpmap = RedHaloTools.GetValueByID<ITexmap>(tex, 0, 3);
                    if (vr_bumpmap != null && RedHaloTools.GetValueByID<int>(tex, 0, 4) == 1)
                    {
                        ti.subTexmapInfo.Add("bump_map", ExportTexmap(vr_bumpmap));
                    }

                    break;

                case "VRayColor2Bump":
                    ti.Type = "normal_map";
                    // height
                    ti.Properties.Add("bump_strength", RedHaloTools.GetValueByID<float>(tex, 0, 1));

                    // scale
                    ti.Properties.Add("scale", RedHaloTools.GetValueByID<float>(tex, 0, 2));

                    // map
                    var vr_color2bump = RedHaloTools.GetValueByID<ITexmap>(tex, 0, 0);
                    if (vr_color2bump != null)
                    {
                        ti.subTexmapInfo.Add("bump_map", ExportTexmap(vr_color2bump));
                    }

                    break;

                case "VRayBump2Normal":
                    ti.Type = "normal_map";
                    // bump map mult
                    ti.Properties.Add("bump_strength", RedHaloTools.GetValueByID<float>(tex, 0, 1)/100.0f);

                    // mode
                    // 0:Tangent Space 1:Local XYZ 2:Screen Sapce 3:World Space
                    ti.Properties.Add("mode", RedHaloTools.GetValueByID<int>(tex, 0, 2));

                    // channel
                    ti.Properties.Add("channel", RedHaloTools.GetValueByID<int>(tex, 0, 3));

                    // map
                    var vr_bump2normal = RedHaloTools.GetValueByID<ITexmap>(tex, 0, 0);
                    if (vr_bump2normal != null)
                    {
                        ti.subTexmapInfo.Add("bump_map", ExportTexmap(vr_bump2normal));
                    }
                    break;

                case "VRayBitmap":
                    ti.Type = "bitmap";
                    // 处理图片，如果Apply启用，且Width或Height小于1，渲染一张新图片，并启用此图片
                    croppintApply = RedHaloTools.GetValueByID<int>(tex, 0, 18);
                    
                    ClipU = RedHaloTools.GetValueByID<float>(tex, 0, 20);
                    ClipV = RedHaloTools.GetValueByID<float>(tex, 0, 21);
                    ClipH = RedHaloTools.GetValueByID<float>(tex, 0, 23);
                    ClipW = RedHaloTools.GetValueByID<float>(tex, 0, 22);

                    _filename = RedHaloTools.GetValueByID<string>(tex, 0, 0);
                    
                    new_bm = null;                    

                    // 如果开启了裁切，重新渲染一张新图
                    if (croppintApply == 1 && ((ClipH < 1) || (ClipW < 1)))
                    {
                        string filepath = Path.GetDirectoryName(_filename);
                        string filename = Path.GetFileNameWithoutExtension(_filename);
                        string fileext = Path.GetExtension(_filename);

                        // 渲染新贴图
                        var bmInfo = RedHaloTools.GetValueByID<IPBBitmap>(tex, 0, 13);

                        float bm_width = bmInfo.Bi.Width * ClipW > 2 ? bmInfo.Bi.Width * ClipW : 2;
                        float bm_height = bmInfo.Bi.Height * ClipH > 2 ? bmInfo.Bi.Height * ClipH : 2;

                        _filename = Path.Combine(filepath, $"{filename}_001{fileext}");
                        //new_bm = CreateBitmap(tex, (ushort)bm_width, (ushort)bm_height, _filename);
                    }

                    stdUVGen = RedHaloTools.GetValueByID<IReferenceTarget>(tex, 0, 29) as IStdUVGen;
                    uvgen = RedHaloTools.GetValueByID<IReferenceTarget>(tex, 0, 29) as IUVGen;                                       

                    ti.Properties.Add("filename", _filename);

                    ti.Properties.Add("clipu", ClipU);
                    ti.Properties.Add("clipv", ClipV);
                    ti.Properties.Add("clipw", ClipW);
                    ti.Properties.Add("cliph", ClipH);

                    AddImageWrapProperty(ti, stdUVGen);
                    AddUvTransformProperties(ti, stdUVGen);                                       

                    ti.Properties.Add("alpha_source", RedHaloTools.GetValueByID<int>(tex, 0, 26));                    

                    // 贴图类型：纹理或环境
                    // 0:Angular 1:Cubic 2:Spherical 3:Mirror Ball 4:3ds Max standard
                    var environType = RedHaloTools.GetValueByID<int>(tex, 0, 3);
                    if (environType == 4)
                    {
                        ti.Properties.Add("mapping_type", "TEXTURE");

                        // 3ds standard texture
                        AddStandardMappingProperties(ti, stdUVGen, uvgen);
                    }
                    else
                    {
                        ti.Properties.Add("mapping_type", "ENVIRON");

                        AddVRayEnvironmentMappingProperties(ti, environType);
                    }

                    ti.Properties.Add("mono_output", RedHaloTools.GetValueByID<int>(tex, 0, 25));
                    ti.Properties.Add("rgb_output", RedHaloTools.GetValueByID<int>(tex, 0, 24));
                    //ti.Properties.Add("premult_alpha", $"{RedHaloTools.GetValueByID<int>(tex, 0, 12)}");

                    // Gain
                    float overallMult = RedHaloTools.GetValueByID<float>(tex, 0, 1);
                    float renderMult = RedHaloTools.GetValueByID<float>(tex, 0, 2);
                    ti.Properties.Add("gain", overallMult * renderMult);
                    
                    break;

                case "VRayColor":
                    ti.Type = "color_map";
                    // Color / Temp
                    // 如果是色温，转成颜色
                    var vrcolor_mode = RedHaloTools.GetValueByID<int>(tex, 0, 0);
                    var temperature = 6500f;
                    if ( vrcolor_mode == 0)
                    {
                        temperature = RedHaloTools.GetValueByID<float>(tex, 0, 1);
                        maxRGB = RedHaloTools.GetRgbFromKelvin(temperature);
                    }
                    else
                    {
                        var clr_r = RedHaloTools.GetValueByID<float>(tex, 0, 2);
                        var clr_g = RedHaloTools.GetValueByID<float>(tex, 0, 3);
                        var clr_b = RedHaloTools.GetValueByID<float>(tex, 0, 4);
                        maxRGB = RedHaloCore.Global.Color.Create(clr_r, clr_g, clr_b);
                    }
                    ti.Properties.Add("color", new[] { maxRGB.R, maxRGB.G, maxRGB.B, 1 });

                    // gamma
                    ti.Properties.Add("gamma", RedHaloTools.GetValueByID<float>(tex, 0, 8));

                    break;

                case "VRayCompTex":
                    ti.Type = "mix";
                    // Source A
                    var vr_comptex_a = RedHaloTools.GetValueByID<ITexmap>(tex, 0, 0);
                    if ( vr_comptex_a != null)
                    {
                        ti.subTexmapInfo.Add("map1", ExportTexmap(vr_comptex_a));
                    }

                    // Source B
                    var vr_comptex_b = RedHaloTools.GetValueByID<ITexmap>(tex, 0, 1);
                    if ( vr_comptex_b != null)
                    {
                        ti.subTexmapInfo.Add("map2", ExportTexmap(vr_comptex_b));
                    }

                    // Operator
                    var MixModeNames = new string[] { "ADD", "SUBTRACT", "DIFFERENCE", "MULTIPLY", "DIVIDE", "MINIMUM", "MAXIMUM", "COLORSHIFT", "COLORTINT", "BENDALPHASTRAIGHT", "BLENDAPLHAPREMULIPLIED" };                    
                    ti.Properties.Add("mode", $"{MixModeNames[RedHaloTools.GetValueByID<int>(tex, 0, 2)]}");

                    // multiplier
                    ti.Properties.Add("multiplier", RedHaloTools.GetValueByID<float>(tex, 0, 3));
                    break;

                #endregion

                #region Corona Textures
                // Corona Textures
                case "CoronaAO":
                    ti.Type = "ao";
                    // occuluded color
                    maxRGB = RedHaloTools.GetValueByID<IColor>(tex, 0, 0);
                    ti.Properties.Add("occluded_color", new[] { maxRGB.R, maxRGB.G, maxRGB.B, 1 });

                    // unoccluded color
                    maxRGB = RedHaloTools.GetValueByID<IColor>(tex, 0, 1);
                    ti.Properties.Add("unoccluded_color", new[] { maxRGB.R, maxRGB.G, maxRGB.B, 1 });

                    // radius 场景尺寸
                    ti.Properties.Add("radius", RedHaloTools.GetValueByID<float>(tex, 0, 3));

                    // subdivs
                    ti.Properties.Add("subdivs", RedHaloTools.GetValueByID<int>(tex, 0, 2));

                    // mode
                    // 0:OUTSIDE 1: 2: 3: 4:INSIDE
                    var cr_mode = RedHaloTools.GetValueByID<int>(tex, 0, 17);
                    switch (cr_mode)
                    {
                        case 0:
                            ti.Properties.Add("mode", "OUTSIDE");
                            break;
                        case 1:
                            ti.Properties.Add("mode", "INSIDE");
                            break;
                        default:
                            ti.Properties.Add("mode", "BOTH");
                            break;
                    }
                    // only_sameobject
                    var exeludeMode = RedHaloTools.GetValueByID<int>(tex, 0, 12);
                    if (exeludeMode == 3)
                    {
                        ti.Properties.Add("only_sameobject", "1");
                    }
                    else
                    {
                        ti.Properties.Add("only_sameobject", "0");
                    }
                    

                    // texmap_radius
                    var crao_texmap_radius = RedHaloTools.GetValueByID<ITexmap>(tex, 0, 9);
                    if (crao_texmap_radius != null && RedHaloTools.GetValueByID<int>(tex, 0, 15) == 1)
                    {
                        ti.subTexmapInfo.Add("texmap_radius", ExportTexmap(crao_texmap_radius));
                    }

                    // texmap_occluded
                    var crao_texmap_occluded = RedHaloTools.GetValueByID<ITexmap>(tex, 0, 7);
                    if (crao_texmap_occluded != null && RedHaloTools.GetValueByID<int>(tex, 0, 13) == 1)
                    {
                        ti.subTexmapInfo.Add("texmap_occluded", ExportTexmap(crao_texmap_occluded));
                    }

                    // texmap_unoccluded
                    var crao_texmap_unoccluded = RedHaloTools.GetValueByID<ITexmap>(tex, 0, 8);
                    if (crao_texmap_unoccluded != null && RedHaloTools.GetValueByID<int>(tex, 0, 14) == 1)
                    {
                        ti.subTexmapInfo.Add("texmap_unoccluded", ExportTexmap(crao_texmap_unoccluded));
                    }
                    break;

                case "CoronaMix":      
                    ti.Type = "mix";
                    //color1
                    maxRGB = RedHaloTools.GetValueByID<IColor>(tex, (short)0, 10);
                    ti.Properties.Add("color1", new[] { maxRGB.R, maxRGB.G, maxRGB.B, 1 });

                    //color2
                    maxRGB = RedHaloTools.GetValueByID<IColor>(tex, (short)0, 9);
                    ti.Properties.Add("color2", new[] { maxRGB.R, maxRGB.G, maxRGB.B, 1 });

                    // mix amnout
                    amount = RedHaloTools.GetValueByID<float>(tex, 0, 1);
                    ti.Properties.Add("multiplier", amount);

                    // mix mode
                    var CoronaMixMode = new string[] { "ADD", "SUBTRACT", "MULTIPLY", "DIVIDE", "MINIMUM", "MAXIMUM", "MIX", "GAMMA", "DIFFERENCE", "SCREEN", "OVERLAY", "DODGE", "BURN", "LINEARBURN", "LINEAR_LIGHT", "SOFT_LIGHT", "HARDLIGHT", "VIVIDLIGHT", "PINLIGHT", "HARDMIX", "EXCLUSION" };
                    ti.Properties.Add("mode", CoronaMixMode[RedHaloTools.GetValueByID<int>(tex, 0, 0)]);

                    //map1
                    var cr_mix_map1 = RedHaloTools.GetValueByID<ITexmap>(tex, 0, 6);
                    if(cr_mix_map1 != null && RedHaloTools.GetValueByID<int>(tex, 0, 12) == 1)
                    {
                        ti.subTexmapInfo.Add("map1", ExportTexmap(cr_mix_map1));
                    }

                    // map2
                    var cr_mix_map2 = RedHaloTools.GetValueByID<ITexmap>(tex, 0, 7);
                    if(cr_mix_map2 != null && RedHaloTools.GetValueByID<int>(tex, 0, 11) == 1)
                    {
                        ti.subTexmapInfo.Add("map2", ExportTexmap(cr_mix_map2));
                    }

                    // mix mask
                    var cr_mix_mask = RedHaloTools.GetValueByID<ITexmap>(tex, 0, 8);
                    if(cr_mix_mask != null && RedHaloTools.GetValueByID<int>(tex, 0, 13) == 1)
                    {
                        ti.subTexmapInfo.Add("mask", ExportTexmap(cr_mix_mask));
                    }

                    break;

                case "CoronaColorCorrect":
                    ti.Type = "color_correction";

                    ti.Properties.Add("color", new[] { 0.8, 0.8, 0.8, 1 });

                    ti.Properties.Add("hue", RedHaloTools.GetValueByID<float>(tex, 0, 23));
                    ti.Properties.Add("saturation", RedHaloTools.GetValueByID<float>(tex, 0, 4));

                    // Color Mode
                    var clrInvert = RedHaloTools.GetValueByID<int>(tex, 0, 9);
                    if(clrInvert == 1)
                    {
                        ti.Properties.Add("color_mode", "INVERT");
                    }
                    else
                    {
                        ti.Properties.Add("color_mode", "NORMAL");
                    }

                    ti.Properties.Add("gamma", RedHaloTools.GetValueByID<float>(tex, 0, 12));
                    ti.Properties.Add("brightness", RedHaloTools.GetValueByID<float>(tex, 0, 2));
                    ti.Properties.Add("contrast", RedHaloTools.GetValueByID<float>(tex, 0, 3));

                    // Texmap
                    var cr_ccTexmap = RedHaloTools.GetValueByID<ITexmap>(tex, 0, 1);
                    if (cr_ccTexmap != null)
                    {
                        ti.subTexmapInfo.Add("texmap", ExportTexmap(cr_ccTexmap));
                    }

                    break;

                case "CoronaNormal":
                    ti.Type = "normal_map";

                    ti.Properties.Add("normal_strength", RedHaloTools.GetValueByID<float>(tex, 0, 0));
                    ti.Properties.Add("bump_strength", RedHaloTools.GetValueByID<float>(tex, 0, 9));
                    ti.Properties.Add("map_channel", "1");

                    ti.Properties.Add("flip_red", RedHaloTools.GetValueByID<int>(tex, 0, 3));
                    ti.Properties.Add("flip_green", RedHaloTools.GetValueByID<int>(tex, 0, 4));
                    ti.Properties.Add("swap_red_green", RedHaloTools.GetValueByID<int>(tex, 0, 5));

                    ti.Properties.Add("map_rotation", "0");

                    // normal map
                    var cr_normalmap = RedHaloTools.GetValueByID<ITexmap>(tex, 0, 1);
                    if (cr_normalmap != null)
                    {
                        ti.subTexmapInfo.Add("normal_map", ExportTexmap(cr_normalmap));
                    }

                    // bump map
                    var cr_bumpmap = RedHaloTools.GetValueByID<ITexmap>(tex, 0, 7);
                    if (cr_bumpmap != null && RedHaloTools.GetValueByID<int>(tex, 0, 8) == 1)
                    {
                        ti.subTexmapInfo.Add("bump_map", ExportTexmap(cr_bumpmap));
                    }
                    break;

                case "CoronaBumpConverter":
                    ti.Type = "normal_map";
                    var cr_bump_converter_tex = RedHaloTools.GetValueByID<ITexmap>(tex, 0, 0);
                    
                    if(cr_bump_converter_tex != null)
                    {
                        ti.subTexmapInfo.Add("bump_map", ExportTexmap(cr_bump_converter_tex));
                        ti.Properties.Add("bump_strength", RedHaloTools.GetValueByID<float>(tex, 0, 1));
                    }
                    
                    break;

                case "CoronaFrontBack":
                    ti.Type = "corona_front_back";

                    // Color1
                    maxRGB = RedHaloTools.GetValueByID<IColor>(tex, 0, 0);
                    ti.Properties.Add("front_color", new[] { maxRGB.R, maxRGB.G, maxRGB.B, 1 });
                    
                    // Color2
                    maxRGB = RedHaloTools.GetValueByID<IColor>(tex, 0, 1);
                    ti.Properties.Add("back_color", new[] { maxRGB.R, maxRGB.G, maxRGB.B, 1 });

                    // map1
                    var cr_front_tex = RedHaloTools.GetValueByID<ITexmap>(tex, 0, 4);
                    if(cr_front_tex != null && RedHaloTools.GetValueByID<int>(tex, 0, 2) == 1)
                    {
                        ti.subTexmapInfo.Add("front_texmap", ExportTexmap(cr_front_tex));
                    }
                    
                    // map2
                    var cr_back_tex = RedHaloTools.GetValueByID<ITexmap>(tex, 0, 5);
                    if(cr_back_tex != null && RedHaloTools.GetValueByID<int>(tex, 0, 3) == 1)
                    {
                        ti.subTexmapInfo.Add("back_texmap", ExportTexmap(cr_back_tex));
                    }

                    break;

                case "CoronaColor":
                    ti.Type = "color_map";
                    // color
                    // gamma
                    // reverse gamma
                    
                    // method 0:color, 1:colorHDR, 2:temperature, 3:hexcolor
                    var method = RedHaloTools.GetValueByID<int>(tex, 0, 5);
                    switch (method)
                    {
                        case 0: // color
                            maxRGB = RedHaloTools.GetValueByID<IColor>(tex, 0, 0);
                            break;
                        case 1: // colorHDR
                            var hdrcolor = RedHaloTools.GetValueByID<IPoint3>(tex, 0, 1);
                            maxRGB = RedHaloCore.Global.Color.Create(hdrcolor.X, hdrcolor.Y, hdrcolor.Z);
                            break; 
                        case 2: // temperature
                            temperature = RedHaloTools.GetValueByID<float>(tex, 0, 4); 
                            maxRGB = RedHaloTools.GetRgbFromKelvin(temperature);
                            break;
                        case 3: // hex color
                            var hexcolor = RedHaloTools.GetValueByID<string>(tex, 0, 6);
                            var clr = (Color)ColorConverter.ConvertFromString(hexcolor);
                            maxRGB = RedHaloCore.Global.Color.Create(clr.R / 255.0f, clr.G / 255.0f, clr.B / 255.0f);
                            break;
                        default:
                            break;
                    }
                    ti.Properties.Add("color", new[] { maxRGB.R, maxRGB.G, maxRGB.B, 1 });

                    break;
                
                case "CoronaBitmap":
                    ti.Type = "bitmap";
                    
                    _filename = RedHaloTools.GetValueByID<string>(tex, 0, 0);
                    _filename = RedHaloTools.GetActualPath(_filename);

                    // 处理图片，如果Apply启用，且Width或Height小于1，渲染一张新图片，并启用此图片
                    croppintApply = RedHaloTools.GetValueByID<int>(tex, 0, 19);

                    ClipU = RedHaloTools.GetValueByID<float>(tex, 0, 15);
                    ClipV = RedHaloTools.GetValueByID<float>(tex, 0, 16);
                    ClipH = RedHaloTools.GetValueByID<float>(tex, 0, 18);
                    ClipW = RedHaloTools.GetValueByID<float>(tex, 0, 17);

                    // UVW offset
                    var uvwOffset = RedHaloTools.GetValueByID<IPoint3>(tex, 0, 6);

                    // UVW Tiling
                    var uvwScale = RedHaloTools.GetValueByID<IPoint3>(tex, 0, 5);

                    // Tiling model
                    // 0: None 1: Repeat 2:Mirror 3: Clamp
                    var u_tiling_mode = RedHaloTools.GetValueByID<int>(tex, 0, 8);
                    var v_tiling_mode = RedHaloTools.GetValueByID<int>(tex, 0, 9);
                    
                    string _imgWrap = "REPEAT";

                    if(u_tiling_mode == 0 || v_tiling_mode == 0)
                    {
                        _imgWrap = "CLIP";
                    }

                    if (u_tiling_mode == 2 || v_tiling_mode == 2)
                    {
                        _imgWrap = "MIRROR";
                    }

                    if (u_tiling_mode == 3 || v_tiling_mode == 3)
                    {
                        _imgWrap = "EXTEND";
                    }
        
                    ti.Properties.Add("filename", _filename);

                    ti.Properties.Add("clipu", ClipU);
                    ti.Properties.Add("clipv", ClipV);
                    ti.Properties.Add("clipw", ClipW);
                    ti.Properties.Add("cliph", ClipH);                    

                    ti.Properties.Add("image_wrap", _imgWrap);                    
                     
                    ti.Properties.Add("u_scale", uvwScale.X);
                    ti.Properties.Add("v_scale", uvwScale.Y);
                    ti.Properties.Add("u_offset", uvwOffset.X);
                    ti.Properties.Add("v_offset", uvwOffset.Y);
                    
                    ti.Properties.Add("w_angle", RedHaloTools.GetValueByID<float>(tex, 0, 25));
                    ti.Properties.Add("v_angle", 0);
                    ti.Properties.Add("u_angle", RedHaloTools.GetValueByID<float>(tex, 0, 26));
                    
                    // 贴图类型：纹理或环境
                    //ti.Properties.Add("mapping_type", RedHaloTools.GetValueByID<int>(tex, 0, 3));
                    ti.Properties.Add("mapping_type", "ENVIRON");

                    // enviroMapping
                    // 0:Spherical 1:Screen 2:Dome 3:Cross 4:Mirror Ball
                    var enviroMapping = RedHaloTools.GetValueByID<int>(tex, 0, 3);
                    
                    // 贴图方式
                    switch (enviroMapping)
                    {
                        case 0:
                            ti.Properties.Add("mapping_uvw", "SPHERICAL");
                            break;
                        case 1:
                            ti.Properties.Add("mapping_uvw", "SCREEN");
                            break;
                        case 2:
                            ti.Properties.Add("mapping_uvw", "DOME");
                            break;
                        case 3:
                            ti.Properties.Add("mapping_uvw", "CROSS");
                            break;
                        case 4:
                            ti.Properties.Add("mapping_uvw", "MIRROR_BALL");
                            break;
                        default:
                            ti.Properties.Add("mapping_uvw", "SPHERICAL");
                            break;
                    }

                    // alpha source
                    // 0:Image Alpha 1:RGB Intensity 2:None(Opaque)
                    ti.Properties.Add("alpha_source", RedHaloTools.GetValueByID<int>(tex, 0, 12));

                    // 0:RGB Intensity 1:Image Alpha
                    ti.Properties.Add("mono_output", RedHaloTools.GetValueByID<int>(tex, 0, 13));
                    
                    // 0:RGB 1:Image Alpha as gray
                    ti.Properties.Add("rgb_output", RedHaloTools.GetValueByID<int>(tex, 0, 14));
                    //ti.Properties.Add("premult_alpha", $"{RedHaloTools.GetValueByID<int>(tex, 0, 12)}");

                    // Gain
                    //float overallMult = RedHaloTools.GetValueByID<float>(tex, 0, 1);
                    //float renderMult = RedHaloTools.GetValueByID<float>(tex, 0, 2);
                    //ti.Properties.Add("gain", $"{overallMult * renderMult}");
                    
                    break;

                case "CoronaRoundEdges": // Blender Bevel node
                    ti.Type = "bevel";
                    
                    // radius
                    var radius = RedHaloTools.GetValueByID<float>(tex, 0, 2); // World Space Radius
                    var sample = RedHaloTools.GetValueByID<int>(tex, 0, 3); // Sample
                    var mapStrengthAdditional = RedHaloTools.GetValueByID<float>(tex, 0, 6) / 1000f; // Map Strength Additional [0-999]

                    ti.Properties.Add("radius", radius);
                    ti.Properties.Add("sample", sample);
                    ti.Properties.Add("map_strength_additional", mapStrengthAdditional);

                    var mapRadius = RedHaloTools.GetValueByID<ITexmap>(tex, 0, 0); // Map Radius
                    var mapRadiusOn = RedHaloTools.GetValueByID<int>(tex, 0, 4) == 1; // Map Radius On
                    if (mapRadius != null && mapRadiusOn)
                    {
                        ti.subTexmapInfo.Add("radius_texmap", ExportTexmap(mapRadius));
                    }

                    var mapAdditionalBump = RedHaloTools.GetValueByID<ITexmap>(tex, 0, 1); // Map Additional Bump
                    var mapAdditionalBumpOn = RedHaloTools.GetValueByID<int>(tex, 0, 5) == 1; // Map Additional Bump On
                    if (mapAdditionalBump != null && mapAdditionalBumpOn)
                    {
                        ti.subTexmapInfo.Add("additional_bump_texmap", ExportTexmap(mapAdditionalBump));
                    }

                    break;

                #endregion

                case "Composite":
                    ti.Type = "composite";
                    /*
                     * ParamBlocks paramID
                     * 1: mapEnabled
                     * 3: MaskEnabled
                     * 5: BlendMode
                     * 6: layerName
                     * 8: opacity
                     * 9: mapList
                     * 10: maskList                     
                     */

                    // MapList -- Layer List
                    var pb2 = tex.GetParamBlockByID(0);                    

                    var maplist = pb2.GetParamDefByIndex(9);

                    ti.Properties.Add("count", pb2.Count(9));
                    for (int k = 0; k < pb2.Count(9); k++)
                    {                        
                        Dictionary<string, dynamic> compositionLayer = new Dictionary<string, dynamic>();
                        // mapsList
                        // ---enabled
                        compositionLayer.Add("enabled", pb2.GetInt(1, 0, k) == 1);

                        // ---opacity
                        compositionLayer.Add("opacity", pb2.GetFloat(8, 0, k)/100);

                        // Blend Mode
                        /*
                         * Blend mode list
                         * https://help.autodesk.com/view/MAXDEV/2024/ENU/?guid=GUID-611E1342-F976-4E95-8F78-88175B329745
                         */
                        var blendMode = new[] {
                            "Mix", "Average", "Add", "Subtract", "Darken", "Multiply", "Burn", "Linear_Burn", "Lighten", "Screen", "Dodge", "Linear Dodge", 
                            "Spotlight", "Spotlight_Blend", "Overlay", "Soft_Light", "Hard_Light", "Pin Light", "Hard_Mix", "Difference", "Exclusion", "Hue", "Saturation", "color", "Value" };
                        
                        var blendMode_index = pb2.GetInt(5, 0, k);
                        compositionLayer.Add("blend_mode", blendMode[blendMode_index].ToUpper());
                        
                        // ---map tex
                        tex = pb2.GetTexmap(9, 0, k);
                        if(tex != null)
                        {
                            compositionLayer.Add("map", MaterialUtils.ExportTexmap(tex));
                        }

                        // ---mask tex
                        tex = pb2.GetTexmap(10, 0, k);
                        if(tex != null)
                        {
                            compositionLayer.Add("mask_map", MaterialUtils.ExportTexmap(tex));
                        }
                        else
                        {
                            compositionLayer.Add("mask_map", null);
                        }

                        // ---mask enabled
                        compositionLayer.Add("mask_enabled", pb2.GetInt(3, 0, k) == 1);
                        
                        ti.Properties.Add($"layer_{k + 1}", compositionLayer);
                    }
                  
                    break;
                
                default:
                    break;
            }
            return ti;
        }
    }
}