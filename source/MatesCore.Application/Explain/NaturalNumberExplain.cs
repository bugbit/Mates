using Mates.Common.Interfaces;
using Mates.Common.Output;
using Mates.Common.Result;
using Mates.MathLib.Number;

namespace MatesCore.Application.Explain;

public class NaturalNumberExplain : INaturalNumberExplain
{
    public IResult Sumar2Numeros(string num1, string num2)
    {
        var latex = new LaTeXBuilder();
        var n1 = NaturalNumber.Parse(num1);
        var n2 = NaturalNumber.Parse(num2);
        var (result, carries) = NaturalNumber.SumWithCarries(n1, n2);

        latex.Append(num1).Append(", ").Append(num2);

        return new LaTeXResult { LaTeX = latex.ToString() };
    }
}
