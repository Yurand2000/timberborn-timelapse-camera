using System;
using TimberApi.ConsoleSystem;
using TimberApi.DependencyContainerSystem;
using Timberborn.Core;
using Timberborn.ToolSystem;
using Timberborn.SingletonSystem;
using Timberborn.WaterSystemRendering;
using Timberborn.LevelVisibilitySystem;
using Timberborn.Persistence;
using UnityEngine;

namespace Yurand.Timberborn.TimelapseCamera
{
    public class ScreenshotService : IPostLoadableSingleton, ISaveableSingleton
    {
        public static System.Action<string, string> TakeScreenshotDelegate;
        public static System.Action SetCameraDelegate;
        private GameObject screenshotCameraObject;
        private Camera screenshotCamera;
        private IConsoleWriter console;
        private WaterOpacityService waterOpacityService;
        private ToolGroupManager toolGroupManager;
        private ToolManager toolManager;
        private ILevelVisibilityService levelVisibilityService;
        private ScreenshotSunService screenshotSunService;

        public ScreenshotService(
            IConsoleWriter console,
            WaterOpacityService waterOpacityService,
            ToolGroupManager toolGroupManager,
            ToolManager toolManager,
            ILevelVisibilityService levelVisibilityService,
            ScreenshotSunService screenshotSunService
        ) {
            this.console = console;
            this.waterOpacityService = waterOpacityService;
            this.toolManager = toolManager;
            this.toolGroupManager = toolGroupManager;
            this.levelVisibilityService = levelVisibilityService;
            this.screenshotSunService = screenshotSunService;

            if (PluginEntryPoint.debugLogging) {
                console.LogInfo("Screenshot Service Initialized Successfully");
            }
        }

        public void PostLoad()
        {
            //create screenshot camera
            screenshotCameraObject = new GameObject("ScreenshotCamera");
            screenshotCamera = screenshotCameraObject.AddComponent<Camera>();
            screenshotCamera.enabled = false;

            //copy camera settings from main camera
            var local_camera = Camera.main;
            if (local_camera != null) {
                screenshotCamera.CopyFrom(local_camera);
                screenshotCamera.cullingMask &= ~(int)Layers.UIMask;

                if (PluginEntryPoint.debugLogging)
                    console.LogInfo("Successfully Created Screenshot Camera");
            } else {
                console.LogError("Cannot Find Main Game Camera");
                return;
            }

            //try load camera settings
            var singleton_loader = DependencyContainer.GetInstance<ISingletonLoader>();
            if (singleton_loader.HasSingleton(singletonKey))
            {
                var loader = singleton_loader.GetSingleton(singletonKey);
                if (loader.Has(CameraRotationKey)) {
                    screenshotCamera.transform.rotation = loader.Get(CameraRotationKey);

                    if (PluginEntryPoint.debugLogging)
                        console.LogInfo("Loaded Screenshot Camera Rotation: " + screenshotCamera.transform.rotation.ToString());
                }

                if (loader.Has(CameraPositionKey)) {
                    screenshotCamera.transform.position = loader.Get(CameraPositionKey);

                    if (PluginEntryPoint.debugLogging)
                        console.LogInfo("Loaded Screenshot Camera Position: " + screenshotCamera.transform.position.ToString());
                }

                if (loader.Has(CameraFovKey)) {
                    screenshotCamera.fieldOfView = loader.Get(CameraFovKey);

                    if (PluginEntryPoint.debugLogging)
                        console.LogInfo("Loaded Screenshot Camera FOV: " + screenshotCamera.fieldOfView.ToString());
                }
            }

            //setup service delegates
            TakeScreenshotDelegate = TakeScreenshot;
            SetCameraDelegate = SetScreenshotCamera;

            if (PluginEntryPoint.debugLogging) {
                console.LogInfo("Screenshot Service Loaded Successfully");
            }
        }

        public void Save(ISingletonSaver saverService)
        {
            var saver = saverService.GetSingleton(singletonKey);
            saver.Set(CameraRotationKey, screenshotCamera.transform.rotation);
            saver.Set(CameraPositionKey, screenshotCamera.transform.position);
            saver.Set(CameraFovKey, screenshotCamera.fieldOfView);

            if (PluginEntryPoint.debugLogging) {
                console.LogInfo("Screenshot Service Saved Successfully");
            }
        }

        private void SetScreenshotCamera() {
            var local_camera = Camera.main;
            if (local_camera != null) {
                screenshotCamera.transform.position = local_camera.transform.position;
                screenshotCamera.transform.rotation = local_camera.transform.rotation;
                screenshotCamera.fieldOfView = local_camera.fieldOfView;

                if (PluginEntryPoint.debugLogging)
                    console.LogInfo("Successfully Updated Screenshot Camera Settings");
            } else {
                console.LogError("Cannot Find Main Game Camera");
            }
        }

        private void TakeScreenshot(string output_folder, string output_file) {
            if (screenshotCamera != null) {
                SaveCameraView(PluginEntryPoint.directory + "/" + output_folder, output_file);
            } else {
                console.LogError("Screenshot Camera Not Available");
            }
        }
        private void SaveCameraView(string output_folder, string output_file)
        {
            RenderTexture screenTexture = new RenderTexture(Screen.width, Screen.height, 16);

            //setup scene
            var activeTool = toolManager.ActiveTool;
            var activeToolGroup = toolGroupManager.ActiveToolGroup;
            var waterHidden = waterOpacityService.IsWaterHidden;
            var maxVisibileLevel = levelVisibilityService.MaxVisibleLevel;
            var isMaxVisible = levelVisibilityService.LevelIsAtMax;
            var isFogVisible = RenderSettings.fog;

            screenshotSunService.setSunForScreenshotRendering(screenshotCamera);
            if (activeTool is not null) toolManager.SwitchToDefaultTool();
            if (activeToolGroup is not null) toolGroupManager.CloseToolGroup();
            if (waterHidden) waterOpacityService.ToggleOpacityOverride();
            if (isMaxVisible) levelVisibilityService.ResetMaxVisibleLevel();
            if (isFogVisible) RenderSettings.fog = false;

            //render screenshot
            screenshotCamera.enabled = true;
            screenshotCamera.targetTexture = screenTexture;
            screenshotCamera.Render();
            screenshotCamera.targetTexture = null;
            screenshotCamera.enabled = false;

            //reset scene
            if (isFogVisible) RenderSettings.fog = true;
            if (isMaxVisible) levelVisibilityService.SetMaxVisibleLevel(maxVisibileLevel);
            if (waterHidden) waterOpacityService.ToggleOpacityOverride();
            if (activeToolGroup is not null) toolGroupManager.SwitchToolGroup(activeToolGroup);
            if (activeTool is not null) toolManager.SwitchTool(activeTool);
            screenshotSunService.resetSunForScreenshotRendering();

            //read rendered image
            Texture2D renderedTexture = new Texture2D(Screen.width, Screen.height);
            RenderTexture.active = screenTexture;
            renderedTexture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            renderedTexture.Apply();
            RenderTexture.active = null;
            byte[] byteArray = renderedTexture.EncodeToPNG();

            //destroy temporary data.
            UnityEngine.Object.Destroy(renderedTexture);
            screenTexture.Release();

            //save on file
            try {
                System.IO.Directory.CreateDirectory(output_folder);
                System.IO.File.WriteAllBytes(output_folder + "/" + output_file, byteArray);

                if (PluginEntryPoint.debugLogging)
                    console.LogInfo("Successfully Saved Screenshot");
            } catch (Exception e) {
                console.LogError("Exceprion on saving screenshot: " + e.Message);
            }
        }

        private static readonly SingletonKey singletonKey = new SingletonKey(nameof(ScreenshotService));
        private static readonly PropertyKey<Quaternion> CameraRotationKey = new PropertyKey<Quaternion>("CameraRotation");
        private static readonly PropertyKey<Vector3> CameraPositionKey = new PropertyKey<Vector3>("CameraPosition");
        private static readonly PropertyKey<float> CameraFovKey = new PropertyKey<float>("CameraFOV");
    }
}
