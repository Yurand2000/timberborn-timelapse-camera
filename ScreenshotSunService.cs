using HarmonyLib;
using System.Reflection;
using Timberborn.SingletonSystem;
using TimberApi.DependencyContainerSystem;
using TimberApi.ConsoleSystem;
using UnityEngine;

namespace Yurand.Timberborn.TimelapseCamera
{
    public class ScreenshotSunService : ILoadableSingleton
    {
        private MethodInfo sunUpdateMethod;
        private MethodInfo sunUpdateColorsMethod;
        private FieldInfo sunLightField;


        private object sunSingleton;
        private object dayStageTransition;
        private IConsoleWriter console;

        public ScreenshotSunService(IConsoleWriter console) {
            this.console = console;
        }

        public void Load()
        {
            var sunClassType = AccessTools.TypeByName("Timberborn.SkySystem.Sun")
                ?? throw new System.Exception("ScreenshotSunService: type Timberborn.SkySystem.Sun not found");
            var dayStageType = AccessTools.TypeByName("Timberborn.SkySystem.DayStage")
                ?? throw new System.Exception("ScreenshotSunService: type Timberborn.SkySystem.DayStage not found");
            var dayStageTransitionType = AccessTools.TypeByName("Timberborn.SkySystem.DayStageTransition")
                ?? throw new System.Exception("ScreenshotSunService: type Timberborn.SkySystem.DayStageTransition not found");

            sunUpdateMethod = AccessTools.Method(sunClassType, "Update")
                ?? throw new System.Exception("ScreenshotSunService: method Update not found");

            sunUpdateColorsMethod = AccessTools.Method(sunClassType, "UpdateColors", new System.Type[]{
                dayStageTransitionType
            }) ?? throw new System.Exception("ScreenshotSunService: method UpdateColors not found");

            var dayStageTransitionConstructor = AccessTools.Constructor(
                dayStageTransitionType,
                new System.Type[]{
                    dayStageType,
                    typeof(bool),
                    dayStageType,
                    typeof(bool),
                    typeof(float)
                }
            ) ?? throw new System.Exception("ScreenshotSunService: dayStageTransition constructor not found");

            var dayStageDay = System.Enum.GetValues(dayStageType).GetValue(1)
                ?? throw new System.Exception("ScreenshotSunService: dayStage enum as day not found");
            dayStageTransition = dayStageTransitionConstructor.Invoke(new object[] {dayStageDay, false, dayStageDay, false, 0.0f})
                ?? throw new System.Exception("ScreenshotSunService: dayStageTransition not constructed correctly");

            sunSingleton = DependencyContainer.GetInstance(sunClassType)
                ?? throw new System.Exception("ScreenshotSunService: Sun singleton not found");
            sunLightField = AccessTools.Field(sunClassType, "_sun")
                ?? throw new System.Exception("ScreenshotSunService: Sun light field not found");

            if (PluginEntryPoint.debugLogging)
                console.LogInfo("Successfully loaded ScreenshotSunService");
        }

        public void setSunForScreenshotRendering(Camera camera) {
            sunUpdateColorsMethod.Invoke(sunSingleton, new object[]{ dayStageTransition });
            ((Light) sunLightField.GetValue(sunSingleton)).transform.rotation = camera.transform.rotation;
        }

        public void resetSunForScreenshotRendering() {
            sunUpdateMethod.Invoke(sunSingleton, null);
        }
    }
}