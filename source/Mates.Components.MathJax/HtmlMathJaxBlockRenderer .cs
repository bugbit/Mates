using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Extensions.Mathematics;

namespace Mates.Components.MathJax;

public class HtmlMathJaxBlockRenderer : HtmlObjectRenderer<MathBlock>
{
    protected override void Write(HtmlRenderer renderer, MathBlock obj)
    {
        renderer.EnsureLine();
        if (renderer.EnableHtmlForBlock)
            renderer.WriteLine("$$");

        renderer.WriteLeafRawLines(obj, true, renderer.EnableHtmlEscape);

        if (renderer.EnableHtmlForBlock)
            renderer.Write("$$");
    }
}
