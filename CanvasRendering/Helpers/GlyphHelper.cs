using System.Text;
using Typography.Library;
using Typography.OpenFont;
using Typography.TextLayout;

namespace CanvasRendering.Helpers;

public unsafe class GlyphHelper
{
    private static readonly Dictionary<string, Typeface> _cache = new();

    public static List<(float[] Vertices, uint VertexCount)> GetVboData(string text, uint size, string fontPath)
    {
        if (!_cache.TryGetValue(fontPath, out Typeface typeface))
        {
            typeface = new OpenFontReader().Read(FileManager.LoadFile(fontPath));

            _cache.Add(fontPath, typeface);
        }

        float scale = typeface.CalculateScaleToPixel(size);

        GlyphLayout glyphLayout = new()
        {
            Typeface = typeface
        };
        glyphLayout.Layout(text.ToArray(), 0, text.Length);

        List<(float[], uint)> data = new();

        GlyphTranslatorToPath translatorToPath = new();
        WritablePath writablePath = new();
        SimpleCurveFlattener simpleCurveFlattener = new();
        TessTool tessTool = new();

        translatorToPath.SetOutput(writablePath);

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

            translatorToPath.Read(glyph.GlyphPoints, glyph.EndPoints, matrix);

            x += item.AdvanceX;
        }

        float[] flattenPoints = simpleCurveFlattener.Flatten(writablePath.Points, out int[] endContours)!;

        float[] tessData = tessTool.TessAsTriVertexArray(flattenPoints, endContours, out int vertexCount);

        data.Add((tessData, (uint)vertexCount));

        return data;
    }
}
