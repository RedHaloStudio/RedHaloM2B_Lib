using Newtonsoft.Json;

namespace RedHaloM2B.Textures
{
    public class RedHaloTilesTex : BaseTex
    {
        // "mortar_map", "bricks_map", "brick_color", "tile_type", "mortar_color", "horizontal_count", "vertical_count", "horizontal_gap", "line_shift"

        // 0 Mortar Color: Type: Rgba - Value: 0.20,0.2,0.2
        [JsonProperty("mortar_color")]
        public string MortarColor { get; set; } = "0.2,0.2,0.2";

        // Mortar Texmap
        [JsonProperty("mortar_map")]
        public TexmapInfo MortarTexmap { get; set; }

        // 1 Brick Color: Type: Rgba - Value: 0.60,0.6,0.6
        [JsonProperty("brick_color")]
        public string BrickColor { get; set; } = "0.6,0.6,0.6";

        // Brick Texmap
        [JsonProperty("bricks_map")]
        public TexmapInfo BrickTexmap { get; set; }

        // 2 Horizontal Count: Type: Float - Value: 4
        [JsonProperty("horizontal_count")]
        public float HorizontalCount { get; set; }

        // 3 Vertical Count: Type: Float - Value: 8
        [JsonProperty("vertical_count")]
        public float VerticalCount { get; set; }

        // 4 Color Variance: Type: Float - Value: 0
        [JsonProperty("color_variance")]
        public float ColorVariance { get; set; }

        // 5 Vertical Gap: Type: Float - Value: 0.5
        [JsonProperty("vertical_gap")]
        public float VerticalGap { get; set; }

        // 6 Horizontal Gap: Type: Float - Value: 0.5
        [JsonProperty("horizontal_gap")]
        public float HorizontalGap { get; set; }

        // 7 Line Shift: Type: Float - Value: 0.5
        [JsonProperty("line_shift")]
        public float LineShift { get; set; }

        // 8 Random Shift: Type: Float - Value: 0
        [JsonProperty("random_shift")]
        public float RandomShift { get; set; }

        // 9 Holes: Type: Int - Value: 0
        // 10 Random Seed: Type: Int - Value: 16744
        // 11 Fade Variance: Type: Int - Value: 1
        // 12 Edge Roughness: Type: Float - Value: 0.05
        // 13 : Type: Float - Value: 0
        // 14 : Type: Int - Value: 1
        // 15 : Type: Int - Value: 1
        // 16 : Type: Int - Value: 1
        // 17 Row Change: Type: Float - Value: 0.5
        // 18 Column Change: Type: Float - Value: 0.5
        // 19 Per Column: Type: Int - Value: 1
        // 20 Per Row: Type: Int - Value: 2
        // 21 Tiles Type: Type: Int - Value: 7

    }
}
