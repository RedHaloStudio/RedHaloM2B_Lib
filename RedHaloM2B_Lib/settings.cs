using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedHaloM2B
{
    public static class GlobalSettings
    {
        public static string ExportFormat { get; set; } = "USD";
        public static float SceneScale { get; set; } = 1.0f;
        public static string OutputPath { get; set; } = System.IO.Path.Combine(Environment.GetEnvironmentVariable("TEMP"), "RH_M2B_TEMP");
        public static string LogPath { get; set; } = Environment.GetEnvironmentVariable("TEMP");
        
    }
}
