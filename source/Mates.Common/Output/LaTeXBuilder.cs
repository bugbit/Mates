using System.Text;

namespace Mates.Common.Output;

public sealed class LaTeXBuilder
{
    private StringBuilder _builder = new StringBuilder();

    public override string ToString() => _builder.ToString();

    public LaTeXBuilder Append(string value)
    {
        _builder.Append(value);

        return this;
    }
}
