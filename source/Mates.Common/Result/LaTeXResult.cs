namespace Mates.Common.Result;

public class LaTeXResult : IResult
{
    public string? LaTeX { get; set; }

    public void Accept(IResultVisitor visitor)
    {
        visitor.Visit(this);
    }
}
