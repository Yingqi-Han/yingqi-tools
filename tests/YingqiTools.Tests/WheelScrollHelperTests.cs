using YingqiTools.Services;
using Xunit;

namespace YingqiTools.Tests;

public sealed class WheelScrollHelperTests
{
    [Theory]
    [InlineData(0, 500, -120, 48)]
    [InlineData(100, 500, -120, 148)]
    [InlineData(100, 500, 120, 52)]
    [InlineData(100, 500, -60, 124)]
    public void WheelDelta_MovesAtControlledSpeed(double current, double maximum, int delta, double expected)
    {
        Assert.Equal(expected, WheelScrollHelper.GetTargetOffset(current, maximum, delta));
    }

    [Theory]
    [InlineData(10, 500, 120, 0)]
    [InlineData(480, 500, -120, 500)]
    public void WheelDelta_ClampsToScrollableRange(double current, double maximum, int delta, double expected)
    {
        Assert.Equal(expected, WheelScrollHelper.GetTargetOffset(current, maximum, delta));
    }

    [Fact]
    public void NoScrollableContent_ReturnsTop()
    {
        Assert.Equal(0, WheelScrollHelper.GetTargetOffset(100, 0, -120));
    }
}
