using System.IO;
using ManagedServices;

namespace RedHaloM2B
{
    internal class ScriptsUtilities
    {
        public static void ExecuteMaxScriptCommand(string ms)
        {
            if (!string.IsNullOrEmpty(ms))
            {
#if MAX2022 || MAX2023 || MAX2024 || MAX2025 || MAX2026
                ManagedServices.MaxscriptSDK.ExecuteMaxscriptCommand(ms, ManagedServices.MaxscriptSDK.ScriptSource.NotSpecified);
#else
                ManagedServices.MaxscriptSDK.ExecuteMaxscriptCommand(ms);
#endif
            }
        }

        public static void ExecuteMaxScriptFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                string  maxScriptcmd = File.ReadAllText(filePath);
                ExecuteMaxScriptCommand(maxScriptcmd);
            }
        }
    }
}
