using HarmonyLib;
using System.Reflection;
using Timberborn.SingletonSystem;
using TimberApi.DependencyContainerSystem;
using TimberApi.ConsoleSystem;
using Timberborn.SkySystem;
using UnityEngine;

namespace Yurand.Timberborn.TimelapseCamera
{
    public class ScreenshotSunService : ILoadableSingleton
    {
        private MethodInfo sunUpdateColorsMethod;
        private FieldInfo sunLightField;

        private DayStageTransition dayStageTransition;
        private IConsoleWriter console;
        private Sun sun;

        public ScreenshotSunService(IConsoleWriter console, Sun sun) {
            this.console = console;
            this.sun = sun;
        }

        public void Load()
        {
            var sunClassType = typeof(Sun);

            sunUpdateColorsMethod = AccessTools.Method(sunClassType, "UpdateColors", new System.Type[]{
                typeof(DayStageTransition)
            }) ?? throw new System.Exception("ScreenshotSunService: method UpdateColors not found");

            dayStageTransition = new DayStageTransition(DayStage.Day, "", DayStage.Day, "", 0.0f);

            sunLightField = AccessTools.Field(sunClassType, "_sun")
                ?? throw new System.Exception("ScreenshotSunService: Sun light field not found");

            if (PluginEntryPoint.debugLogging)
                console.LogInfo("Successfully loaded ScreenshotSunService");
        }

        public void setSunForScreenshotRendering(Camera camera) {
            sunUpdateColorsMethod.Invoke(sun, new object[]{ dayStageTransition });
            ((Light) sunLightField.GetValue(sun)).transform.rotation = camera.transform.rotation;
        }

        public void resetSunForScreenshotRendering() {
            sun.Update();
        }
    }
}