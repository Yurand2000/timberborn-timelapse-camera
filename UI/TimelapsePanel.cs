using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timberborn.EntityPanelSystem;
using Timberborn.CoreUI;
using UnityEngine;
using UnityEngine.UIElements;
using TimberApi.UiBuilderSystem;
using Timberborn.BaseComponentSystem;
using Timberborn.Localization;
using Timberborn.SingletonSystem;
using Timberborn.UILayoutSystem;
using TimberApi.ConsoleSystem;
using static UnityEngine.UIElements.LengthUnit;

namespace Yurand.Timberborn.TimelapseCamera.UI
{
    public class TimelapsePanel : IPanelController
    {
        public static Action OpenOptionsDelegate;
        private UIBuilder uiBuilder;
        private TimelapseManager timelapseManager;
        private PanelStack panelStack;
        private ILoc loc;

        private IConsoleWriter console;

        public TimelapsePanel(UIBuilder uiBuilder, TimelapseManager timelapseManager, PanelStack panelStack, IConsoleWriter console, ILoc loc) {
            this.uiBuilder = uiBuilder;
            this.timelapseManager = timelapseManager;
            this.panelStack = panelStack;
            this.loc = loc;
            this.console = console;
            OpenOptionsDelegate = OpenOptionsPanel;

            if (PluginEntryPoint.debugLogging) {
                console.LogInfo("Timelapse Panel Initialized Successfully");
            }
        }

        private void OpenOptionsPanel() {
            panelStack.HideAndPush(this);

            if (PluginEntryPoint.debugLogging) {
                console.LogInfo("Timelapse Panel Opened");
            }
        }

        public VisualElement GetPanel() {
            if (timelapseManager == null) throw new Exception();

            UIBoxBuilder panelBuilder = uiBuilder.CreateBoxBuilder()
                .SetHeight(new Length(520, Pixel))
                .SetWidth(new Length(600, Pixel))
                .ModifyScrollView(builder => builder.SetName("elementPreview"));

            var panelContent = uiBuilder.CreateComponentBuilder().CreateVisualElement();

            panelContent.AddPreset(factory => factory.Toggles()
                .Checkbox(locKey: timelapsePanelEnableLoc,
                    name: "EnableTimelapse",
                    builder: builder => builder.SetStyle(style => {
                        style.marginBottom = new Length(10, Pixel);
                    })
            ));

            panelContent.AddPreset(factory => factory.Labels()
                .GameTextHeading(timelapsePanelFrequencySliderLoc, builder:
                    builder => builder.SetStyle(style => {
                        style.alignSelf = Align.Center;
                        style.marginBottom = new Length(10, Pixel);
                    })
            ));

            panelContent.AddPreset(factory => factory.Labels()
                .GameText(locKey: TimelapseFrequencyHelpers.LocalizedString(timelapseManager.Frequency),
                    name: "FrequencyLabel",
                    builder: builder => builder.SetStyle(style => {
                        style.alignSelf = Align.Center;
                        style.marginBottom = new Length(10, Pixel);
                    })
            ));

            panelContent.AddPreset(factory => factory.Sliders()
                .SliderIntCircle(
                    lowValue: TimelapseFrequencyHelpers.FrequencyMin(),
                    highValue: TimelapseFrequencyHelpers.FrequencyMax(),
                    value: TimelapseFrequencyHelpers.ToInt(timelapseManager.Frequency),
                    name: "FrequencySlider",
                    text: "",
                    builder: builder => builder.SetStyle(style => {
                        style.marginBottom = new Length(10, Pixel);
                    })
            ));
            
            panelBuilder.AddComponent(panelContent.Build());

            VisualElement root = panelBuilder
                .AddCloseButton("CloseButton")
                .SetBoxInCenter()
                .AddHeader(timelapsePanelHeaderLoc)
                .BuildAndInitialize();
            
            var frequencyLabel = root.Q<Label>("FrequencyLabel");
            var frequencySlider = root.Q<SliderInt>("FrequencySlider");
            frequencySlider.RegisterValueChangedCallback(changeEvent => {
                var new_frequency = TimelapseFrequencyHelpers.FromInt(changeEvent.newValue);

                frequencyLabel.text = loc.T(TimelapseFrequencyHelpers.LocalizedString(new_frequency));
                timelapseManager.SetFrequency(new_frequency);
            });

            var enabledButton = root.Q<Toggle>("EnableTimelapse");
            enabledButton.value = timelapseManager.Enabled;
            enabledButton.RegisterValueChangedCallback(changeEvent => {
                timelapseManager.SetEnabled(changeEvent.newValue);
            });

            root.Q<Button>("CloseButton").clicked += OnUICancelled;

            return root;
        }
        public void OnUICancelled()
        {
            panelStack.Pop(this);

            if (PluginEntryPoint.debugLogging) {
                console.LogInfo("Timelapse Panel Closed");
            }
        }
        public bool OnUIConfirmed()
        {
            return false;
        }

        private const string timelapsePanelHeaderLoc = "yurand.timelapsecamera.panel_header";
        private const string timelapsePanelEnableLoc = "yurand.timelapsecamera.enable";
        private const string timelapsePanelFrequencySliderLoc = "yurand.timelapsecamera.frequency";
    }
}