using Silk.NET.Maths;
using Silk.NET.OpenGLES;

namespace CanvasRendering.Helpers;

public unsafe class Framebuffer : IDisposable
{
    private readonly GL _gl;
    private readonly Vector2D<uint> _size;

    public GLEnum Format { get; }

    public GLEnum Type { get; }

    public uint FboId { get; }

    public uint TextureId { get; }

    public Framebuffer(GL gl, Vector2D<uint> size)
    {
        _gl = gl;
        _size = size;

        _gl.GetInteger(GLEnum.ImplementationColorReadFormat, out int format);
        _gl.GetInteger(GLEnum.ImplementationColorReadType, out int type);

        Format = (GLEnum)format;
        Type = (GLEnum)type;

        FboId = _gl.GenFramebuffer();
        TextureId = _gl.GenTexture();

        _gl.BindTexture(GLEnum.Texture2D, TextureId);

        _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)GLEnum.Linear);
        _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)GLEnum.Linear);
        _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapS, (int)GLEnum.Repeat);
        _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapT, (int)GLEnum.Repeat);

        _gl.TexImage2D(GLEnum.Texture2D, 0, (int)GLEnum.Rgba8, _size.X, _size.Y, 0, Format, Type, null);

        _gl.BindTexture(GLEnum.Texture2D, 0);

        _gl.BindFramebuffer(GLEnum.Framebuffer, FboId);

        _gl.FramebufferTexture2D(GLEnum.Framebuffer, GLEnum.ColorAttachment0, GLEnum.Texture2D, TextureId, 0);

        _gl.BindFramebuffer(GLEnum.Framebuffer, 0);
    }

    public void Dispose()
    {
        _gl.DeleteFramebuffer(FboId);
        _gl.DeleteTexture(TextureId);

        GC.SuppressFinalize(this);
    }
}
