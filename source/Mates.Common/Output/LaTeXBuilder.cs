using System.Text;

namespace Mates.Common.Output;

public sealed class LaTeXBuilder
{
    private StringBuilder _builder = new StringBuilder();

    public override string ToString() => _builder.ToString();

    public LaTeXBuilder Text(string value)
    {
        _builder.Append(value);

        return this;
    }

    public LaTeXBuilder BlockIf(bool condition, Func<LaTeXBuilder, LaTeXBuilder> block) => condition ? block(this) : this;

    public LaTeXBuilder BeginEnv(string envName, string? options = null)
    {
        Text(@$"\begin{{{envName}}}").BlockIf(options is not null, l => l.Text($"{{{options}}}"));

        return this;
    }

    public LaTeXBuilder EndEnv(string envName) => Text(@$"\end{{{envName}}}");

    public LaTeXBuilder BeginArray(string? justification = null) => BeginEnv("array", justification);

    public LaTeXBuilder EndArray() => EndEnv("array");

    public LaTeXBuilder Array(string? justification, params Func<LaTeXBuilder, LaTeXBuilder>[] rows)
    {
        var thisBuilder = BeginArray(justification);

        for (var i = 0; i < rows.Length; i++)
        {
            var block = rows[i];

            thisBuilder = block(thisBuilder).BlockIf(i < rows.Length - 1, l => l.Text(@"\\"));
        }

        return thisBuilder.EndArray();
    }

    public LaTeXBuilder Command(string commandName, params Func<LaTeXBuilder, LaTeXBuilder>[] args)
    {
        var thisBuilder = Text(@$"\{commandName}");

        foreach (var arg in args)
        {
            thisBuilder = thisBuilder.Text("{");
            thisBuilder = arg(thisBuilder).Text("}");
        }

        return thisBuilder;
    }

    public LaTeXBuilder Phantom(params Func<LaTeXBuilder, LaTeXBuilder>[] args) => Command("phantom", args);

    public LaTeXBuilder HLine() => Command("hline");
}
