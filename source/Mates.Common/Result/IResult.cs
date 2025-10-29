namespace Mates.Common.Result;

public interface IResult
{
    void Accept(IResultVisitor visitor);
}
