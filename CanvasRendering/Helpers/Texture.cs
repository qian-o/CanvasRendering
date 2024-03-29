﻿using Silk.NET.Maths;
using Silk.NET.OpenGLES;

namespace CanvasRendering.Helpers;

public unsafe class Texture : IDisposable
{
    private readonly GL _gl;
    private readonly GLEnum _format;
    private readonly GLEnum _type;

    public uint PboId { get; }

    public uint TextureId { get; }

    public Vector2D<uint> Size { get; private set; }

    public Texture(GL gl, GLEnum format, GLEnum type)
    {
        _gl = gl;
        _format = format;
        _type = type;

        PboId = _gl.GenBuffer();
        TextureId = _gl.GenTexture();

        _gl.BindTexture(GLEnum.Texture2D, TextureId);

        _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)GLEnum.Linear);
        _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)GLEnum.Linear);
        _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapS, (int)GLEnum.Repeat);
        _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapT, (int)GLEnum.Repeat);

        _gl.BindTexture(GLEnum.Texture2D, 0);
    }

    public void AllocationBuffer(Vector2D<uint> size, out nint pboData)
    {
        Size = size;

        uint dataSize = Size.X * Size.Y * 4;

        _gl.BindBuffer(GLEnum.PixelUnpackBuffer, PboId);
        _gl.BindTexture(GLEnum.Texture2D, TextureId);

        _gl.BufferData(GLEnum.PixelUnpackBuffer, dataSize, null, GLEnum.StreamDraw);

        pboData = (nint)_gl.MapBufferRange(GLEnum.PixelUnpackBuffer, 0, dataSize, (uint)(GLEnum.MapReadBit | GLEnum.MapWriteBit));

        _gl.BindTexture(GLEnum.Texture2D, 0);
        _gl.BindBuffer(GLEnum.PixelUnpackBuffer, 0);
    }

    public void FlushTexture()
    {
        _gl.BindBuffer(GLEnum.PixelUnpackBuffer, PboId);
        _gl.BindTexture(GLEnum.Texture2D, TextureId);

        _gl.UnmapBuffer(GLEnum.PixelUnpackBuffer);
        _gl.TexImage2D(GLEnum.Texture2D, 0, (int)GLEnum.Rgba8, Size.X, Size.Y, 0, _format, _type, null);

        _gl.BindTexture(GLEnum.Texture2D, 0);
        _gl.BindBuffer(GLEnum.PixelUnpackBuffer, 0);
    }

    public void Dispose()
    {
        _gl.DeleteBuffer(PboId);
        _gl.DeleteTexture(TextureId);

        GC.SuppressFinalize(this);
    }
}
