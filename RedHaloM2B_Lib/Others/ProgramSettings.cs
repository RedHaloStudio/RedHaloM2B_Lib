using Newtonsoft.Json;
using System;
using System.IO;

namespace RedHaloM2B
{
    public class AppSettings
    {
        [JsonProperty("export_format")]
        public string ExportFormat { get; set; } = "USD";

        [JsonProperty("output_path")]
        public string OutputPath { get; set; } = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), "RH_M2B_TEMP");

        [JsonProperty("log_path")]
        public string LogPath { get; set; } = Environment.GetEnvironmentVariable("TEMP");
    }
}