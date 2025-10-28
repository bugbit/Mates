using Xunit;

namespace Mates.MathLib.Tests;

public class BcdWriterTests
{

    [Fact]
    public void ToBcd_ShouldReturnDataInBuildOrder()
    {
        using var w = new BcdWriter();
        // Add: 2,1,9,0,5,4  => _data interno: [0x21, 0x90, 0x54]
        w.Add(2); w.Add(1); w.Add(9); w.Add(0); w.Add(5); w.Add(4);

        var bcd = w.ToBcd();

        Assert.Equal(new byte[] { 0x21, 0x90, 0x54 }, bcd.Data);
        Assert.Equal(6, bcd.Digits);
    }

    [Fact]
    public void ToBcdReverse_ShouldReverseDigits_NotJustBytes()
    {
        using var w = new BcdWriter();
        // Interno: [0x21, 0x90, 0x54] (6 dígitos: 2,1,9,0,5,4)
        w.Add(2); w.Add(1); w.Add(9); w.Add(0); w.Add(5); w.Add(4);

        // Invertir dígito a dígito: [4,5,0,9,1,2] => bytes [0x45, 0x09, 0x12]
        var bcd = w.ToBcdReverse();

        Assert.Equal(new byte[] { 0x45, 0x09, 0x12 }, bcd.Data);
        Assert.Equal(6, bcd.Digits);

        // Asegura que NO es una simple inversión de bytes ([0x54,0x90,0x21])
        Assert.NotEqual(new byte[] { 0x54, 0x90, 0x21 }, bcd.Data);
    }

    [Fact]
    public void ToBcdReverse_OddDigits_ShouldPadLowNibbleWithZero()
    {
        using var w = new BcdWriter();
        // Dígitos: 1,2,3,4,5  -> interno: [0x12, 0x34, 0x50] (nibble bajo último = 0)
        w.Add(1); w.Add(2); w.Add(3); w.Add(4); w.Add(5);

        // Reversa: [5,4,3,2,1] -> [0x54, 0x32, 0x10]
        var bcd = w.ToBcdReverse();

        Assert.Equal(new byte[] { 0x54, 0x32, 0x10 }, bcd.Data);
        Assert.Equal(5, bcd.Digits);
    }

    [Fact]
    public void Add_ShouldThrow_OnDigitOutOfRange()
    {
        using var w = new BcdWriter();
        Assert.Throws<ArgumentOutOfRangeException>(() => w.Add(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => w.Add(10));
    }

    [Theory]
    [InlineData("1234", new byte[] { 0x12, 0x34 }, 4)]
    [InlineData("987654", new byte[] { 0x98, 0x76, 0x54 }, 6)]
    public void Parse_ShouldMatchExpectedBytes(string input, byte[] expectedBytes, int expectedDigits)
    {
        // Bcd.Parse usa internamente BcdWriter.Add y ToBcd()
        var bcd = Bcd.Parse(input);

        Assert.Equal(expectedBytes, bcd.Data);
        Assert.Equal(expectedDigits, bcd.Digits);
    }
}
