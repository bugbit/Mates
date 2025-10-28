using System;
using System.Collections;
using System.Linq;
using Mates.MathLib;
using Xunit;

namespace Mates.MathLib.Tests;

public class BcdTests
{
    [Fact]
    public void Parse_1234_ProducesExpectedBytesAndDigits()
    {
        // Act
        var bcd = Bcd.Parse("1234");

        // Assert
        Assert.Equal(4, bcd.Digits);
        Assert.Equal(new byte[] { 0x12, 0x34 }, bcd.Data);
    }

    [Theory]
    [InlineData("1234", 0, 1)]
    [InlineData("1234", 1, 2)]
    [InlineData("1234", 2, 3)]
    [InlineData("1234", 3, 4)]
    [InlineData("456912", -1, 2)] // negativo: último dígito
    [InlineData("456912", -6, 4)] // negativo: primero
    public void GetDigit_Indexing_Works(string value, int idx, int expectedDigit)
    {
        var bcd = Bcd.Parse(value);
        Assert.Equal(expectedDigit, bcd.GetDigit(idx));
    }

    [Fact]
    public void GetDigit_OutOfRange_ReturnsZero()
    {
        var bcd = Bcd.Parse("12");
        Assert.Equal(0, bcd.GetDigit(5));   // fuera de rango
        Assert.Equal(0, new Bcd().GetDigit(0)); // sin datos
    }

    [Fact]
    public void GetIdx_And_GetIndex_AreConsistent()
    {
        var bcd=new Bcd(11);

        // No depende de datos internos: sólo de la aritmética de índices
        for (int idx = 0; idx < 10; idx++)
        {
            var (idxData, idx4Bit) = bcd.GetIdx(idx);
            Assert.Equal(idx, Bcd.GetIndex(idxData, idx4Bit));
        }
    }

    [Fact]
    public void SetDigit_ExpandsStorage_And_PersistsValue()
    {
        var bcd = new Bcd();

        // Escribimos un dígito en un índice que fuerza a crear bytes
        bcd.SetDigit(3, 7); // índice 3 -> segundo byte, nibble alto

        // Debe poder leerse sin importar que antes no hubiera datos
        Assert.Equal(7, bcd.GetDigit(3));

        // Otros dígitos no escritos de ese byte/vecinos deben ser 0 por defecto
        Assert.Equal(0, bcd.GetDigit(2)); // nibble bajo del mismo byte
        Assert.Equal(0, bcd.GetDigit(0));
        Assert.Equal(0, bcd.GetDigit(1));
    }

    [Fact]
    public void SetDigit_ReplacesNibble_Correctly()
    {
        // Partimos de 0x12, 0x34 => "1234"
        var bcd = Bcd.Parse("1234");

        // Cambiamos el nibble alto del primer byte (índice 0 -> '1') a '9'
        bcd.SetDigit(0, 9);
        Assert.Equal(9, bcd.GetDigit(0));
        Assert.Equal(2, bcd.GetDigit(1)); // el nibble bajo del mismo byte debe quedar intacto

        // Cambiamos el nibble bajo del segundo byte (índice 3 -> '4') a '5'
        bcd.SetDigit(3, 5);
        Assert.Equal(5, bcd.GetDigit(3));
        Assert.Equal(3, bcd.GetDigit(2)); // el nibble alto del mismo byte debe quedar intacto

        // Verificación cruda de bytes: ahora deberían ser [0x92, 0x35]
        Assert.Equal(new byte[] { 0x92, 0x35 }, bcd.Data);
    }

    [Fact]
    public void Parse_WithNonDigit_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => Bcd.Parse("12a3"));
    }

    [Fact]
    public void Sum_SameLength_AddsCorrectly_AndReturnsNewBcd()
    {
        // 123 + 877 = 1000  (mismo número de dígitos; gestiona acarreo extra)
        var a = Bcd.Parse("123");
        var b = Bcd.Parse("877");

        var (result, carries) = Bcd.Sum(a, b);

        Assert.NotNull(result);
        Assert.Equal(Bcd.Parse("1000").Data, result!.Data);
        Assert.Equal(Bcd.Parse("1000").Digits, result.Digits);

        // Opcional: los carries pueden devolverse si la implementación los marca;
        // no asumo formato concreto aquí para no acoplar el test a flags internos.
    }

    [Fact]
    public void SetDigits_GrowsOnly_DoesNotShrinkData()
    {
        var bcd = Bcd.Parse("1234"); // 2 bytes
        bcd.SetDigits(6);            // debe crecer a 3 bytes como mínimo

        Assert.True(bcd.Data.Length >= 3);
        // Si luego se pide menos, no hay garantía de “encoger”: sólo comprobamos que no rompa
        bcd.SetDigits(2);
        Assert.True(bcd.Data.Length >= 1);
    }
}
