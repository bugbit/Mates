using Markdig;

namespace Mates.Components.CSharpMath;

public static class MarkdownExtensions
{
    /// <summary>
    /// Uses the math extension.
    /// </summary>
    /// <param name="pipeline">The pipeline.</param>
    /// <returns>The modified pipeline</returns>
    public static MarkdownPipelineBuilder UseCSharpMath(this MarkdownPipelineBuilder pipeline)
    {
        pipeline.Extensions.AddIfNotAlready<CSharpMathExtension>();
        return pipeline;
    }
}
