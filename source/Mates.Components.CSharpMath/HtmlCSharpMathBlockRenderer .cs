using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Extensions.Mathematics;

namespace Mates.Components.CSharpMath;

public class HtmlCSharpMathBlockRenderer : HtmlObjectRenderer<MathBlock>
{
    protected override void Write(HtmlRenderer renderer, MathBlock obj)
    {
        renderer.WriteImage(obj);
    }
}
