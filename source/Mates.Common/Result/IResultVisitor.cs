namespace Mates.Common.Result;

public interface IResultVisitor
{
    void Visit(LaTeXResult result);
}
