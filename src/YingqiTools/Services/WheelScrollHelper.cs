namespace YingqiTools.Services;

internal static class WheelScrollHelper
{
    internal const double PixelsPerDetent = 48d;
    private const double DeltaPerDetent = 120d;

    internal static double GetTargetOffset(double currentOffset, double scrollableHeight, int wheelDelta)
    {
        if (!double.IsFinite(currentOffset) || !double.IsFinite(scrollableHeight) || scrollableHeight <= 0) return 0;
        double normalizedOffset = Math.Clamp(currentOffset, 0, scrollableHeight);
        if (wheelDelta == 0) return normalizedOffset;
        double targetOffset = normalizedOffset - (wheelDelta / DeltaPerDetent * PixelsPerDetent);
        return Math.Clamp(targetOffset, 0, scrollableHeight);
    }
}
