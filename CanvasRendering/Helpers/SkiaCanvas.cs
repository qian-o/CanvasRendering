using CanvasRendering.Contracts;
using CanvasRendering.Shaders;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using SkiaSharp;
using System;
using System.Drawing;
using System.Numerics;

namespace CanvasRendering.Helpers;

public unsafe class SkiaCanvas : ICanvas
{
    private readonly GL _gl;
    private readonly Vector2D<uint> Size;

    /// <summary>
    /// 绘制状态
    /// </summary>
    public bool IsDrawing { get; private set; }

    public uint VertexBuffer { get; private set; }

    public uint TexCoordBuffer { get; private set; }

    public uint DrawTexture { get; private set; }

    public SKSurface Surface { get; private set; }
    public uint BufferData { get; private set; }

    public SkiaCanvas(GL gl, Vector2D<uint> size)
    {
        _gl = gl;
        Size = size;

        CreateTextureAndSurface();
    }

    public void Begin()
    {
        IsDrawing = true;
    }

    public void End()
    {
        IsDrawing = false;
    }

    public void Clear()
    {
        Surface.Canvas.Clear(SKColors.White);
    }

    public void DrawRectangle(RectangleF rectangle, Color color)
    {
        using SKPaint paint = new() { IsAntialias = true, IsDither = true, FilterQuality = SKFilterQuality.High };
        paint.ColorF = new SKColor(color.R, color.G, color.B, color.A);

        Surface.Canvas.DrawRect(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, paint);
    }

    public void DrawCircle(PointF origin, float radius, Color color)
    {
        using SKPaint paint = new() { IsAntialias = true, IsDither = true, FilterQuality = SKFilterQuality.High };
        paint.ColorF = new SKColor(color.R, color.G, color.B, color.A);

        Surface.Canvas.DrawCircle(origin.X, origin.Y, radius, paint);
    }

    public void DrawLine(PointF start, PointF end, float width, Color color)
    {
        using SKPaint paint = new() { IsAntialias = true, IsDither = true, FilterQuality = SKFilterQuality.High };
        paint.ColorF = new SKColor(color.R, color.G, color.B, color.A);
        paint.StrokeWidth = width;

        Surface.Canvas.DrawLine(start.X, start.Y, end.X, end.Y, paint);
    }

    public void DrawString(Point point, string text, uint size, Color color, string fontPath)
    {
        using SKPaint paint = new() { IsAntialias = true, IsDither = true, FilterQuality = SKFilterQuality.High };
        paint.ColorF = new SKColor(color.R, color.G, color.B, color.A);
        paint.TextSize = size;
        paint.Typeface = SKTypeface.FromFile(fontPath);

        Surface.Canvas.DrawText(text, point.X, point.Y, paint);
    }

    public void DrawCanvas(ICanvas canvas, Rectangle<int> rectangle, bool clipToBounds)
    {
        if (canvas is not SkiaCanvas skiaCanvas)
        {
            throw new ArgumentException("canvas must be SkiaCanvas");
        }

        using SKPaint paint = new() { IsAntialias = true, IsDither = true, FilterQuality = SKFilterQuality.High };
        paint.ColorF = new SKColor(255, 255, 255, 255);

        if (clipToBounds)
        {
            Surface.Canvas.Save();
            Surface.Canvas.ClipRect(new SKRect(rectangle.Origin.X, rectangle.Origin.Y, rectangle.Max.X, rectangle.Max.Y));
            Surface.Canvas.DrawImage(skiaCanvas.Surface.Snapshot(), rectangle.Origin.X, rectangle.Origin.Y, paint);
            Surface.Canvas.Restore();
        }
        else
        {
            Surface.Canvas.DrawSurface(skiaCanvas.Surface, rectangle.Origin.X, rectangle.Origin.Y, paint);
        }
    }

    /// <summary>
    /// 更新当前画板的顶点缓冲区，用于后续绘制该画板时使用。
    /// </summary>
    /// <param name="rectangle">绘制该画板时的坐标及大小</param>
    private void UpdateVertexBuffer(Rectangle<float> rectangle)
    {
        float[] vertices = new float[] {
            rectangle.Origin.X, rectangle.Origin.Y, 0,
            rectangle.Origin.X, rectangle.Max.Y, 0,
            rectangle.Max.X, rectangle.Origin.Y, 0,
            rectangle.Max.X, rectangle.Max.Y, 0
        };

        _gl.BindBuffer(GLEnum.ArrayBuffer, VertexBuffer);
        _gl.BufferSubData<float>(GLEnum.ArrayBuffer, 0, (uint)(vertices.Length * sizeof(float)), vertices);
        _gl.BindBuffer(GLEnum.ArrayBuffer, 0);

        ReadOnlySpan<byte> bytes = Surface.Snapshot().PeekPixels().GetPixelSpan();
        fixed (void* p = bytes)
        {
            _gl.BindTexture(GLEnum.Texture2D, DrawTexture);
            _gl.TexSubImage2D(GLEnum.Texture2D, 0, 0, 0, Size.X, Size.Y, PixelFormat.Rgba, PixelType.UnsignedByte, p);
            _gl.BindTexture(GLEnum.Texture2D, 0);
        }
    }

    public static void DrawOnWindow(GL gl, ShaderProgram textureProgram, SkiaCanvas canvas)
    {
        uint positionAttrib = (uint)textureProgram.GetAttribLocation(DefaultVertex.PositionAttrib);
        uint texCoordAttrib = (uint)textureProgram.GetAttribLocation(DefaultVertex.TexCoordAttrib);

        gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

        gl.EnableVertexAttribArray(positionAttrib);
        gl.EnableVertexAttribArray(texCoordAttrib);

        textureProgram.Enable();

        Matrix4x4 projection = Matrix4x4.CreateOrthographicOffCenter(0.0f, canvas.Size.X, canvas.Size.Y, 0.0f, -1.0f, 1.0f);

        gl.UniformMatrix4(textureProgram.GetUniformLocation(DefaultVertex.ProjectionUniform), 1, false, (float*)&projection);

        canvas.UpdateVertexBuffer(new Rectangle<float>(0, 0, canvas.Size.X, canvas.Size.Y));

        gl.BindBuffer(GLEnum.ArrayBuffer, canvas.VertexBuffer);
        gl.VertexAttribPointer(positionAttrib, 3, GLEnum.Float, false, 0, null);

        gl.BindBuffer(GLEnum.ArrayBuffer, canvas.TexCoordBuffer);
        gl.VertexAttribPointer(texCoordAttrib, 2, GLEnum.Float, false, 0, null);

        gl.ActiveTexture(GLEnum.Texture0);
        gl.BindTexture(GLEnum.Texture2D, canvas.DrawTexture);
        gl.Uniform1(textureProgram.GetUniformLocation(TextureFragment.TexUniform), 0);

        gl.DrawArrays(GLEnum.TriangleStrip, 0, 4);

        gl.BindTexture(GLEnum.Texture2D, 0);

        gl.BindBuffer(GLEnum.ArrayBuffer, 0);

        textureProgram.Disable();

        gl.DisableVertexAttribArray(positionAttrib);
        gl.DisableVertexAttribArray(texCoordAttrib);
    }

    public void Dispose()
    {
        _gl.DeleteTexture(DrawTexture);
        Surface.Dispose();

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 创建纹理和表面
    /// </summary>
    private void CreateTextureAndSurface()
    {
        // 顶点坐标系
        {
            float[] vertices = new float[12];

            VertexBuffer = _gl.GenBuffer();
            _gl.BindBuffer(GLEnum.ArrayBuffer, VertexBuffer);
            _gl.BufferData<float>(GLEnum.ArrayBuffer, (uint)(vertices.Length * sizeof(float)), vertices, GLEnum.DynamicDraw);
            _gl.BindBuffer(GLEnum.ArrayBuffer, 0);
        }

        // 纹理坐标系
        {
            float[] texCoords = new float[] {
                0, 0,
                0, 1,
                1, 0,
                1, 1
            };

            TexCoordBuffer = _gl.GenBuffer();
            _gl.BindBuffer(GLEnum.ArrayBuffer, TexCoordBuffer);
            _gl.BufferData<float>(GLEnum.ArrayBuffer, (uint)(texCoords.Length * sizeof(float)), texCoords, GLEnum.StaticDraw);
            _gl.BindBuffer(GLEnum.ArrayBuffer, 0);
        }

        DrawTexture = _gl.GenTexture();

        _gl.BindTexture(GLEnum.Texture2D, DrawTexture);

        _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)GLEnum.Linear);
        _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)GLEnum.Linear);
        _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapS, (int)GLEnum.Repeat);
        _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapT, (int)GLEnum.Repeat);

        _gl.TexImage2D(GLEnum.Texture2D, 0, (int)GLEnum.Rgba8, Size.X, Size.Y, 0, PixelFormat.Rgba, PixelType.UnsignedByte, null);

        _gl.BindTexture(GLEnum.Texture2D, 0);

        Surface = SKSurface.Create(new SKImageInfo((int)Size.X, (int)Size.Y, SKColorType.Rgba8888));
    }
}
