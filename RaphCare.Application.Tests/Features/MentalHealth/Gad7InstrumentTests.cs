using RaphCare.Application.Features.MentalHealth;
using Xunit;

namespace RaphCare.Application.Tests.Features.MentalHealth;

public sealed class Gad7InstrumentTests
{
    [Theory]
    [InlineData(0, "Minimal")]
    [InlineData(4, "Minimal")]
    [InlineData(5, "Mild")]
    [InlineData(9, "Mild")]
    [InlineData(10, "Moderate")]
    [InlineData(14, "Moderate")]
    [InlineData(15, "Severe")]
    [InlineData(21, "Severe")]
    public void SeverityBandsMatchGad7Cutoffs(int total, string expected)
    {
        Assert.Equal(expected, Gad7Instrument.SeverityForTotal(total));
    }
}
