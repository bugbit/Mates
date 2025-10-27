namespace Mates.Math.Tests;

using System;
using System.Linq;
using Mates.Math;
using Xunit;
using FluentAssertions;

public class BcdTests
{
    [Fact]
    public void Constructor_DeberiaCrearListaVacia()
    {
        var bcd = new Bcd();

        bcd.Data.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0, 0, 1)]
    [InlineData(1, 0, 0)]
    [InlineData(2, 1, 1)]
    [InlineData(3, 1, 0)]
    [InlineData(-1, 0, 0)] // negativo
    [InlineData(-2, 0, 1)] // negativo par
    public void GetIdx_DeberiaCalcularCorrectamente(int idx, int esperadoIdxData, int esperadoIdx4Bit)
    {
        var bcd = new Bcd();

        // Prepara data para el caso de negativos
        bcd.SetDigit(0, 1);
        bcd.SetDigit(1, 1);

        var (idxData, idx4Bit) = bcd.GetIdx(idx);

        idxData.Should().Be(esperadoIdxData);
        idx4Bit.Should().Be(esperadoIdx4Bit);
    }

    [Theory]
    [InlineData(0, 5)]
    [InlineData(1, 9)]
    [InlineData(2, 3)]
    [InlineData(3, 7)]
    public void SetDigit_DeberiaAgregarODefinirDigitosCorrectamente(int idx, int digit)
    {
        var bcd = new Bcd();

        bcd.SetDigit(idx, digit);

        // Verificamos que el Data tenga al menos el byte esperado
        var data = bcd.Data;
        data.Should().NotBeEmpty();

        var (idxData, idxRem) = Math.DivRem(Math.Abs(idx), 2);
        var valorGuardado = data[idxData];
        var nibble = (valorGuardado >> ((1 - idxRem) * 4)) & 0xF;

        nibble.Should().Be(digit);
    }

    [Fact]
    public void SetDigit_DeberiaActualizarDigitoExistenteAumentarLista()
    {
        var bcd = new Bcd();
        bcd.SetDigit(0, 3);
        bcd.Data.Length.Should().Be(1);

        bcd.SetDigit(1, 8);
        bcd.Data.Length.Should().Be(1); // mismo byte
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(10)]
    public void SetDigit_DeberiaLanzarExcepcionSiDigitoInvalido(int digit)
    {
        var bcd = new Bcd();

        Action act = () => bcd.SetDigit(0, digit);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Data_DeberiaDevolverCopiaYNoReferencia()
    {
        var bcd = new Bcd();
        bcd.SetDigit(0, 4);

        var data1 = bcd.Data;
        data1[0] = 0xFF; // modificar copia

        var data2 = bcd.Data;
        data2[0].Should().NotBe(0xFF); // original no debe cambiar
    }

    [Fact]
    public void GetDigit_ReturnsCorrectDigits()
    {
        // Arrange
        // Numero 1234
        var data = new List<byte> { 0x12, 0x34 };
        var bcd = new Bcd(data, digits: 4);

        // Act & Assert
        Assert.Equal(1, bcd.GetDigit(0)); // nibble bajo del primer byte
        Assert.Equal(2, bcd.GetDigit(1)); // nibble alto del primer byte
        Assert.Equal(3, bcd.GetDigit(2)); // nibble bajo del segundo byte
        Assert.Equal(4, bcd.GetDigit(3)); // nibble alto del segundo byte
    }

    [Fact]
    public void GetDigit_SupportsNegativeIndices()
    {
        // Arrange
        var data = new List<byte> { 0x12, 0x34 };
        var bcd = new Bcd(data, digits: 4);

        // Act & Assert
        Assert.Equal(4, bcd.GetDigit(-1)); // último dígito
        Assert.Equal(3, bcd.GetDigit(-2));
        Assert.Equal(2, bcd.GetDigit(-3));
        Assert.Equal(1, bcd.GetDigit(-4));
    }

    [Fact]
    public void GetDigit_Return0WhenIndexOutOfRange()
    {
        // Arrange
        var data = new List<byte> { 0x12 };
        var bcd = new Bcd(data, digits: 2);

        // Act & Assert
        Assert.Equal(0, bcd.GetDigit(5));
        Assert.Equal(0, bcd.GetDigit(-3));
    }
}

