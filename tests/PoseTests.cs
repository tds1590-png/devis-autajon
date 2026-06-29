using DevisHp.Core;
using Xunit;

namespace DevisHp.Tests;

public class PoseTests
{
    [Fact]
    public void NbPosesLaize_WithGivenGoldenInputs_Returns3()
    {
        var n = PoseCalculator.NbPosesLaize(100, 4, 18, 330, false);
        Assert.Equal(3, n);
    }

    [Fact]
    public void NbPosesAvance_WithGivenGoldenInputs_Returns2()
    {
        var n = PoseCalculator.NbPosesAvance(150, 4, 320);
        Assert.Equal(2, n);
    }
}
