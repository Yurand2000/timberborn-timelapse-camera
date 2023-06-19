using HarmonyLib;
using TimberApi.ConsoleSystem;
using TimberApi.ModSystem;

namespace Yurand.Timberborn.TimelapseCamera
{
    [HarmonyPatch]
    public class PluginEntryPoint : IModEntrypoint
    {
        public static string directory;

        public const bool debugLogging = true;

        public void Entry(IMod mod, IConsoleWriter consoleWriter)
        {
            new Harmony("Yurand.Timberborn.TimelapseCamera").PatchAll();
            directory = mod.DirectoryPath;
            consoleWriter.LogInfo(directory);
        }
    }
}