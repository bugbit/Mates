using Markdig.Extensions.Mathematics;
using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace Mates.Components.CSharpMath;

public class HtmlCSharpMathInlineRenderer : HtmlObjectRenderer<MathInline>
{
    protected override void Write(HtmlRenderer renderer, MathInline obj)
    {
        renderer.WriteImage(ref obj.Content);
    }
}
