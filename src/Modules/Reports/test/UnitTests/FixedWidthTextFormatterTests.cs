using System;
using FluentAssertions;
using Common.SharedKernel.Core.Formatting;
using Xunit;

namespace Reports.test.UnitTests;

public class FixedWidthTextFormatterTests
{
    [Fact]
    public void FormatNumber_Truncates_And_Pads()
    {
        var text = FixedWidthTextFormatter.FormatNumber(100.1234567m, 20, 6);
        text.Should().Be("0000000000100.123456");
        text.Length.Should().Be(20);
    }

    [Fact]
    public void FormatNumberStrict_Throws_On_Overflow()
    {
        Action act = () => FixedWidthTextFormatter.FormatNumberStrict(12345678901234567890.12m, 20, 2);
        act.Should().Throw<OverflowException>();
    }

    [Fact]
    public void FormatDate_ddMMyyyy()
    {
        var text = FixedWidthTextFormatter.FormatDate(new DateTime(2025, 2, 6));
        text.Should().Be("06022025");
    }
}
