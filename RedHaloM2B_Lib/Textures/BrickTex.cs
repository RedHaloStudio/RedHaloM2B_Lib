using Autodesk.Max;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RedHaloM2B.Textures
{
    internal class BrickTex : BaseTex
    {
        /*
          "Mortar_Map", "Bricks_Map", "Brick_color", "Tile_Type", "Mortar_color", "Horizontal_Count", "Vertical_Count", "Horizontal_Gap", "Line_Shift"
         */
        [XmlIgnore]
        public IColor BrickColor { get; set; }

        public int TileType {  get; set; }
        [XmlIgnore]
        public IColor MortarColor { get; set; }
        public int HorizontalCount {  get; set; }
        public int VerticalCount { get; set; }
        public float HorizontalGap {  get; set; }
        public float LineShift {  get; set; }

        public string MortarMap {  get; set; } // 不定类型
        public string BrickMap {  get; set; }  // 不定类型
    }
}
