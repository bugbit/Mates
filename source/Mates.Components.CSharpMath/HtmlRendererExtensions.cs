using CSharpMath.SkiaSharp;
using Markdig.Helpers;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mates.Components.CSharpMath
{
    public static class HtmlRendererExtensions
    {
        public static void WriteImage(this HtmlRenderer renderer, string latex)
        {
            var painter = new MathPainter { LaTeX = latex }; // or TextPainter

            painter.TextColor = SKColors.White;

            using var png = painter.DrawAsStream(format: SkiaSharp.SKEncodedImageFormat.Jpeg);

            if (png is null)
                return;

            using var strean = new MemoryStream();

            png.CopyTo(strean);

            var src = $"data:image/jpg;base64,{Convert.ToBase64String(strean.ToArray())}";
            var obj = new HtmlAttributes();

            renderer.Write("<img");

            obj.AddProperty("src", src);

            renderer.WriteAttributes(obj).WriteLine("/>");
        }

        public static void WriteImage(this HtmlRenderer renderer, ref StringSlice latexSlice)
        {
            renderer.WriteImage(latexSlice.AsSpan().ToString());
        }
        public static void WriteImage(this HtmlRenderer renderer, LeafBlock latexBlock)
        {
            if (latexBlock is null)
                throw new ArgumentNullException(nameof(latexBlock));

            var sb = new StringBuilder();
            var slices = latexBlock.Lines.Lines;
            if (slices is not null)
            {
                for (int i = 0; i < slices.Length; i++)
                {
                    ref StringSlice slice = ref slices[i].Slice;
                    if (slice.Text is null)
                    {
                        break;
                    }

                    if (i > 0)
                    {
                        sb.AppendLine();
                    }

                    ReadOnlySpan<char> span = slice.AsSpan();
                    sb.Append(span);

                    sb.AppendLine();
                }
            }

            renderer.WriteImage(sb.ToString());
        }
    }
}
