using Newtonsoft.Json;
using System;
using System.IO;
using System.Xml.Serialization;

namespace RedHaloM2B
{
    public class RedHaloSettings
    {
        [JsonProperty("world_unit")]
        public float WorldUnit { get; set; } = 1.0f;

        [JsonProperty("image_width")]
        public int ImageWidth { get; set; } = 1920;

        [JsonProperty("image_height")]
        public int ImageHeight { get; set; } = 1080;

        [JsonProperty("image_pixel_aspect")]
        public float ImagePixelAspect { get; set; } = 1.0f;

        [JsonProperty("start")]
        public int AnimateStart { get; set; } = 0;

        [JsonProperty("end")]
        public int AnimateEnd { get; set; } = 100;

        [JsonProperty("frame_rate")]
        public float FrameRate { get; set; } = 24.0f;

        [JsonProperty("gamma")]
        public float Gamma { get; set; } = 2.2f;

        [JsonProperty("linear_workflow")]
        public int LinearWorkflow { get; set; } = 1;

        [JsonProperty("export_format")]
        public string ExportFormat { get; set; } = "USD";

        [JsonProperty("output_path")]
        public string OutputPath { get; set; } = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), "RH_M2B_TEMP");

        [JsonProperty("log_path")]
        public string LogPath { get; set; } = Environment.GetEnvironmentVariable("TEMP");
    }
}