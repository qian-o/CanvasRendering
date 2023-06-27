using System.Text;
using Typography.Library;
using Typography.OpenFont;
using Typography.TextLayout;

namespace CanvasRendering.Helpers;

public unsafe class GlyphHelper
{
    private static readonly Dictionary<string, Typeface> _cache = new();

    public static string GetSvgPath(string text, uint size, string fontPath)
    {
        if (!_cache.TryGetValue(fontPath, out Typeface typeface))
        {
            typeface = new OpenFontReader().Read(File.OpenRead(fontPath));

            _cache.Add(fontPath, typeface);
        }

        float scale = typeface.CalculateScaleToPixel(size);

        GlyphLayout glyphLayout = new()
        {
            Typeface = typeface
        };
        glyphLayout.Layout(text.ToArray(), 0, text.Length);

        StringBuilder stringBuilder = new();

        float x = 0;
        foreach (UnscaledGlyphPlan item in glyphLayout.GetUnscaledGlyphPlanIter())
        {
            Glyph glyph = typeface.GetGlyph(item.glyphIndex);

            float width = glyph.MaxX - glyph.MinX;
            float height = glyph.MaxY - glyph.MinY;

            Matrix3x3 matrix = new();
            matrix.Transform(x * scale, typeface.Bounds.YMax * scale);
            matrix.Scale(1.0f, -1.0f, 0.5f, 0.5f);
            matrix.Scale(scale, scale, 0.5f, 0.5f);

            GlyphTranslatorToPath translatorToPath = new();
            translatorToPath.Read(glyph.GlyphPoints, glyph.EndPoints, matrix);

            stringBuilder.Append(translatorToPath.Path);
            stringBuilder.Append(' ');

            x += item.AdvanceX;
        }
    
        return stringBuilder.ToString();
    }
}
