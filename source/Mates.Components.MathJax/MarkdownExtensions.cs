using Markdig;

namespace Mates.Components.MathJax;

public static class MarkdownExtensions
{
    /// <summary>
    /// Uses the math extension.
    /// </summary>
    /// <param name="pipeline">The pipeline.</param>
    /// <returns>The modified pipeline</returns>
    public static MarkdownPipelineBuilder UseMathJax(this MarkdownPipelineBuilder pipeline)
    {
        pipeline.Extensions.AddIfNotAlready<MathJaxExtension>();
        return pipeline;
    }
}
