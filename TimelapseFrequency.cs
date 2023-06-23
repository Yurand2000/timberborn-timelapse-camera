namespace Yurand.Timberborn.TimelapseCamera
{
    public enum TimelapseFrequency
    {
        OneHour = 1,
        TwoHours,
        FourHours,
        SixHours,
        EightHours,
        TwelveHours,
        Daily,
        TwoDays,
        EachCycle,
    }

    public static class TimelapseFrequencyHelpers
    {
        public static string LocalizedString(TimelapseFrequency frequency) {
            switch (frequency) {
                case TimelapseFrequency.OneHour: return "yurand.timelapsecamera.frequency.1h";
                case TimelapseFrequency.TwoHours: return "yurand.timelapsecamera.frequency.2h";
                case TimelapseFrequency.FourHours: return "yurand.timelapsecamera.frequency.4h";
                case TimelapseFrequency.SixHours: return "yurand.timelapsecamera.frequency.6h";
                case TimelapseFrequency.EightHours: return "yurand.timelapsecamera.frequency.8h";
                case TimelapseFrequency.TwelveHours: return "yurand.timelapsecamera.frequency.12h";
                case TimelapseFrequency.Daily: return "yurand.timelapsecamera.frequency.1day";
                case TimelapseFrequency.TwoDays: return "yurand.timelapsecamera.frequency.2day";
                case TimelapseFrequency.EachCycle: return "yurand.timelapsecamera.frequency.1cycle";
                default: return "";
            }
        }

        public static TimelapseFrequency Default() {
            return TimelapseFrequency.Daily;
        }

        public static int ToInt(TimelapseFrequency frequency) {
            return (int)frequency;
        }

        public static TimelapseFrequency FromInt(int frequency) {
            if (frequency >= FrequencyMin && frequency <= FrequencyMax)
                return (TimelapseFrequency)frequency;
            else
                return Default();
        }
        public const int FrequencyMin = (int)(TimelapseFrequency.OneHour);
        public const int FrequencyMax = (int)(TimelapseFrequency.TwoDays);
    }
}