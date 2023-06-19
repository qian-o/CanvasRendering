using Silk.NET.Maths;
using Silk.NET.OpenGLES;

namespace CanvasRendering.Helpers;

public unsafe class Framebuffer : IDisposable
{
    private readonly GL _gl;
    private readonly Vector2D<uint> _size;

    public GLEnum Format { get; }

    public GLEnum Type { get; }

    public uint MaxSamples { get; }

    public uint DrawFbo { get; }

    public uint DrawTexture { get; }

    public uint MultisampleFbo { get; }

    public uint MultisampleTexture { get; }

    public uint MultisampleRbo { get; }

    public Framebuffer(GL gl, Vector2D<uint> size)
    {
        _gl = gl;
        _size = size;

        _gl.GetInteger(GLEnum.ImplementationColorReadFormat, out int format);
        _gl.GetInteger(GLEnum.ImplementationColorReadType, out int type);
        _gl.GetInteger(GLEnum.MaxSamples, out int maxSamples);

        Format = (GLEnum)format;
        Type = (GLEnum)type;
        MaxSamples = (uint)maxSamples;

        MultisampleFbo = _gl.GenFramebuffer();
        MultisampleTexture = _gl.GenTexture();
        MultisampleRbo = _gl.GenRenderbuffer();
        DrawFbo = _gl.GenFramebuffer();
        DrawTexture = _gl.GenTexture();

        // 绘图纹理
        {
            _gl.BindTexture(GLEnum.Texture2D, DrawTexture);

            _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)GLEnum.Linear);
            _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)GLEnum.Linear);
            _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapS, (int)GLEnum.Repeat);
            _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapT, (int)GLEnum.Repeat);

            _gl.TexImage2D(GLEnum.Texture2D, 0, (int)GLEnum.Rgba8, _size.X, _size.Y, 0, Format, Type, null);

            _gl.BindTexture(GLEnum.Texture2D, 0);

            _gl.BindFramebuffer(GLEnum.Framebuffer, DrawFbo);

            _gl.FramebufferTexture2D(GLEnum.Framebuffer, GLEnum.ColorAttachment0, GLEnum.Texture2D, DrawTexture, 0);

            _gl.BindFramebuffer(GLEnum.Framebuffer, 0);
        }

        // 多重采样纹理、缓冲区
        {
            _gl.BindFramebuffer(GLEnum.Framebuffer, MultisampleFbo);

            _gl.BindTexture(GLEnum.Texture2DMultisample, MultisampleTexture);

            _gl.TexStorage2DMultisample(GLEnum.Texture2DMultisample, MaxSamples, GLEnum.Rgba8, _size.X, _size.Y, true);

            _gl.TexParameter(GLEnum.Texture2DMultisample, GLEnum.TextureMinFilter, (int)GLEnum.Linear);
            _gl.TexParameter(GLEnum.Texture2DMultisample, GLEnum.TextureMagFilter, (int)GLEnum.Linear);
            _gl.TexParameter(GLEnum.Texture2DMultisample, GLEnum.TextureWrapS, (int)GLEnum.Repeat);
            _gl.TexParameter(GLEnum.Texture2DMultisample, GLEnum.TextureWrapT, (int)GLEnum.Repeat);

            _gl.BindTexture(GLEnum.Texture2DMultisample, 0);

            _gl.FramebufferTexture2D(GLEnum.Framebuffer, GLEnum.ColorAttachment0, GLEnum.Texture2DMultisample, MultisampleTexture, 0);

            _gl.BindRenderbuffer(GLEnum.Renderbuffer, MultisampleRbo);

            _gl.RenderbufferStorageMultisample(GLEnum.Renderbuffer, MaxSamples, GLEnum.Depth24Stencil8, _size.X, _size.Y);

            _gl.BindRenderbuffer(GLEnum.Renderbuffer, 0);

            _gl.FramebufferRenderbuffer(GLEnum.Framebuffer, GLEnum.DepthAttachment, GLEnum.Renderbuffer, MultisampleRbo);
            _gl.FramebufferRenderbuffer(GLEnum.Framebuffer, GLEnum.StencilAttachment, GLEnum.Renderbuffer, MultisampleRbo);

            _gl.BindFramebuffer(GLEnum.Framebuffer, 0);
        }
    }

    public void GenBmp(string filePath)
    {
        byte[] bytes = new byte[_size.X * _size.Y * 4];

        fixed (void* data = bytes)
        {
            _gl.BindFramebuffer(GLEnum.Framebuffer, MultisampleFbo);
            _gl.ReadnPixels(0, 0, _size.X, _size.Y, Format, Type, (uint)bytes.Length, data);
        }
    }

    public void Dispose()
    {
        _gl.DeleteFramebuffer(DrawFbo);
        _gl.DeleteTexture(DrawTexture);
        _gl.DeleteFramebuffer(MultisampleFbo);
        _gl.DeleteTexture(MultisampleTexture);
        _gl.DeleteRenderbuffer(MultisampleRbo);

        GC.SuppressFinalize(this);
    }
}
