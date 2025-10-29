using System.Collections;

namespace Mates.MathLib.Number;

public class NaturalNumber
{
    private readonly Bcd _bcd;

    public NaturalNumber(Bcd bcd) => _bcd = bcd;

    public static NaturalNumber Parse(string value)
    {
        return new NaturalNumber(Bcd.Parse(value));
    }

    public static (bool ok, NaturalNumber? number) TryParse(string value)
    {
        var (ok, bcd) = Bcd.TryParse(value);

        return (ok, ok ? new NaturalNumber(bcd!) : null);
    }

    public static (NaturalNumber result, BitArray? carries) SumWithCarries(NaturalNumber n1, NaturalNumber n2)
    {
        var (result, carries) = Bcd.Sum(n1._bcd, n2._bcd, BcdOperationFlags.ReturnCarries);

        if (result is null)
            throw new NullReferenceException(nameof(result));

        return (new NaturalNumber(result), carries);
    }
}
