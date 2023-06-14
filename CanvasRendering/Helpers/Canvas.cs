using Silk.NET.Maths;
using Silk.NET.OpenGLES;

namespace CanvasRendering.Helpers;

public class Canvas
{
    private readonly GL _gl;
    private readonly ShaderHelper _shaderHelper;
    private readonly Rectangle<int> _rectangle;
    private readonly float _scale;
    private readonly Vector2D<uint> _actualSize;

    public Framebuffer Framebuffer { get; }

    public uint VertexBuffer { get; }

    public uint TexCoordBuffer { get; }

    public Canvas(GL gl, ShaderHelper shaderHelper, Rectangle<int> rectangle)
    {
        _gl = gl;
        _shaderHelper = shaderHelper;
        _rectangle = rectangle;

        _gl.GetInteger(GLEnum.MaxTextureSize, out int maxTextureSize);

        if (_rectangle.Size.X * _rectangle.Size.Y > maxTextureSize)
        {
            _scale = Convert.ToSingle(Math.Sqrt((double)maxTextureSize / (_rectangle.Size.X * _rectangle.Size.Y)));

            _actualSize = new Vector2D<uint>(Convert.ToUInt32(_rectangle.Size.X * _scale), Convert.ToUInt32(_rectangle.Size.Y * _scale));
        }
        else
        {
            _scale = 1;

            _actualSize = new Vector2D<uint>((uint)_rectangle.Size.X, (uint)_rectangle.Size.Y);
        }

        Framebuffer = new Framebuffer(gl, _actualSize);
    }
}
