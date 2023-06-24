using FreeTypeSharp;
using FreeTypeSharp.Native;
using System.Runtime.InteropServices;
using System.Text;

namespace CanvasRendering.Helpers;

public unsafe class Glyph : IDisposable
{
    private static readonly byte[] TypefaceBuffer;

    private delegate int MoveTo(FT_Vector* to, void* user);
    private delegate int LineTo(FT_Vector* to, void* user);
    private delegate int ConicTo(FT_Vector* control, FT_Vector* to, void* user);
    private delegate int CubicTo(FT_Vector* control1, FT_Vector* control2, FT_Vector* to, void* user);

    private readonly string _text;
    private readonly uint _size;
    private readonly float _scale;
    private readonly Dictionary<char, string> _pairs;
    private readonly nint _moveToPointer;
    private readonly nint _lineToPointer;
    private readonly nint _conicToPointer;
    private readonly nint _cubicToPointer;

    private StringBuilder stringBuilder;
    private MoveTo moveTo;
    private LineTo lineTo;
    private ConicTo conicTo;
    private CubicTo cubicTo;

    static Glyph()
    {
        TypefaceBuffer = File.ReadAllBytes(@"Resources/方正FW筑紫古典S黑 简.ttf");
    }

    public Glyph(string text, uint size)
    {
        moveTo = (to, user) => { stringBuilder.Append($"M {to->x * _scale} {to->y * _scale} "); return 0; };
        lineTo = (to, user) => { stringBuilder.Append($"L {to->x * _scale} {to->y * _scale} "); return 0; };
        conicTo = (control, to, user) => { stringBuilder.Append($"Q {control->x * _scale} {control->y * _scale} {to->x * _scale} {to->y * _scale} "); return 0; };
        cubicTo = (control1, control2, to, user) => { stringBuilder.Append($"C {control1->x * _scale} {control1->y * _scale} {control2->x * _scale} {control2->y * _scale} {to->x * _scale} {to->y * _scale} "); return 0; };

        _text = text;
        _size = size;
        _scale = 1.0f / 64.0f;
        _pairs = new Dictionary<char, string>();
        _moveToPointer = Marshal.GetFunctionPointerForDelegate(moveTo);
        _lineToPointer = Marshal.GetFunctionPointerForDelegate(lineTo);
        _conicToPointer = Marshal.GetFunctionPointerForDelegate(conicTo);
        _cubicToPointer = Marshal.GetFunctionPointerForDelegate(cubicTo);

        Initialization();
    }

    private void Initialization()
    {
        fixed (byte* typeface = TypefaceBuffer)
        {
            FreeTypeLibrary library = new();

            FT.FT_New_Memory_Face(library.Native, (nint)typeface, TypefaceBuffer.Length, 0, out nint aface);

            FT.FT_Set_Pixel_Sizes(aface, _size, 0);

            FT_FaceRec* rec = (FT_FaceRec*)aface;

            FT_Matrix matrix = new()
            {
                xx = 1 << 16,
                xy = 0,
                yx = 0,
                yy = -1 << 16,
            };

            FT_Vector delta = new()
            {
                x = 0,
                y = rec->max_advance_height
            };

            FT.FT_Set_Transform(aface, (nint)(&matrix), (nint)(&delta));

            foreach (char c in _text)
            {
                _pairs.Add(c, GetSvg(library, (FT_FaceRec*)aface, c));
            }

            library.Dispose();
        }
    }

    private string GetSvg(FreeTypeLibrary library, FT_FaceRec* aface, char c)
    {
        stringBuilder = new StringBuilder();

        uint glyphIndex = FT.FT_Get_Char_Index((nint)aface, c);

        FT.FT_Load_Glyph((nint)aface, glyphIndex, 0);

        FT.FT_Get_Glyph((nint)aface->glyph, out nint aglyph);

        FT_Outline_Funcs outline_Funcs = new()
        {
            moveTo = _moveToPointer,
            lineTo = _lineToPointer,
            conicTo = _conicToPointer,
            cubicTo = _cubicToPointer,
            shift = 0,
            delta = 0
        };

        FT.FT_Outline_Decompose((nint)(&aface->glyph->outline), ref outline_Funcs, IntPtr.Zero);
        FT.FT_Outline_Done(library.Native, (nint)(&aface->glyph->outline));
        FT.FT_Done_Glyph(aglyph);

        stringBuilder.Append('Z');

        return stringBuilder.ToString();
    }

    public void Dispose()
    {
        moveTo = null;
        lineTo = null;
        conicTo = null;
        cubicTo = null;

        GC.SuppressFinalize(this);
    }
}
