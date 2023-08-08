using Silk.NET.Maths;
using Silk.NET.OpenGLES;

namespace CanvasRendering.Helpers;

public unsafe class Texture : IDisposable
{
    private readonly GL _gl;
    private readonly GLEnum _format;
    private readonly GLEnum _type;

    public uint TextureId { get; }

    public Vector2D<uint> CurrentSize { get; private set; }

    public Texture(GL gl, GLEnum format, GLEnum type)
    {
        _gl = gl;
        _format = format;
        _type = type;

        TextureId = _gl.GenTexture();

        _gl.BindTexture(GLEnum.Texture2D, TextureId);

        _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)GLEnum.Linear);
        _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)GLEnum.Linear);
        _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapS, (int)GLEnum.Repeat);
        _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapT, (int)GLEnum.Repeat);

        _gl.BindTexture(GLEnum.Texture2D, 0);
    }

    public void UpdateImage(Vector2D<uint> size, void* pixels)
    {
        _gl.BindTexture(GLEnum.Texture2D, TextureId);

        if (CurrentSize == size)
        {
            _gl.TexSubImage2D(GLEnum.Texture2D, 0, 0, 0, size.X, size.Y, _format, _type, pixels);
        }
        else
        {
            _gl.TexImage2D(GLEnum.Texture2D, 0, (int)GLEnum.Rgba8, size.X, size.Y, 0, _format, _type, pixels);

            CurrentSize = size;
        }

        _gl.BindTexture(GLEnum.Texture2D, 0);
    }

    public void Dispose()
    {
        _gl.DeleteTexture(TextureId);

        GC.SuppressFinalize(this);
    }
}
