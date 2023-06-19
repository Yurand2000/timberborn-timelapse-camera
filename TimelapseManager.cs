using System;
using TimberApi.ConsoleSystem;
using TimberApi.DependencyContainerSystem;
using Timberborn.SingletonSystem;
using Timberborn.TimeSystem;
using Timberborn.TickSystem;
using Timberborn.Persistence;
using Timberborn.SettlementNameSystem;

namespace Yurand.Timberborn.TimelapseCamera
{
    public class TimelapseManager : IPostLoadableSingleton, ITickableSingleton, ISaveableSingleton
    {
        public bool Enabled { get; private set; }
        public TimelapseFrequency Frequency { get; private set; }
        public IngameDateTime LastScreenshotTime { get; private set; }
        private IngameDateTime nextScreenshotTime;

        private SettlementNameService settlementNameService;
        private IDayNightCycle timeSystem;
        private IConsoleWriter console;

        public static Action TakeManualScreenshotDelegate;

        public TimelapseManager(IConsoleWriter console, IDayNightCycle timeSystem, SettlementNameService settlementNameService)
        {
            this.console = console;
            this.timeSystem = timeSystem;
            this.settlementNameService = settlementNameService;
            this.LastScreenshotTime = new IngameDateTime(Int32.MaxValue, 0);
            this.Frequency = TimelapseFrequency.Daily;
            this.Enabled = false;

            if (PluginEntryPoint.debugLogging) {
                console.LogInfo("Timelapse Manager Initialized Successfully");
            }
        }

        public void Save(ISingletonSaver saverService)
        {
            var saver = saverService.GetSingleton(singletonKey);
            saver.Set(TimelapseEnabledKey, Enabled);
            saver.Set(TimelapseFrequencyKey, TimelapseFrequencyHelpers.ToInt(Frequency));
            saver.Set(LastScreenshotDayKey, LastScreenshotTime.day);
            saver.Set(LastScreenshotHourKey, LastScreenshotTime.hour);

            if (PluginEntryPoint.debugLogging) {
                console.LogInfo("Timelapse Manager Saved Successfully");
            }
        }

        public void PostLoad()
        {
            var singleton_loader = DependencyContainer.GetInstance<ISingletonLoader>();
            if (!singleton_loader.HasSingleton(singletonKey)) return;

            var loader = singleton_loader.GetSingleton(singletonKey);
            if (loader.Has(TimelapseFrequencyKey)) {
                Frequency = TimelapseFrequencyHelpers.FromInt(loader.Get(TimelapseFrequencyKey));
                ComputeNextScreenshotTime();

                if (PluginEntryPoint.debugLogging)
                    console.LogInfo("Loaded Frequency: " + Frequency.ToString());
            }
            if (loader.Has(TimelapseEnabledKey)) {
                Enabled = loader.Get(TimelapseEnabledKey);

                if (PluginEntryPoint.debugLogging)
                    console.LogInfo("Loaded Enabled: " + Enabled.ToString());
            }
            if (loader.Has(LastScreenshotDayKey) && loader.Has(LastScreenshotHourKey)) {
                var date = loader.Get(LastScreenshotDayKey);
                var time = loader.Get(LastScreenshotHourKey);
                LastScreenshotTime = new IngameDateTime(date, time);

                if (PluginEntryPoint.debugLogging)
                    console.LogInfo("Loaded Last Screenshot Time: " + LastScreenshotTime.ToString());
            }

            if (Enabled) ComputeNextScreenshotTime();
            TakeManualScreenshotDelegate = () => {
                ScreenshotService.TakeScreenshotDelegate(screenshotFolder(), manualScreenshotName() + ".png");
            };
            
            if (PluginEntryPoint.debugLogging) {
                console.LogInfo("Timelapse Manager Started Successfully");
            }
        }

        public void Tick()
        {
            if (!Enabled) return;

            var now = new IngameDateTime(timeSystem);
            if (now >= nextScreenshotTime) {
                ScreenshotService.TakeScreenshotDelegate(screenshotFolder(), screenshotName(now) + ".png");
                if (PluginEntryPoint.debugLogging) {
                    console.LogInfo("Screenshot fired at " + now.ToString());
                }

                LastScreenshotTime = nextScreenshotTime;
                ComputeNextScreenshotTime();
            }
        }

        public string manualScreenshotName() {
            return screenshotName(new IngameDateTime(timeSystem)) + " manual";
        }

        private string screenshotName(IngameDateTime now)
        {
            return String.Format("{0} - day-time {1}-{2}-00", settlementNameService.SettlementName, now.day, now.hour);
        }

        private string screenshotFolder()
        {
            return settlementNameService.SettlementName + "/";
        }

        public void SetEnabled(bool enabled) {
            Enabled = enabled;
            LastScreenshotTime = new IngameDateTime(timeSystem);
            ComputeNextScreenshotTime();

            if (PluginEntryPoint.debugLogging) {
                if (Enabled) console.LogInfo("Timelapse Enabled");
                else console.LogInfo("Timelapse Disabled");
            }
        }

        public void SetFrequency(TimelapseFrequency frequency) {
            Frequency = frequency;
            ComputeNextScreenshotTime();

            if (PluginEntryPoint.debugLogging) {
                console.LogInfo("Frequency set to " + frequency.ToString());
            }
        }

        private void ComputeNextScreenshotTime() {
            nextScreenshotTime = LastScreenshotTime;
            switch (Frequency) {
                case TimelapseFrequency.OneHour: nextScreenshotTime.addHours(1); break;
                case TimelapseFrequency.TwoHours: nextScreenshotTime.addHours(2); break;
                case TimelapseFrequency.FourHours: nextScreenshotTime.addHours(4); break;
                case TimelapseFrequency.SixHours: nextScreenshotTime.addHours(6); break;
                case TimelapseFrequency.EightHours: nextScreenshotTime.addHours(8); break;
                case TimelapseFrequency.TwelveHours: nextScreenshotTime.addHours(12); break;
                case TimelapseFrequency.Daily: nextScreenshotTime.addDays(1); break;
                case TimelapseFrequency.TwoDay: nextScreenshotTime.addDays(2); break;
                case TimelapseFrequency.EachCycle: nextScreenshotTime.addDays(30); break;
                default: nextScreenshotTime.day = Int32.MaxValue; break;
            }

            if (PluginEntryPoint.debugLogging) {
                console.LogInfo("Next screenshot at " + nextScreenshotTime.ToString());
            }
        }

        private static readonly SingletonKey singletonKey = new SingletonKey(nameof(TimelapseManager));
        private static readonly PropertyKey<bool> TimelapseEnabledKey = new PropertyKey<bool>("TimelapseEnabled");
        private static readonly PropertyKey<int> TimelapseFrequencyKey = new PropertyKey<int>("TimelapseFrequency");
        private static readonly PropertyKey<int> LastScreenshotDayKey = new PropertyKey<int>("LastScreenshotDay");
        private static readonly PropertyKey<int> LastScreenshotHourKey = new PropertyKey<int>("LastScreenshotHour");

        public struct IngameDateTime {
            public int day;
            public int hour;

            public IngameDateTime(int day, int hour) {
                this.day = day;
                this.hour = hour;
            }

            public IngameDateTime(IDayNightCycle timeService) {
                this.day = timeService.DayNumber;
                this.hour = (int)timeService.HoursPassedToday;
            }

            public void addHours(int hours) {
                if (hours >= 24) {
                    addDays(hours / 24);
                    hours = hours % 24;
                }

                if (hour + hours >= 24) {
                    hour += hours - 24;
                    day += 1;
                } else {
                    hour += hours;
                }
            }

            public void addDays(int days) {
                day += days;
            }

            public override string ToString() {
                return "[day:" + day.ToString() + "; hour:" + hour.ToString() + "]";
            }

            public static bool operator<(IngameDateTime l, IngameDateTime r) {
                return l.day < r.day || (l.day == r.day && l.hour < r.hour);
            }
            public static bool operator>(IngameDateTime l, IngameDateTime r) {
                return l.day > r.day || (l.day == r.day && l.hour > r.hour);
            }

            public static bool operator<=(IngameDateTime l, IngameDateTime r) {
                return !(l > r);
            }
            public static bool operator>=(IngameDateTime l, IngameDateTime r) {
                return !(l < r);
            }
        }
    }
}