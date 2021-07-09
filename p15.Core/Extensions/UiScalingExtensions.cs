namespace p15.Core.Extensions
{
    public enum FontSizes : int
    {
        Text = 12,
        MediumText = 14,
        LargeText = 16,
        MenuItem = 12,
        PackageButton = 20,
        AppViewButton = 10
    }

    public static class UiScalingExtensions
    {
        public static int Scale(this int defaultFontSize, int uiScale)
        {
            return (int)(defaultFontSize * (uiScale / 100.0));
        }

        public static int Scale(this FontSizes defaultFontSize, int uiScale)
        {
            return (int)((int)defaultFontSize * (uiScale / 100.0));
        }
    }
}
