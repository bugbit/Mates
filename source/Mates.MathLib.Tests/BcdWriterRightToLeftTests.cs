using Xunit;

namespace Mates.MathLib.Tests;

public class BcdWriterRightToLeftTests
{
    [Theory]
    [InlineData(-1)]
    [InlineData(10)]
    [InlineData(42)]
    public void Add_GivenDigitOutOfRange_ThenThrowsArgumentOutOfRange(int invalidDigit)
    {
        // Arrange
        var sut = new BcdWriterRightToLeft();

        // Act + Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => sut.Add(invalidDigit));
    }

    [Fact]
    public void ToBcd_GivenNoDigits_ThenReturnsEmptyBcd()
    {
        // Arrange
        var sut = new BcdWriterRightToLeft();

        // Act
        var bcd = sut.ToBcd();

        // Assert
        // SUP: Bcd expone Digits y Bytes
        Assert.NotNull(bcd);
        Assert.Equal(0, GetDigits(bcd));
        Assert.Empty(GetBytes(bcd));
    }

    [Theory(DisplayName = "Empaquetado par: bytes se invierten (Right-To-Left) y nibbles alto/bajo correctos")]
    [MemberData(nameof(EvenDigitsCases))]
    public void ToBcd_GivenEvenDigits_ThenPacksPairsAndReverses((int[] digits, byte[] expectedBytes) data)
    {
        // Arrange
        var sut = new BcdWriterRightToLeft();
        foreach (var d in data.digits)
            sut.Add(d);

        // Act
        var bcd = sut.ToBcd();

        // Assert
        Assert.Equal(data.digits.Length, GetDigits(bcd));
        Assert.Equal(data.expectedBytes, GetBytes(bcd));
    }

    [Theory(DisplayName = "Empaquetado impar: último nibble bajo a 0 y bytes invertidos")]
    [MemberData(nameof(OddDigitsCases))]
    public void ToBcd_GivenOddDigits_ThenPadsLowNibbleZeroAndReverses((int[] digits, byte[] expectedBytes) data)
    {
        // Arrange
        var sut = new BcdWriterRightToLeft();
        foreach (var d in data.digits)
            sut.Add(d);

        // Act
        var bcd = sut.ToBcd();

        // Assert
        Assert.Equal(data.digits.Length, GetDigits(bcd));
        Assert.Equal(data.expectedBytes, GetBytes(bcd));
    }

    [Fact(DisplayName = "Secuencia larga: consistencia de índices y flip-flop de nibble")]
    public void ToBcd_GivenManyDigits_ThenPacksAllCorrectly()
    {
        // Arrange
        var digits = new[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        var sut = new BcdWriterRightToLeft();
        foreach (var d in digits)
            sut.Add(d);

        // Internamente: [0x12, 0x34, 0x56, 0x78] -> Reverse -> [0x78, 0x56, 0x34, 0x12]
        var expected = new byte[] { 0x78, 0x56, 0x34, 0x12 };

        // Act
        var bcd = sut.ToBcd();

        // Assert
        Assert.Equal(digits.Length, GetDigits(bcd));
        Assert.Equal(expected, GetBytes(bcd));
    }

    // ===== Datos de prueba =====

    public static IEnumerable<object[]> EvenDigitsCases()
    {
        // Caso simple 2 dígitos: [1,2] -> interno [0x12] -> Reverse = [0x12]
        yield return new object[] { (new[] { 1, 2 }, new byte[] { 0x12 }) };

        // 4 dígitos: interno [0x12, 0x34] -> Reverse [0x34, 0x12]
        yield return new object[] { (new[] { 1, 2, 3, 4 }, new byte[] { 0x34, 0x12 }) };

        // 6 dígitos con ceros: [0,9,0,0,0,1] -> interno [0x09,0x00,0x01] -> Reverse [0x01,0x00,0x09]
        yield return new object[] { (new[] { 0, 9, 0, 0, 0, 1 }, new byte[] { 0x01, 0x00, 0x09 }) };
    }

    public static IEnumerable<object[]> OddDigitsCases()
    {
        // 1 dígito: [5] -> interno [0x50] -> Reverse [0x50]
        yield return new object[] { (new[] { 5 }, new byte[] { 0x50 }) };

        // 3 dígitos: [9,8,7] -> interno [0x98,0x70] -> Reverse [0x70,0x98]
        yield return new object[] { (new[] { 9, 8, 7 }, new byte[] { 0x70, 0x98 }) };

        // 5 dígitos todos cero: [0,0,0,0,0] -> interno [0x00,0x00,0x00] -> Reverse igual
        yield return new object[] { (new[] { 0, 0, 0, 0, 0 }, new byte[] { 0x00, 0x00, 0x00 }) };
    }

    // ===== Helpers para desacoplar de la implementación concreta de Bcd =====
    // Ajusta estas funciones si tu Bcd expone otros miembros (p. ej., .AsSpan(), .Count, etc.)

    private static IEnumerable<byte> GetBytes(object bcd)
    {
        // Intento propiedades comunes: Bytes, Data, Value
        var type = bcd.GetType();
        var prop = type.GetProperty("Bytes") ?? type.GetProperty("Data") ?? type.GetProperty("Value");
        if (prop == null)
            throw new InvalidOperationException("No se encontró una propiedad de bytes en Bcd (esperaba Bytes/Data/Value). Ajusta el test.");
        var val = prop.GetValue(bcd);
        return (val as IEnumerable<byte>) ?? ((Array)val).Cast<byte>();
    }

    private static int GetDigits(object bcd)
    {
        var type = bcd.GetType();
        var prop = type.GetProperty("Digits") ?? type.GetProperty("Length") ?? type.GetProperty("Count");
        if (prop == null)
            throw new InvalidOperationException("No se encontró una propiedad de dígitos en Bcd (esperaba Digits/Length/Count). Ajusta el test.");
        return (int)Convert.ChangeType(prop.GetValue(bcd), typeof(int));
    }
}
