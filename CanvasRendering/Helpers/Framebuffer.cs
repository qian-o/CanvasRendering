using Silk.NET.Maths;
using Silk.NET.OpenGLES;

namespace CanvasRendering.Helpers;

public unsafe class Framebuffer : IDisposable
{
    private readonly GL _gl;
    private readonly Vector2D<uint> _size;

    public PixelFormat Format { get; }

    public PixelType Type { get; }

    public uint Fbo { get; }

    public uint Depth { get; }

    public uint Texture { get; }

    public Framebuffer(GL gl, Vector2D<uint> size)
    {
        _gl = gl;
        _size = size;

        _gl.GetInteger(GLEnum.ImplementationColorReadFormat, out int format);
        _gl.GetInteger(GLEnum.ImplementationColorReadType, out int type);

        Format = (PixelFormat)format;
        Type = (PixelType)type;

        Fbo = _gl.GenFramebuffer();
        Depth = _gl.GenRenderbuffer();
        Texture = _gl.GenTexture();

        _gl.BindTexture(GLEnum.Texture2D, Texture);

        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Linear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

        _gl.TexImage2D(GLEnum.Texture2D, 0, (int)InternalFormat.Rgba, _size.X, _size.Y, 0, Format, Type, IntPtr.Zero);

        _gl.BindTexture(TextureTarget.Texture2D, 0);

        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, Fbo);

        _gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, Texture, 0);

        _gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, Depth);
        _gl.RenderbufferStorage(GLEnum.Renderbuffer, GLEnum.Depth24Stencil8, _size.X, _size.Y);

        _gl.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, Depth);
        _gl.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.StencilAttachment, RenderbufferTarget.Renderbuffer, Depth);

        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    public void GenBmp(string filePath)
    {
        byte[] bytes = new byte[_size.X * _size.Y * 4];

        fixed (void* data = bytes)
        {
            _gl.BindFramebuffer(FramebufferTarget.Framebuffer, Fbo);
            _gl.ReadnPixels(0, 0, _size.X, _size.Y, (GLEnum)Format, (GLEnum)Type, (uint)bytes.Length, data);
        }
    }

    public void Dispose()
    {
        _gl.DeleteFramebuffer(Fbo);
        _gl.DeleteRenderbuffer(Fbo);
        _gl.DeleteTexture(Texture);

        GC.SuppressFinalize(this);
    }
}
