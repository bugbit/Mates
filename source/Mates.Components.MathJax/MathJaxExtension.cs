using Markdig;
using Markdig.Extensions.Mathematics;
using Markdig.Renderers;

namespace Mates.Components.MathJax;

public class MathJaxExtension : IMarkdownExtension
{
    public void Setup(MarkdownPipelineBuilder pipeline)
    {
        // Adds the inline parser
        if (!pipeline.InlineParsers.Contains<MathInlineParser>())
        {
            pipeline.InlineParsers.Insert(0, new MathInlineParser());
        }

        // Adds the block parser
        if (!pipeline.BlockParsers.Contains<MathBlockParser>())
        {
            // Insert before EmphasisInlineParser to take precedence
            pipeline.BlockParsers.Insert(0, new MathBlockParser());
        }
    }

    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
        if (renderer is HtmlRenderer htmlRenderer)
        {
            if (!htmlRenderer.ObjectRenderers.Contains<HtmlMathJaxInlineRenderer>())
            {
                htmlRenderer.ObjectRenderers.Insert(0, new HtmlMathJaxInlineRenderer());
            }
            if (!htmlRenderer.ObjectRenderers.Contains<HtmlMathJaxBlockRenderer>())
            {
                htmlRenderer.ObjectRenderers.Insert(0, new HtmlMathJaxBlockRenderer());
            }
        }
    }
}
