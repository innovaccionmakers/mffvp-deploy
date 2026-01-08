using System;
using System.Collections.Generic;
using FluentAssertions;
using Reports.Domain.TransmissionFormat;
using Reports.Domain.TransmissionFormat.Layout;
using Reports.Domain.TransmissionFormat.Records;
using Xunit;

namespace Reports.test.UnitTests;

public class RtRecordsTests
{
    public static IEnumerable<object[]> MovementsWithPositiveSignData()
    {
        yield return new object[] { (Func<decimal, Rt4NumberLine>)Rt4Lines.TransferUnits, TransmissionFormatLayout.Rt4.TransferUnits, 10.123456m };
        yield return new object[] { (Func<decimal, Rt4NumberLine>)Rt4Lines.TransferAmount, TransmissionFormatLayout.Rt4.TransferAmount, 10.12m };
        yield return new object[] { (Func<decimal, Rt4NumberLine>)Rt4Lines.PensionUnits, TransmissionFormatLayout.Rt4.PensionUnits, 5.654321m };
        yield return new object[] { (Func<decimal, Rt4NumberLine>)Rt4Lines.PensionAmount, TransmissionFormatLayout.Rt4.PensionAmount, 5.67m };
        yield return new object[] { (Func<decimal, Rt4NumberLine>)Rt4Lines.WithdrawalUnits, TransmissionFormatLayout.Rt4.WithdrawalUnits, 8.765432m };
        yield return new object[] { (Func<decimal, Rt4NumberLine>)Rt4Lines.WithdrawalAmount, TransmissionFormatLayout.Rt4.WithdrawalAmount, 8.9m };
        yield return new object[] { (Func<decimal, Rt4NumberLine>)Rt4Lines.OtherCommissionUnits, TransmissionFormatLayout.Rt4.OtherCommissionUnits, 4.321098m };
        yield return new object[] { (Func<decimal, Rt4NumberLine>)Rt4Lines.OtherCommissionAmount, TransmissionFormatLayout.Rt4.OtherCommissionAmount, 4.32m };
        yield return new object[] { (Func<decimal, Rt4NumberLine>)Rt4Lines.VitalityTransferUnits, TransmissionFormatLayout.Rt4.VitalityTransferUnits, 3.210987m };
        yield return new object[] { (Func<decimal, Rt4NumberLine>)Rt4Lines.VitalityTransferAmount, TransmissionFormatLayout.Rt4.VitalityTransferAmount, 3.21m };
        yield return new object[] { (Func<decimal, Rt4NumberLine>)Rt4Lines.OtherWithdrawalUnits, TransmissionFormatLayout.Rt4.OtherWithdrawalUnits, 2.109876m };
        yield return new object[] { (Func<decimal, Rt4NumberLine>)Rt4Lines.OtherWithdrawalAmount, TransmissionFormatLayout.Rt4.OtherWithdrawalAmount, 2.1m };
    }

    public static IEnumerable<object[]> CancellationLinesData()
    {
        yield return new object[] { (Func<decimal, Rt4NumberLine>)Rt4Lines.CancellationUnits, TransmissionFormatLayout.Rt4.CancellationUnits, 10.123456m };
        yield return new object[] { (Func<decimal, Rt4NumberLine>)Rt4Lines.CancellationAmount, TransmissionFormatLayout.Rt4.CancellationAmount, 10.12m };
    }
    
    [Fact]
    public void Rt1Record_Should_Format_Correctly()
    {
        var header = new Rt1Header("5", "123456", "CLAVE12345");
        var date = new DateTime(2025, 2, 6);
        var line = new Rt1Record(header, date, 29).ToLine();

        line.Should().Be("000000011005" + "123456" + "06022025" + "00000029" + "CLAVE12345" + "01" + "17");
    }

    [Fact]
    public void Rt1Record_Should_Pad_EntityCode_To_6_Chars()
    {
        var header = new Rt1Header("5", "123", "KEY");
        var date = new DateTime(2025, 2, 6);
        var line = new Rt1Record(header, date, 1).ToLine();

        line.Should().Be("000000011005" + "000123" + "06022025" + "00000001" + "KEY" + "01" + "17");
    }

    [Fact]
    public void Rt1Record_Should_Truncate_EntityCode_Over_6_Chars()
    {
        var header = new Rt1Header("5", "123456789", "KEY");
        var date = new DateTime(2025, 2, 6);
        var line = new Rt1Record(header, date, 1).ToLine();

        line.Should().Be("000000011005" + "123456" + "06022025" + "00000001" + "KEY" + "01" + "17");
    }

    [Fact]
    public void Rt2Record_Should_Format_Correctly()
    {
        var rt2 = new Rt2Record("1234");
        var line = rt2.ToLine(2);
        line.Should().Be("00000002" + "2" + "0000" + "0" + "1" + "1" + "06" + "001234");
    }

    [Fact]
    public void Rt2Record_Should_Pad_PortfolioCode_To_4_Chars()
    {
        var rt2 = new Rt2Record("12");
        var line = rt2.ToLine(2);
        line.Should().Be("00000002" + "2" + "0000" + "0" + "1" + "1" + "06" + "000012");
    }

    [Fact]
    public void Rt2Record_Should_Truncate_PortfolioCode_Over_4_Chars()
    {
        var rt2 = new Rt2Record("123456");
        var line = rt2.ToLine(2);
        line.Should().Be("00000002" + "2" + "0000" + "0" + "1" + "1" + "06" + "123456");
    }

    [Fact]
    public void Rt5Record_Should_Format_Correctly()
    {
        var rt5 = new Rt5Record();
        var line = rt5.ToLine(29);
        line.Should().Be("00000029" + "5");
    }

    [Fact]
    public void Rt4_UnitValue_Should_Format_Correctly()
    {
        var line = Rt4Lines.UnitValue(100.123456m).ToLine(3);
        line.Should().Be("00000003" + "4312" + "0101011" + "+" + "0000000000100.123456");
    }

    [Fact]
    public void Rt4_313_Yield_Negative_Should_Show_Minus_Sign()
    {
        var line = Rt4Lines.YieldAmount(-1000.50m).ToLine(6);
        line.Should().Be("00000006" + "4313" + "0202005" + "-" + "00000000000001000.50");
    }

    [Fact]
    public void Rt4_314_Returns_Should_Format_Correctly()
    {
        var line30 = Rt4Lines.Return30Days(10.12m).ToLine(20);
        var line180 = Rt4Lines.Return180Days(20.34m).ToLine(21);
        var line365 = Rt4Lines.Return365Days(30.56m).ToLine(22);

        line30.Should().Be("00000020" + "4314" + "0101005" + "+" + "00000000000000010.12");
        line180.Should().Be("00000021" + "4314" + "0101010" + "+" + "00000000000000020.34");
        line365.Should().Be("00000022" + "4314" + "0101015" + "+" + "00000000000000030.56");
    }

    [Fact]
    public void Rt4_314_Returns_Negative_Should_Show_Minus_And_Value()
    {
        var line30 = Rt4Lines.Return30Days(-10.12m).ToLine(20);
        var line180 = Rt4Lines.Return180Days(-20.34m).ToLine(21);
        var line365 = Rt4Lines.Return365Days(-30.56m).ToLine(22);

        line30.Should().Be("00000020" + "4314" + "0101005" + "-" + "00000000000000010.12");
        line180.Should().Be("00000021" + "4314" + "0101010" + "-" + "00000000000000020.34");
        line365.Should().Be("00000022" + "4314" + "0101015" + "-" + "00000000000000030.56");
    }
    
    [Theory]
    [MemberData(nameof(MovementsWithPositiveSignData))]
    public void Rt4_313_Movements_Should_Always_Show_Positive_Sign(
        Func<decimal, Rt4NumberLine> lineFactory,
        string expectedCode,
        decimal sampleValue)
    {
        const int recordNumber = 10;

        var positiveLine = lineFactory(sampleValue).ToLine(recordNumber);
        var negativeLine = lineFactory(-sampleValue).ToLine(recordNumber);
        var zeroLine = lineFactory(0m).ToLine(recordNumber);

        var signIndex = 8 + TransmissionFormatLayout.Rt4.R4313.Length + expectedCode.Length;
        positiveLine[signIndex].Should().Be('+');
        negativeLine[signIndex].Should().Be('+');
        zeroLine[signIndex].Should().Be('+');

        negativeLine.Should().Be(positiveLine);

        var expectedSegment = $"{TransmissionFormatLayout.Rt4.R4313}{expectedCode}+";
        positiveLine.Should().Contain(expectedSegment);
        negativeLine.Should().Contain(expectedSegment);
        zeroLine.Should().Contain(expectedSegment);
    }

    [Theory]
    [MemberData(nameof(CancellationLinesData))]
    public void Rt4_313_Cancellation_Should_Show_Negative_Sign_When_Not_Zero_And_Positive_When_Zero(
        Func<decimal, Rt4NumberLine> lineFactory,
        string expectedCode,
        decimal sampleValue)
    {
        const int recordNumber = 10;

        var positiveLine = lineFactory(sampleValue).ToLine(recordNumber);
        var negativeLine = lineFactory(-sampleValue).ToLine(recordNumber);
        var zeroLine = lineFactory(0m).ToLine(recordNumber);

        var signIndex = 8 + TransmissionFormatLayout.Rt4.R4313.Length + expectedCode.Length;
        
        positiveLine[signIndex].Should().Be('-');
        negativeLine[signIndex].Should().Be('-');
        
        zeroLine[signIndex].Should().Be('+');

        negativeLine.Should().Be(positiveLine);

        var expectedNonZeroSegment = $"{TransmissionFormatLayout.Rt4.R4313}{expectedCode}-";
        var expectedZeroSegment = $"{TransmissionFormatLayout.Rt4.R4313}{expectedCode}+";
        positiveLine.Should().Contain(expectedNonZeroSegment);
        negativeLine.Should().Contain(expectedNonZeroSegment);
        zeroLine.Should().Contain(expectedZeroSegment);
    }
}
