using Typography.Library;
using Typography.OpenFont;
using Typography.TextLayout;

namespace CanvasRendering.Helpers;

public unsafe class GlyphHelper
{
    private static readonly Dictionary<string, Typeface> _typefaceCache = new();
    private static readonly Dictionary<ushort, Glyph> _glyphCache = new();
    private static readonly GlyphLayout _glyphLayout = new();
    private static readonly GlyphTranslatorToPath _translatorToPath = new();
    private static readonly WritablePath _writablePath = new();
    private static readonly SimpleCurveFlattener _simpleCurveFlattener = new();
    private static readonly TessTool _tessTool = new();

    static GlyphHelper()
    {
        _simpleCurveFlattener.IncrementalStep = 7;
        _translatorToPath.SetOutput(_writablePath, null);
    }

    public static List<(float[] Vertices, uint VertexCount)> GetVertices(string text, uint size, string fontPath)
    {
        if (!_typefaceCache.TryGetValue(fontPath, out Typeface typeface))
        {
            typeface = new OpenFontReader().Read(FileManager.LoadFile(fontPath));

            _typefaceCache.Add(fontPath, typeface);
        }

        float scale = typeface.CalculateScaleToPixel(size);

        _glyphLayout.Typeface = typeface;
        _glyphLayout.Layout(text.ToArray(), 0, text.Length);

        List<(float[], uint)> data = new();

        float x = 0;
        foreach (UnscaledGlyphPlan item in _glyphLayout.GetUnscaledGlyphPlanIter())
        {
            _writablePath.Points.Clear();

            if (!_glyphCache.TryGetValue(item.glyphIndex, out Glyph glyph))
            {
                glyph = typeface.GetGlyph(item.glyphIndex);

                _glyphCache.Add(item.glyphIndex, glyph);
            }

            Matrix3x3 matrix = new();
            matrix.Transform(x * scale, typeface.Bounds.YMax * scale);
            matrix.Scale(1.0f, -1.0f, 0.5f, 0.5f);
            matrix.Scale(scale, scale, 0.5f, 0.5f);

            _translatorToPath.Read(glyph.GlyphPoints, glyph.EndPoints, matrix);

            if (_writablePath.Points.Any())
            {
                float[] flattenPoints = _simpleCurveFlattener.Flatten(_writablePath.Points, out int[] endContours)!;

                float[] tessData = _tessTool.TessAsTriVertexArray(flattenPoints, endContours, out int vertexCount);

                data.Add((tessData, (uint)vertexCount));
            }

            x += item.AdvanceX;
        }

        return data;
    }
}
