using Markdig.Extensions.Mathematics;
using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace Mates.Components.MathJax;

public class HtmlMathJaxInlineRenderer : HtmlObjectRenderer<MathInline>
{
    protected override void Write(HtmlRenderer renderer, MathInline obj)
    {
        if (renderer.EnableHtmlForInline)
        {
            renderer.WriteLine("$$");
        }

        if (renderer.EnableHtmlEscape)
        {
            renderer.WriteEscape(ref obj.Content);
        }
        else
        {
            renderer.Write(ref obj.Content);
        }

        if (renderer.EnableHtmlForInline)
        {
            renderer.WriteLine("$$");
        }
    }
}
