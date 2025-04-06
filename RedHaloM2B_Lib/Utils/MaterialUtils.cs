using Autodesk.Max;
using RedHaloM2B.Textures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Windows.Input;
using Autodesk.Max.Plugins;

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

                //if (materialSet.Add(mtl))
                //{
                //    uniqueMaterials.Add(mtl);
                //}
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

            //foreach (var m in uniqueMaterials)
            //{
            //    // 如果名字已经存在，计数器加1
            //    if (nameCounts.ContainsKey(m.Name))
            //    {
            //        nameCounts[m.Name]++;
            //        m.Name = $"{m.Name}_{nameCounts[m.Name]}";
            //    }
            //    else
            //    {
            //        nameCounts[m.Name] = 0;
            //    }
            //}

            return uniqueMaterials;
            //materialSet.Concat(uniqueMaterials).ToHashSet();
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
            Debug.Print($"======{texType.ToUpper()}======");
            
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
                    int croppintApply = RedHaloTools.GetValeByID<int>(tex, 0, 6);
                    float ClipH = RedHaloTools.GetValeByID<float>(tex, 0, 3);
                    float ClipU = RedHaloTools.GetValeByID<float>(tex, 0, 0);
                    float ClipV = RedHaloTools.GetValeByID<float>(tex, 0, 1);
                    float ClipW = RedHaloTools.GetValeByID<float>(tex, 0, 2);

                    IBitmap new_bm = null;
                    string _filename = RedHaloTools.GetActualPath(RedHaloTools.GetValeByID<string>(tex, 0, 16));

                    // 如果开启了裁切，重新渲染一张新图
                    if (croppintApply == 1 && ((ClipH < 1) || (ClipW < 1)))
                    {
                        string orig_filename = RedHaloTools.GetValeByID<string>(tex, 0, 16);
                        string filepath = Path.GetDirectoryName(orig_filename);
                        string filename = Path.GetFileNameWithoutExtension(orig_filename);
                        string fileext = Path.GetExtension(orig_filename);

                        // 渲染新贴图
                        var bmInfo = RedHaloTools.GetValeByID<IPBBitmap>(tex, 0, 13);

                        var bm_width = bmInfo.Bi.Width * ClipW > 2 ? bmInfo.Bi.Width * ClipW : 2;
                        var bm_height = bmInfo.Bi.Height * ClipH > 2 ? bmInfo.Bi.Height * ClipH : 2;

                        _filename = Path.Combine(filepath, $"{filename}_001{fileext}");
                        //new_bm = CreateBitmap(tex, (ushort)bm_width, (ushort)bm_height, _filename);
                    }

                    IStdUVGen stdUVGen = RedHaloTools.GetValeByID<IReferenceTarget>(tex, 0, 14) as IStdUVGen;

                    //ImageWrap
                    // Tiling
                    bool Wrap = Convert.ToBoolean(stdUVGen.GetFlag(1 << 0)) || Convert.ToBoolean(stdUVGen.GetFlag(1 << 1));
                    // Mirror
                    bool Mirror = Convert.ToBoolean(stdUVGen.GetFlag(1 << 2)) || Convert.ToBoolean(stdUVGen.GetFlag(1 << 3));

                    // Debug.Print($"{stdUVGen.TextureTiling}");

                    string _imgWrap = "REPEAT";
                    if (Wrap || Mirror)
                    {
                        _imgWrap = "REPEAT";
                    }
                    else
                    {
                        _imgWrap = "CLIP";
                    }

                    if (Mirror && !Wrap)
                    {
                        _imgWrap = "MIRROR";
                    }

                    ti.Properties.Add("filename", _filename);

                    ti.Properties.Add("clipu", ClipU);
                    ti.Properties.Add("clipv", ClipV);
                    ti.Properties.Add("clipw", ClipW);
                    ti.Properties.Add("cliph", ClipH);

                    ti.Properties.Add("image_wrap", _imgWrap);

                    ti.Properties.Add("alpha_source", RedHaloTools.GetValeByID<int>(tex, 0, 11));

                    ti.Properties.Add("u_scale", stdUVGen.GetUScl(0));
                    ti.Properties.Add("v_scale", stdUVGen.GetVScl(0));
                    ti.Properties.Add("u_offset", stdUVGen.GetUOffs(0));
                    ti.Properties.Add("v_offset", stdUVGen.GetVOffs(0));

                    ti.Properties.Add("w_angle", stdUVGen.GetWAng(0));
                    ti.Properties.Add("v_angle", stdUVGen.GetVAng(0));
                    ti.Properties.Add("u_angle", stdUVGen.GetUAng(0));

                    // 贴图类型：纹理/环境
                    //ti.Properties.Add("mapping_type", $"{RedHaloTools.GetValeByID<int>(tex, 0, 11)}");

                    // 贴图方式
                    //ti.Properties.Add("mapping", $"{RedHaloTools.GetValeByID<int>(tex, 0, 11)}");
                    ti.Properties.Add("mono_output", RedHaloTools.GetValeByID<int>(tex, 0, 9));
                    ti.Properties.Add("rgb_output", RedHaloTools.GetValeByID<int>(tex, 0, 10));
                    //ti.Properties.Add("premult_alpha", $"{RedHaloTools.GetValeByID<int>(tex, 0, 12)}");

                    break;

                case "MultiTile":
                    Debug.Print($"======{texType}======");
                    //RedHaloTools.GetParams(tex);
                    break;
                
                case "Bricks":
                case "Tiles":
                    Debug.Print($"======{texType}======");
                    /// Has 4 refs
                    /// 0:StdUVGen
                    /// 1:ParamBlock2
                    /// 2:???  /// 3:???
                    //RedHaloTools.GetParams(tex);                  

                    //var aaa = tex.GetParamBlock(0);
                    //Debug.Print($"ParamBlock is {aaa}");
                    var ppb2 = tex.GetReference(1);
                    int numsubs = ppb2.NumSubs;
                    //Debug.Print($"======={ppb2.NumSubs}");
                    for (int i = 0; i < numsubs; i++)
                    {
                        //Debug.Print($"{i} - {ppb2.}");
                    }
                    

                    //var abab = ppb2.GetParamDefByIndex(9);

                    break;
                
                case "Checker":
                    // Color1
                    maxRGB = RedHaloTools.GetValeByID<IColor>(tex, 0, 1);
                    ti.Properties.Add("color1", $"{maxRGB.R},{maxRGB.G},{maxRGB.B},1");

                    // Color2
                    maxRGB = RedHaloTools.GetValeByID<IColor>(tex, 0, 2);
                    ti.Properties.Add("color2", $"{maxRGB.R},{maxRGB.G},{maxRGB.B},1");

                    //submap 1
                    var submap1 = RedHaloTools.GetValeByID<ITexmap>(tex, 0, 3);
                    AddSubtexmap("map1", submap1);

                    //submap 2
                    var submap2 = RedHaloTools.GetValeByID<ITexmap>(tex, 0, 4);
                    AddSubtexmap("map2", submap2);

                    break;
                
                case "Mix":
                    //color1
                    maxRGB = RedHaloTools.GetValeByID<IColor>(tex, 0, 4);
                    ti.Properties.Add("color1", $"{maxRGB.R},{maxRGB.G},{maxRGB.B},1");
                    
                    //color2
                    maxRGB = RedHaloTools.GetValeByID<IColor>(tex, 0, 5);
                    ti.Properties.Add("color2", $"{maxRGB.R},{maxRGB.G},{maxRGB.B},1");

                    // mix amnout
                    amount = RedHaloTools.GetValeByID<float>(tex, 0, 0);
                    ti.Properties.Add("mix_amount", amount);

                    //ti.Properties.Add("mix_mode", "MIX");

                    //map1
                    var mix_map1 = RedHaloTools.GetValeByID<ITexmap>(tex, 0, 6);
                    AddSubtexmap("map1", mix_map1);

                    // map2
                    var mix_map2 = RedHaloTools.GetValeByID<ITexmap>(tex, 0, 7);
                    AddSubtexmap("map2", mix_map2);

                    // mix mask
                    var mix_mask = RedHaloTools.GetValeByID<ITexmap>(tex, 0, 8);
                    AddSubtexmap("mask", mix_mask);

                    break;
                
                case "Falloff":
                    // color1
                    maxRGBA = RedHaloTools.GetValeByID<IAColor>(tex, 0, 0);
                    ti.Properties.Add("color1", $"{maxRGBA.R},{maxRGBA.G},{maxRGBA.B},{maxRGBA.A}");

                    // color2
                    maxRGBA = RedHaloTools.GetValeByID<IAColor>(tex, 0, 4);
                    ti.Properties.Add("color2", $"{maxRGBA.R},{maxRGBA.G},{maxRGBA.B},{maxRGBA.A}");

                    ti.Properties.Add("map1_amount", RedHaloTools.GetValeByID<float>(tex, 0, 1));
                    ti.Properties.Add("map2_amount", RedHaloTools.GetValeByID<float>(tex, 0, 5));

                    /*
                    Falloff Type:
                    0: Towards / Away
                    1: Perpendicular / Parallel
                    2: Fesnel
                    3: Shadow / Light
                    4: Distance Blend  -->  nearDistance == farDistance
                     */
                    ti.Properties.Add("type", RedHaloTools.GetValeByID<int>(tex, 0, 8));

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
                    ti.Properties.Add("direction", RedHaloTools.GetValeByID<int>(tex, 0, 9));
                    
                    ti.Properties.Add("ior", RedHaloTools.GetValeByID<int>(tex, 0, 12));

                    ti.Properties.Add("near_distance", RedHaloTools.GetValeByID<int>(tex, 0, 14));
                    ti.Properties.Add("far_distance", RedHaloTools.GetValeByID<int>(tex, 0, 15));
                    // map1
                    var falloff_map1 = RedHaloTools.GetValeByID<ITexmap>(tex, 0, 2);
                    AddSubtexmap("map1", falloff_map1);

                    // map2
                    var falloff_map2 = RedHaloTools.GetValeByID<ITexmap>(tex, 0, 6);
                    AddSubtexmap("map2", falloff_map2);

                    break;
                
                case "Color Correction":
                    ti.Type = "color_correction";
                    maxRGBA = RedHaloTools.GetValeByID<IAColor>(tex, 0, 0);
                    ti.Properties.Add("color", $"{maxRGBA.R},{maxRGBA.G},{maxRGBA.B},{maxRGBA.A}");

                    ti.Properties.Add("hue", RedHaloTools.GetValeByID<float>(tex, 0, 7));
                    ti.Properties.Add("saturation", RedHaloTools.GetValeByID<float>(tex, 0, 8));                    

                    // Color Mode
                    var clrMode = RedHaloTools.GetValeByID<int>(tex, 0, 2);
                    switch (clrMode)
                    {
                        case 0:
                            ti.Properties.Add("color_mode", "NORMAL");
                            break;
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
                    var ccMode = RedHaloTools.GetValeByID<int>(tex, 0, 11);
                    if (ccMode == 0)
                    {
                        ti.Properties.Add("gamma", 1.0);
                        ti.Properties.Add("brightness", RedHaloTools.GetValeByID<float>(tex, 0, 13));
                        ti.Properties.Add("contrast", RedHaloTools.GetValeByID<float>(tex, 0, 12));
                    }
                    else
                    {
                        ti.Properties.Add("gamma", RedHaloTools.GetValeByID<float>(tex, 0, 22));
                        ti.Properties.Add("brightness", 0);
                        ti.Properties.Add("contrast", 0);
                    }
                    // Texmap
                    var ccTexmap = RedHaloTools.GetValeByID<ITexmap>(tex, 0, 1);
                    AddSubtexmap("texmap", ccTexmap);

                    break;
                
                case "RGB Multiply":
                    ti.Type = "mix";
                    //color1
                    maxRGB = RedHaloTools.GetValeByID<IColor>(tex, 0, 0);
                    ti.Properties.Add("color1", $"{maxRGB.R},{maxRGB.G},{maxRGB.B},1");

                    // color2
                    maxRGB = RedHaloTools.GetValeByID<IColor>(tex, 0, 1);
                    ti.Properties.Add("color2", $"{maxRGB.R},{maxRGB.G},{maxRGB.B},1");

                    // mix mode
                    ti.Properties.Add("mix_mode", "MULTIPLY");

                    // map1
                    var RgbMult_map1 = RedHaloTools.GetValeByID<ITexmap>(tex, 0, 2);
                    AddSubtexmap("map1", RgbMult_map1);

                    // map2
                    var RgbMult_map2 = RedHaloTools.GetValeByID<ITexmap>(tex, 0, 2);
                    AddSubtexmap("map2", RgbMult_map2);

                    // alphafrom
                    ti.Properties.Add("alpha_from", RedHaloTools.GetValeByID<int>(tex, 0, 6));

                    break;
               
                case "Gradient":
                    maxRGB = RedHaloTools.GetValeByID<IColor>(tex, 0, 0);
                    ti.Properties.Add("color1", $"{maxRGBA.R},{maxRGBA.G},{maxRGBA.B},1");

                    maxRGB = RedHaloTools.GetValeByID<IColor>(tex, 0, 1);
                    ti.Properties.Add("color2", $"{maxRGBA.R},{maxRGBA.G},{maxRGBA.B},1");

                    maxRGB = RedHaloTools.GetValeByID<IColor>(tex, 0, 2);
                    ti.Properties.Add("color3", $"{maxRGBA.R},{maxRGBA.G},{maxRGBA.B},1");

                    ti.Properties.Add("color2_pos", RedHaloTools.GetValeByID<float>(tex, 0, 9));

                    // Gradient type
                    // 0: Linear
                    // 1: Radial
                    var gradientType = RedHaloTools.GetValeByID<int>(tex, 0, 10);
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
                    maxRGBA = RedHaloTools.GetValeByID<IAColor>(tex, 0, 0);
                    ti.Properties.Add("color", $"{maxRGBA.R},{maxRGBA.G},{maxRGBA.B},{maxRGBA.A}");

                    ti.Properties.Add("gamma", RedHaloTools.GetValeByID<float>(tex, 0, 3));
                    ti.Properties.Add("reverse_gamma", RedHaloTools.GetValeByID<int>(tex, 0, 5));

                    // Color Map
                    var colormap_texmap = RedHaloTools.GetValeByID<ITexmap>(tex, 0, 1);
                    var colormap_texmap_enable = RedHaloTools.GetValeByID<int>(tex, 0, 2) == 1;
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
                    ti.Properties.Add("map", RedHaloTools.GetValeByID<int>(tex, 0, 0));
                    break;

                #endregion

                #region VRay Textures
                // VRay Textures
                case "VRayDirt":
                    ti.Type = "ao";
                    // occuluded color
                    maxRGB = RedHaloTools.GetValeByID<IColor>(tex, 0, 2);
                    ti.Properties.Add("occluded_color", $"{maxRGB.R},{maxRGB.G},{maxRGB.B},1");

                    // unoccluded color
                    maxRGB = RedHaloTools.GetValeByID<IColor>(tex, 0, 4);
                    ti.Properties.Add("unoccluded_color", $"{maxRGB.R},{maxRGB.G},{maxRGB.B},1");

                    // radius
                    ti.Properties.Add("radius", RedHaloTools.GetValeByID<float>(tex, 0, 0));

                    // subdivs
                    ti.Properties.Add("subdivs", RedHaloTools.GetValeByID<int>(tex, 0, 8));

                    // mode
                    // 0:OUTSIDE 1: 2: 3: 4:INSIDE
                    var vr_mode = RedHaloTools.GetValeByID<int>(tex,0, 23);
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
                    ti.Properties.Add("only_sameobject", RedHaloTools.GetValeByID<int>(tex, 0, 17));

                    // texmap_radius
                    var vrao_texmap_radius = RedHaloTools.GetValeByID<ITexmap>(tex, 0, 1);
                    if(vrao_texmap_radius != null && RedHaloTools.GetValeByID<int>(tex, 0, 34) == 1)
                    {
                        ti.subTexmapInfo.Add("texmap_radius", ExportTexmap(vrao_texmap_radius));
                    }

                    // texmap_occluded
                    var vrao_texmap_occluded = RedHaloTools.GetValeByID<ITexmap>(tex, 0, 3);
                    if(vrao_texmap_occluded != null && RedHaloTools.GetValeByID<int>(tex, 0, 37) == 1)
                    {
                        ti.subTexmapInfo.Add("texmap_occluded", ExportTexmap(vrao_texmap_occluded));
                    }

                    // texmap_unoccluded
                    var vrao_texmap_unoccluded = RedHaloTools.GetValeByID<ITexmap>(tex, 0, 5);
                    if (vrao_texmap_unoccluded != null && RedHaloTools.GetValeByID<int>(tex, 0, 40) == 1)
                    {
                        ti.subTexmapInfo.Add("texmap_unoccluded", ExportTexmap(vrao_texmap_unoccluded));
                    }

                    break;

                case "VRayNormalMap":
                    ti.Type = "normal_map";
                    ti.Properties.Add("normal_strength", RedHaloTools.GetValeByID<float>(tex, 0, 2));
                    ti.Properties.Add("bump_strength", RedHaloTools.GetValeByID<float>(tex, 0, 5));
                    ti.Properties.Add("map_channel", RedHaloTools.GetValeByID<int>(tex, 0, 6));

                    ti.Properties.Add("flip_red", RedHaloTools.GetValeByID<int>(tex, 0, 7));
                    ti.Properties.Add("flip_green", RedHaloTools.GetValeByID<int>(tex, 0, 8));
                    ti.Properties.Add("swap_red_green", RedHaloTools.GetValeByID<int>(tex, 0, 9));

                    ti.Properties.Add("map_rotation", RedHaloTools.GetValeByID<float>(tex, 0, 10));

                    // normal map
                    var vr_normalmap = RedHaloTools.GetValeByID<ITexmap>(tex, 0, 0);
                    if (vr_normalmap != null && RedHaloTools.GetValeByID<int>(tex, 0, 1) == 1)
                    {
                        ti.subTexmapInfo.Add("normal_map", ExportTexmap(vr_normalmap));
                    }

                    // bump map
                    var vr_bumpmap = RedHaloTools.GetValeByID<ITexmap>(tex, 0, 3);
                    if (vr_bumpmap != null && RedHaloTools.GetValeByID<int>(tex, 0, 4) == 1)
                    {
                        ti.subTexmapInfo.Add("bump_map", ExportTexmap(vr_bumpmap));
                    }

                    break;

                case "VRayColor2Bump":
                    ti.Type = "vray_color2bump";
                    // height
                    ti.Properties.Add("height", RedHaloTools.GetValeByID<float>(tex, 0, 1));

                    // scale
                    ti.Properties.Add("scale", RedHaloTools.GetValeByID<float>(tex, 0, 2));

                    // map
                    var vr_color2bump = RedHaloTools.GetValeByID<ITexmap>(tex, 0, 0);
                    if (vr_color2bump != null)
                    {
                        ti.subTexmapInfo.Add("texmap", ExportTexmap(vr_color2bump));
                    }

                    break;

                case "VRayBump2Normal":
                    ti.Type = "vray_bump2normal";
                    // bump map mult
                    ti.Properties.Add("bump_map_mult", RedHaloTools.GetValeByID<float>(tex, 0, 1));

                    // mode
                    // 0:Tangent Space 1:Local XYZ 2:Screen Sapce 3:World Space
                    ti.Properties.Add("mode", RedHaloTools.GetValeByID<int>(tex, 0, 2));

                    // channel
                    ti.Properties.Add("channel", RedHaloTools.GetValeByID<int>(tex, 0, 3));

                    // map
                    var vr_bump2normal = RedHaloTools.GetValeByID<ITexmap>(tex, 0, 0);
                    if (vr_bump2normal != null)
                    {
                        ti.subTexmapInfo.Add("texmap", ExportTexmap(vr_bump2normal));
                    }
                    break;

                case "VRayBitmap":
                    ti.Type = "bitmap";
                    // 处理图片，如果Apply启用，且Width或Height小于1，渲染一张新图片，并启用此图片
                    croppintApply = RedHaloTools.GetValeByID<int>(tex, 0, 18);
                    
                    ClipU = RedHaloTools.GetValeByID<float>(tex, 0, 20);
                    ClipV = RedHaloTools.GetValeByID<float>(tex, 0, 21);
                    ClipH = RedHaloTools.GetValeByID<float>(tex, 0, 23);
                    ClipW = RedHaloTools.GetValeByID<float>(tex, 0, 22);

                    _filename = RedHaloTools.GetValeByID<string>(tex, 0, 0);
                    
                    new_bm = null;                    

                    // 如果开启了裁切，重新渲染一张新图
                    if (croppintApply == 1 && ((ClipH < 1) || (ClipW < 1)))
                    {
                        string filepath = Path.GetDirectoryName(_filename);
                        string filename = Path.GetFileNameWithoutExtension(_filename);
                        string fileext = Path.GetExtension(_filename);

                        // 渲染新贴图
                        var bmInfo = RedHaloTools.GetValeByID<IPBBitmap>(tex, 0, 13);

                        float bm_width = bmInfo.Bi.Width * ClipW > 2 ? bmInfo.Bi.Width * ClipW : 2;
                        float bm_height = bmInfo.Bi.Height * ClipH > 2 ? bmInfo.Bi.Height * ClipH : 2;

                        _filename = Path.Combine(filepath, $"{filename}_001{fileext}");
                        //new_bm = CreateBitmap(tex, (ushort)bm_width, (ushort)bm_height, _filename);
                    }

                    stdUVGen = RedHaloTools.GetValeByID<IReferenceTarget>(tex, 0, 29) as IStdUVGen;
                    
                    //ImageWrap
                    // Tiling
                    Wrap = Convert.ToBoolean(stdUVGen.GetFlag(1 << 0)) || Convert.ToBoolean(stdUVGen.GetFlag(1 << 1));
                    // Mirror
                    Mirror = Convert.ToBoolean(stdUVGen.GetFlag(1 << 2)) || Convert.ToBoolean(stdUVGen.GetFlag(1 << 3));

                    _imgWrap = "REPEAT";
                    if (Wrap || Mirror)
                    {
                        _imgWrap = "REPEAT";
                    }
                    else
                    {
                        _imgWrap = "CLIP";
                    }

                    if (Mirror && !Wrap)
                    {
                        _imgWrap = "MIRROR";
                    }                    

                    ti.Properties.Add("filename", _filename);

                    ti.Properties.Add("clipu", $"{ClipU}");
                    ti.Properties.Add("clipv", $"{ClipV}");
                    ti.Properties.Add("clipw", $"{ClipW}");
                    ti.Properties.Add("cliph", $"{ClipH}");
                    
                    ti.Properties.Add("image_wrap", _imgWrap);

                    ti.Properties.Add("alpha_source", RedHaloTools.GetValeByID<int>(tex, 0, 26));

                    ti.Properties.Add("u_scale", $"{stdUVGen.GetUScl(0)}");
                    ti.Properties.Add("v_scale", $"{stdUVGen.GetVScl(0)}");
                    ti.Properties.Add("u_offset", $"{stdUVGen.GetUOffs(0)}");
                    ti.Properties.Add("v_offset", $"{stdUVGen.GetVOffs(0)}");

                    ti.Properties.Add("w_angle", $"{stdUVGen.GetWAng(0)}");
                    ti.Properties.Add("v_angle", $"{stdUVGen.GetVAng(0)}");
                    ti.Properties.Add("u_angle", $"{stdUVGen.GetUAng(0)}");

                    // 贴图类型：纹理/环境
                    ti.Properties.Add("mapping_type", RedHaloTools.GetValeByID<int>(tex, 0, 3));

                    // 贴图方式
                    //ti.Properties.Add("mapping", $"{RedHaloTools.GetValeByID<int>(tex, 0, 11)}");
                    ti.Properties.Add("mono_output", RedHaloTools.GetValeByID<int>(tex, 0, 25));
                    ti.Properties.Add("rgb_output", RedHaloTools.GetValeByID<int>(tex, 0, 24));
                    //ti.Properties.Add("premult_alpha", $"{RedHaloTools.GetValeByID<int>(tex, 0, 12)}");

                    // Gain
                    float overallMult = RedHaloTools.GetValeByID<float>(tex, 0, 1);
                    float renderMult = RedHaloTools.GetValeByID<float>(tex, 0, 2);
                    ti.Properties.Add("gain", $"{overallMult * renderMult}");
                    
                    break;

                case "VRayColor":
                    ti.Type = "color_map";
                    // Color / Temp
                    // 如果是色温，转成颜色
                    var vrcolor_mode = RedHaloTools.GetValeByID<int>(tex, 0, 0);
                    var temperature = 6500f;
                    if ( vrcolor_mode == 0)
                    {
                        temperature = RedHaloTools.GetValeByID<float>(tex, 0, 1);
                        maxRGB = RedHaloTools.GetRgbFromKelvin(temperature);
                    }
                    else
                    {
                        var clr_r = RedHaloTools.GetValeByID<float>(tex, 0, 2);
                        var clr_g = RedHaloTools.GetValeByID<float>(tex, 0, 3);
                        var clr_b = RedHaloTools.GetValeByID<float>(tex, 0, 4);
                        maxRGB = RedHaloCore.Global.Color.Create(clr_r, clr_g, clr_b);
                    }
                    ti.Properties.Add("color", $"{maxRGB.R},{maxRGB.G},{maxRGB.B},1");

                    // gamma
                    ti.Properties.Add("gamma", RedHaloTools.GetValeByID<float>(tex, 0, 8));

                    break;

                case "VRayCompTex":
                    ti.Type = "mix";
                    // Source A
                    var vr_comptex_a = RedHaloTools.GetValeByID<ITexmap>(tex, 0, 0);
                    if ( vr_comptex_a != null)
                    {
                        ti.subTexmapInfo.Add("map1", ExportTexmap(vr_comptex_a));
                    }

                    // Source B
                    var vr_comptex_b = RedHaloTools.GetValeByID<ITexmap>(tex, 0, 1);
                    if ( vr_comptex_b != null)
                    {
                        ti.subTexmapInfo.Add("map2", ExportTexmap(vr_comptex_b));
                    }

                    // Operator
                    var MixModeNames = new string[] { "ADD", "SUBTRACT", "DIFFERENCE", "MULTIPLY", "DIVIDE", "MINIMUM", "MAXIMUM", "COLORSHIFT", "COLORTINT", "BENDALPHASTRAIGHT", "BLENDAPLHAPREMULIPLIED" };                    
                    ti.Properties.Add("mode", $"{MixModeNames[RedHaloTools.GetValeByID<int>(tex, 0, 2)]}");

                    // multiplier
                    ti.Properties.Add("multiplier", RedHaloTools.GetValeByID<float>(tex, 0, 3));
                    break;

                #endregion

                #region Corona Textures
                // Corona Textures
                case "CoronaAO":
                    ti.Type = "ao";
                    // occuluded color
                    maxRGB = RedHaloTools.GetValeByID<IColor>(tex, 0, 0);
                    ti.Properties.Add("occluded_color", $"{maxRGB.R},{maxRGB.G},{maxRGB.B},1");

                    // unoccluded color
                    maxRGB = RedHaloTools.GetValeByID<IColor>(tex, 0, 1);
                    ti.Properties.Add("unoccluded_color", $"{maxRGB.R},{maxRGB.G},{maxRGB.B},1");

                    // radius 场景尺寸
                    ti.Properties.Add("radius", RedHaloTools.GetValeByID<float>(tex, 0, 3));

                    // subdivs
                    ti.Properties.Add("subdivs", RedHaloTools.GetValeByID<int>(tex, 0, 2));

                    // mode
                    // 0:OUTSIDE 1: 2: 3: 4:INSIDE
                    var cr_mode = RedHaloTools.GetValeByID<int>(tex, 0, 17);
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
                    var exeludeMode = RedHaloTools.GetValeByID<int>(tex, 0, 12);
                    if (exeludeMode == 3)
                    {
                        ti.Properties.Add("only_sameobject", "1");
                    }
                    else
                    {
                        ti.Properties.Add("only_sameobject", "0");
                    }
                    

                    // texmap_radius
                    var crao_texmap_radius = RedHaloTools.GetValeByID<ITexmap>(tex, 0, 9);
                    if (crao_texmap_radius != null && RedHaloTools.GetValeByID<int>(tex, 0, 15) == 1)
                    {
                        ti.subTexmapInfo.Add("texmap_radius", ExportTexmap(crao_texmap_radius));
                    }

                    // texmap_occluded
                    var crao_texmap_occluded = RedHaloTools.GetValeByID<ITexmap>(tex, 0, 7);
                    if (crao_texmap_occluded != null && RedHaloTools.GetValeByID<int>(tex, 0, 13) == 1)
                    {
                        ti.subTexmapInfo.Add("texmap_occluded", ExportTexmap(crao_texmap_occluded));
                    }

                    // texmap_unoccluded
                    var crao_texmap_unoccluded = RedHaloTools.GetValeByID<ITexmap>(tex, 0, 8);
                    if (crao_texmap_unoccluded != null && RedHaloTools.GetValeByID<int>(tex, 0, 14) == 1)
                    {
                        ti.subTexmapInfo.Add("texmap_unoccluded", ExportTexmap(crao_texmap_unoccluded));
                    }
                    break;

                case "CoronaMix":      
                    ti.Type = "mix";
                    //color1
                    maxRGB = RedHaloTools.GetValeByID<IColor>(tex, (short)0, 10);
                    ti.Properties.Add("color1", $"{maxRGB.R},{maxRGB.G},{maxRGB.B},1");

                    //color2
                    maxRGB = RedHaloTools.GetValeByID<IColor>(tex, (short)0, 9);
                    ti.Properties.Add("color2", $"{maxRGB.R},{maxRGB.G},{maxRGB.B},1");

                    // mix amnout
                    amount = RedHaloTools.GetValeByID<float>(tex, 0, 1);
                    ti.Properties.Add("mix_amount", amount);

                    // mix mode
                    var CoronaMixMode = new string[] { "ADD", "SUBTRACT", "MULTIPLY", "DIVIDE", "MINIMUM", "MAXIMUM", "MIX", "GAMMA", "DIFFERENCE", "SCREEN", "OVERLAY", "DODGE", "BURN", "LINEARBURN", "LINEAR_LIGHT", "SOFT_LIGHT", "HARDLIGHT", "VIVIDLIGHT", "PINLIGHT", "HARDMIX", "EXCLUSION" };
                    ti.Properties.Add("mix_mode", CoronaMixMode[RedHaloTools.GetValeByID<int>(tex, 0, 0)]);

                    //map1
                    var cr_mix_map1 = RedHaloTools.GetValeByID<ITexmap>(tex, 0, 6);
                    if(cr_mix_map1 != null && RedHaloTools.GetValeByID<int>(tex, 0, 12) == 1)
                    {
                        ti.subTexmapInfo.Add("map1", ExportTexmap(cr_mix_map1));
                    }

                    // map2
                    var cr_mix_map2 = RedHaloTools.GetValeByID<ITexmap>(tex, 0, 7);
                    if(cr_mix_map2 != null && RedHaloTools.GetValeByID<int>(tex, 0, 11) == 1)
                    {
                        ti.subTexmapInfo.Add("map2", ExportTexmap(cr_mix_map2));
                    }

                    // mix mask
                    var cr_mix_mask = RedHaloTools.GetValeByID<ITexmap>(tex, 0, 8);
                    if(cr_mix_mask != null && RedHaloTools.GetValeByID<int>(tex, 0, 13) == 1)
                    {
                        ti.subTexmapInfo.Add("mask", ExportTexmap(cr_mix_mask));
                    }

                    break;

                case "CoronaColorCorrect":
                    ti.Type = "color_correction";

                    ti.Properties.Add("color", $"0.8,0.8,0.8,1");

                    ti.Properties.Add("hue", RedHaloTools.GetValeByID<float>(tex, 0, 23));
                    ti.Properties.Add("saturation", RedHaloTools.GetValeByID<float>(tex, 0, 4));

                    // Color Mode
                    var clrInvert = RedHaloTools.GetValeByID<int>(tex, 0, 9);
                    if(clrInvert == 1)
                    {
                        ti.Properties.Add("color_mode", "INVERT");
                    }
                    else
                    {
                        ti.Properties.Add("color_mode", "NORMAL");
                    }

                    ti.Properties.Add("gamma", RedHaloTools.GetValeByID<float>(tex, 0, 12));
                    ti.Properties.Add("brightness", RedHaloTools.GetValeByID<float>(tex, 0, 2));
                    ti.Properties.Add("contrast", RedHaloTools.GetValeByID<float>(tex, 0, 3));

                    // Texmap
                    var cr_ccTexmap = RedHaloTools.GetValeByID<ITexmap>(tex, 0, 1);
                    if (cr_ccTexmap != null)
                    {
                        ti.subTexmapInfo.Add("texmap", ExportTexmap(cr_ccTexmap));
                    }

                    break;

                case "CoronaNormal":
                    ti.Type = "normal_map";

                    ti.Properties.Add("normal_strength", RedHaloTools.GetValeByID<float>(tex, 0, 0));
                    ti.Properties.Add("bump_strength", RedHaloTools.GetValeByID<float>(tex, 0, 9));
                    ti.Properties.Add("map_channel", "1");

                    ti.Properties.Add("flip_red", RedHaloTools.GetValeByID<int>(tex, 0, 3));
                    ti.Properties.Add("flip_green", RedHaloTools.GetValeByID<int>(tex, 0, 4));
                    ti.Properties.Add("swap_red_green", RedHaloTools.GetValeByID<int>(tex, 0, 5));

                    ti.Properties.Add("map_rotation", "0");

                    // normal map
                    var cr_normalmap = RedHaloTools.GetValeByID<ITexmap>(tex, 0, 1);
                    if (cr_normalmap != null)
                    {
                        ti.subTexmapInfo.Add("normal_map", ExportTexmap(cr_normalmap));
                    }

                    // bump map
                    var cr_bumpmap = RedHaloTools.GetValeByID<ITexmap>(tex, 0, 7);
                    if (cr_bumpmap != null && RedHaloTools.GetValeByID<int>(tex, 0, 8) == 1)
                    {
                        ti.subTexmapInfo.Add("bump_map", ExportTexmap(cr_bumpmap));
                    }
                    break;

                case "CoronaBumpConverter":
                    ti.Type = "corona_bump_converter";
                    var cr_bump_converter_tex = RedHaloTools.GetValeByID<ITexmap>(tex, 0, 0);
                    
                    if(cr_bump_converter_tex != null)
                    {
                        ti.subTexmapInfo.Add("texmap", ExportTexmap(cr_bump_converter_tex));
                        ti.Properties.Add("strength", RedHaloTools.GetValeByID<float>(tex, 0, 1));
                    }
                    
                    break;

                case "CoronaFrontBack":
                    Debug.Print($"======{texType}======");
                    RedHaloTools.GetParams(tex);
                    ti.Type = "corona_front_back";

                    // Color1
                    maxRGB = RedHaloTools.GetValeByID<IColor>(tex, 0, 0);
                    ti.Properties.Add("color1", $"{maxRGB.R},{maxRGB.G},{maxRGB.B},1");
                    
                    // Color2
                    maxRGB = RedHaloTools.GetValeByID<IColor>(tex, 0, 1);
                    ti.Properties.Add("color2", $"{maxRGB.R},{maxRGB.G},{maxRGB.B},1");

                    // map1
                    var cr_front_tex = RedHaloTools.GetValeByID<ITexmap>(tex, 0, 4);
                    if(cr_front_tex != null && RedHaloTools.GetValeByID<int>(tex, 0, 2) == 1)
                    {
                        ti.subTexmapInfo.Add("map1", ExportTexmap(cr_front_tex));
                    }
                    
                    // map2
                    var cr_back_tex = RedHaloTools.GetValeByID<ITexmap>(tex, 0, 5);
                    if(cr_back_tex != null && RedHaloTools.GetValeByID<int>(tex, 0, 3) == 1)
                    {
                        ti.subTexmapInfo.Add("map2", ExportTexmap(cr_back_tex));
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
                            "Normal", "Average", "Addition", "Subtract", "Darken", "Multiply", "Color Burn", "Linear Burn", "Lighten", "Screen", "Color Dodge", "Linear Dodge", 
                            "Spotlight", "Spotlight Blend", "Overlay", "Soft Light", "Hard Light", "Pin Light", "Hard Mix", "Difference", "Exclusion", "Hue", "Saturation", "color", "Value" };
                        
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
