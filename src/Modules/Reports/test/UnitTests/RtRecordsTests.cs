using System;
using FluentAssertions;
using Reports.Domain.TransmissionFormat;
using Reports.Domain.TransmissionFormat.Records;
using Xunit;

namespace Reports.test.UnitTests;

public class RtRecordsTests
{
    [Fact]
    public void Rt1Record_Should_Format_Correctly()
    {
        var header = new Rt1Header("5", "123456", "CLAVE12345");
        var date = new DateTime(2025, 2, 6);
        var line = new Rt1Record(header, date, 29).ToLine();

        line.Should().Be("00001105" + "123456" + "06022025" + "00029" + "CLAVE12345" + "01" + "17");
    }

    [Fact]
    public void Rt1Record_Should_Pad_EntityCode_To_6_Chars()
    {
        var header = new Rt1Header("5", "123", "KEY");
        var date = new DateTime(2025, 2, 6);
        var line = new Rt1Record(header, date, 1).ToLine();

        line.Should().Be("00001105" + "000123" + "06022025" + "00001" + "KEY" + "01" + "17");
    }

    [Fact]
    public void Rt1Record_Should_Truncate_EntityCode_Over_6_Chars()
    {
        var header = new Rt1Header("5", "123456789", "KEY");
        var date = new DateTime(2025, 2, 6);
        var line = new Rt1Record(header, date, 1).ToLine();

        line.Should().Be("00001105" + "123456" + "06022025" + "00001" + "KEY" + "01" + "17");
    }

    [Fact]
    public void Rt2Record_Should_Format_Correctly()
    {
        var rt2 = new Rt2Record("1234");
        var line = rt2.ToLine(2);
        line.Should().Be("00002" + "2" + "0000" + "0" + "1" + "1" + "6" + "1234");
    }

    [Fact]
    public void Rt2Record_Should_Pad_PortfolioCode_To_4_Chars()
    {
        var rt2 = new Rt2Record("12");
        var line = rt2.ToLine(2);
        line.Should().Be("00002" + "2" + "0000" + "0" + "1" + "1" + "6" + "0012");
    }

    [Fact]
    public void Rt2Record_Should_Truncate_PortfolioCode_Over_4_Chars()
    {
        var rt2 = new Rt2Record("123456");
        var line = rt2.ToLine(2);
        line.Should().Be("00002" + "2" + "0000" + "0" + "1" + "1" + "6" + "1234");
    }

    [Fact]
    public void Rt5Record_Should_Format_Correctly()
    {
        var rt5 = new Rt5Record();
        var line = rt5.ToLine(29);
        line.Should().Be("00029" + "5");
    }

    [Fact]
    public void Rt4_UnitValue_Should_Format_Correctly()
    {
        var line = Rt4Lines.UnitValue(100.123456m).ToLine(3);
        line.Should().Be("00003" + "4312" + "0101011" + "+" + "0000000000100.123456");
    }

    [Fact]
    public void Rt4_313_Yield_Negative_Should_Show_Minus_Sign()
    {
        var line = Rt4Lines.YieldAmount(-1000.50m).ToLine(6);
        line.Should().Be("00006" + "4313" + "0202005" + "-" + "00000000000001000.50");
    }

    [Fact]
    public void Rt4_314_Returns_Should_Format_Correctly()
    {
        var line30 = Rt4Lines.Return30Days(10.12m).ToLine(20);
        var line180 = Rt4Lines.Return180Days(20.34m).ToLine(21);
        var line365 = Rt4Lines.Return365Days(30.56m).ToLine(22);

        line30.Should().Be("00020" + "4314" + "0101005" + "+" + "00000000000000010.12");
        line180.Should().Be("00021" + "4314" + "0101010" + "+" + "00000000000000020.34");
        line365.Should().Be("00022" + "4314" + "0101015" + "+" + "00000000000000030.56");
    }

    [Fact]
    public void Rt4_314_Returns_Negative_Should_Still_Show_Plus_And_Abs()
    {
        var line30 = Rt4Lines.Return30Days(-10.12m).ToLine(20);
        var line180 = Rt4Lines.Return180Days(-20.34m).ToLine(21);
        var line365 = Rt4Lines.Return365Days(-30.56m).ToLine(22);

        line30.Should().Be("00020" + "4314" + "0101005" + "+" + "00000000000000010.12");
        line180.Should().Be("00021" + "4314" + "0101010" + "+" + "00000000000000020.34");
        line365.Should().Be("00022" + "4314" + "0101015" + "+" + "00000000000000030.56");
    }
}
