using System;
using TimberApi.UiBuilderSystem;
using Timberborn.SingletonSystem;
using Timberborn.UILayoutSystem;
using UnityEngine.UIElements;
using static UnityEngine.UIElements.LengthUnit;

namespace Yurand.Timberborn.TimelapseCamera.UI
{
    public class TimelapseInterface : ILoadableSingleton
    {
        private readonly UILayout uiLayout;
        private readonly UIBuilder uiBuilder;

        private VisualElement uiRoot;

        private const int _panelOrder = 8;

        public TimelapseInterface(UILayout layout, UIBuilder builder)
        {
            uiLayout = layout ?? throw new ArgumentNullException(nameof(layout));
            uiBuilder = builder ?? throw new ArgumentNullException(nameof(builder));
        }

        public void Load()
        {
            var panelBuilder = uiBuilder
                .CreateComponentBuilder()
                .CreateVisualElement()
                .AddClass("top-right-item")
                .AddClass("square-large--green")
                .SetFlexDirection(FlexDirection.Row)
                .SetFlexWrap(Wrap.Wrap)
                .SetJustifyContent(Justify.Center);

            panelBuilder.AddPreset(factory => factory.Labels()
                .GameText(locKey: timelapseTopRightLabelLoc, builder:
                    builder => builder.SetStyle(style => {
                        style.alignSelf = Align.Center;
                        style.marginLeft = new Length(3, Pixel);
                        style.marginRight = new Length(3, Pixel);  
                        style.marginTop = new Length(3, Pixel);
                        style.marginBottom = new Length(3, Pixel);
                    })
            ));
            
            panelBuilder.AddPreset(factory => factory.Buttons()
                .ButtonGame(locKey: timelapseSetCameraButtonLoc, name: "SetCameraButton",
                    fontSize: new Length(8, Pixel),
                    builder: builder => builder.SetStyle(style => {
                        style.alignSelf = Align.Center;
                        style.marginLeft = new Length(3, Pixel);
                        style.marginRight = new Length(3, Pixel);  
                        style.marginTop = new Length(3, Pixel);
                        style.marginBottom = new Length(3, Pixel);
                    })
            ));

            panelBuilder.AddPreset(factory => factory.Buttons()
                .ButtonGame(locKey: timelapseSettingsButtonLoc, name: "SettingsButton",
                    fontSize: new Length(8, Pixel),
                    builder: builder => builder.SetStyle(style => {
                        style.alignSelf = Align.Center;
                        style.marginLeft = new Length(3, Pixel);
                        style.marginRight = new Length(3, Pixel);  
                        style.marginTop = new Length(3, Pixel);
                        style.marginBottom = new Length(3, Pixel);
                    })
            ));

            panelBuilder.AddPreset(factory => factory.Buttons()
                .ButtonGame(locKey: timelapseManualScreenshotButtonLoc, name: "ScreenshotButton",
                    fontSize: new Length(8, Pixel),
                    builder: builder => builder.SetStyle(style => {
                        style.alignSelf = Align.Center;
                        style.marginLeft = new Length(3, Pixel);
                        style.marginRight = new Length(3, Pixel);  
                        style.marginTop = new Length(3, Pixel);
                        style.marginBottom = new Length(3, Pixel);
                    })
            ));

            uiRoot = panelBuilder.BuildAndInitialize();

            uiRoot.Q<Button>("SettingsButton").clicked += () => { TimelapsePanel.OpenOptionsDelegate(); };
            uiRoot.Q<Button>("ScreenshotButton").clicked += () => { TimelapseManager.TakeManualScreenshotDelegate(); };
            uiRoot.Q<Button>("SetCameraButton").clicked += () => { ScreenshotService.SetCameraDelegate(); };

            uiLayout.AddTopRight(uiRoot, _panelOrder);
        }

        private const string timelapseTopRightLabelLoc = "yurand.timelapsecamera.ingame_panel";
        private const string timelapseSetCameraButtonLoc = "yurand.timelapsecamera.set_camera";
        private const string timelapseManualScreenshotButtonLoc = "yurand.timelapsecamera.manual_screenshot";
        private const string timelapseSettingsButtonLoc = "yurand.timelapsecamera.open_settings";
    }
}
