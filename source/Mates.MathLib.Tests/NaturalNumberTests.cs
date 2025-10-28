using Mates.MathLib;
using Mates.MathLib.Number;
using System.Reflection;
using Xunit;

namespace Mates.MathLib.Tests;

public class NaturalNumberTests
{
    // -------- Helpers --------
    private static Bcd GetInnerBcd(NaturalNumber n)
    {
        var f = typeof(NaturalNumber).GetField("_bcd", BindingFlags.NonPublic | BindingFlags.Instance);
        return (Bcd)f!.GetValue(n)!;
    }

    private static string BcdToString(Bcd b)
    {
        var chars = Enumerable.Range(0, b.Digits).Select(i => (char)('0' + b.GetDigit(i)));
        return new string(chars.ToArray());
    }

    private static NaturalNumber N(string s) => new NaturalNumber(Bcd.Parse(s));

    // -------- Tests --------

    [Fact]
    public void SumWithCarries_Basic_NoOverflow_ReturnsCorrectResultAndCarries()
    {
        // 12 + 34 = 46
        var (result, carries) = NaturalNumber.SumWithCarries(N("12"), N("34"));

        var bcd = GetInnerBcd(result);
        Assert.Equal("46", BcdToString(bcd));

        // La implementación actual fija los acarreos por cada paso procesado
        Assert.NotNull(carries);
        Assert.Equal(2, carries!.Length);
        Assert.True(carries.Cast<bool>().All(x => x)); // comportamiento actual
    }

    [Fact]
    public void SumWithCarries_DifferentLengths_RightAlignedResult()
    {
        // 123 + 456789 = 456912
        var (result, carries) = NaturalNumber.SumWithCarries(N("123"), N("456789"));

        var bcd = GetInnerBcd(result);
        Assert.Equal("456912", BcdToString(bcd));

        Assert.NotNull(carries);
        Assert.Equal(6, carries!.Length);            // max(dígitos) de los operandos
        Assert.True(carries.Cast<bool>().All(x => x)); // comportamiento actual
    }

    [Fact]
    public void SumWithCarries_ZeroPlusZero_Works()
    {
        var (result, carries) = NaturalNumber.SumWithCarries(N("0"), N("0"));

        var bcd = GetInnerBcd(result);
        Assert.Equal("0", BcdToString(bcd));

        Assert.NotNull(carries);
        Assert.Equal(1, carries!.Length);
        Assert.True(carries.Cast<bool>().All(x => x)); // comportamiento actual
    }

    [Fact]
    public void SumWithCarries_FinalCarry_ThrowsDueToCarriesOutOfRange_CurrentImplementation()
    {
        // 999 + 1 = 1000
        // Con ReturnCarries, Bcd.Sum hoy intenta marcar carries[i-1] cuando i = count+1 (acarreo final),
        // lo que provoca ArgumentOutOfRangeException (bug conocido).
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            _ = NaturalNumber.SumWithCarries(N("999"), N("1"));
        });
    }
}
