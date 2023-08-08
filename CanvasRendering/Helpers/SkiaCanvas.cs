using CanvasRendering.Contracts;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using SkiaSharp;
using System.Drawing;

namespace CanvasRendering.Helpers;

public unsafe class SkiaCanvas : ICanvas
{
    private readonly GL _gl;

    /// <summary>
    /// 绘制状态
    /// </summary>
    public bool IsDrawing { get; private set; }

    /// <summary>
    /// 画板大小
    /// </summary>
    public Vector2D<uint> Size { get; private set; }

    /// <summary>
    /// 纹理
    /// </summary>
    public Texture Texture { get; private set; }

    /// <summary>
    /// 顶点坐标缓冲区
    /// </summary>
    public uint VertexBuffer { get; private set; }

    /// <summary>
    /// 纹理坐标缓冲区
    /// </summary>
    public uint TexCoordBuffer { get; private set; }

    /// <summary>
    /// Skia画板
    /// </summary>
    public SKSurface Surface { get; private set; }

    public SkiaCanvas(GL gl, Vector2D<uint> size)
    {
        _gl = gl;
        Size = size;

        Initialization();
    }

    /// <summary>
    /// 初始化
    /// </summary>
    private void Initialization()
    {
        Texture = new Texture(_gl, GLEnum.Rgba, GLEnum.UnsignedByte);

        VertexBuffer = _gl.GenBuffer();
        float[] vertices = new float[]
        {
            0.0f, 0.0f, 0.0f,
            0.0f, Size.Y, 0.0f,
            Size.X, 0.0f, 0.0f,
            Size.X, Size.Y, 0.0f
        };
        _gl.BindBuffer(GLEnum.ArrayBuffer, VertexBuffer);
        _gl.BufferData<float>(GLEnum.ArrayBuffer, (uint)(vertices.Length * sizeof(float)), vertices, GLEnum.DynamicDraw);
        _gl.BindBuffer(GLEnum.ArrayBuffer, 0);


        TexCoordBuffer = _gl.GenBuffer();
        float[] texCoords = new float[]
        {
            0, 0,
            0, 1,
            1, 0,
            1, 1
        };
        _gl.BindBuffer(GLEnum.ArrayBuffer, TexCoordBuffer);
        _gl.BufferData<float>(GLEnum.ArrayBuffer, (uint)(texCoords.Length * sizeof(float)), texCoords, GLEnum.StaticDraw);
        _gl.BindBuffer(GLEnum.ArrayBuffer, 0);

        Surface = SKSurface.Create(new SKImageInfo((int)Size.X, (int)Size.Y, SKColorType.Rgba8888, SKAlphaType.Premul));
    }

    public void UpdateSize(Vector2D<uint> size)
    {
        Size = size;

        float[] vertices = new float[]
        {
            0.0f, 0.0f, 0.0f,
            0.0f, Size.Y, 0.0f,
            Size.X, 0.0f, 0.0f,
            Size.X, Size.Y, 0.0f
        };
        _gl.BindBuffer(GLEnum.ArrayBuffer, VertexBuffer);
        _gl.BufferSubData<float>(GLEnum.ArrayBuffer, 0, (uint)(vertices.Length * sizeof(float)), vertices);
        _gl.BindBuffer(GLEnum.ArrayBuffer, 0);

        Surface.Dispose();
        Surface = SKSurface.Create(new SKImageInfo((int)Size.X, (int)Size.Y, SKColorType.Rgba8888, SKAlphaType.Premul));
    }

    /// <summary>
    /// 开始绘制
    /// </summary>
    public void Begin()
    {
        if (!IsDrawing)
        {
            IsDrawing = true;
        }
    }

    /// <summary>
    /// 结束绘制
    /// </summary>
    public void End()
    {
        if (IsDrawing)
        {
            Texture.UpdateImage(Size, (void*)Surface.Snapshot().PeekPixels().GetPixels());
            IsDrawing = false;
        }
    }

    /// <summary>
    /// 清空画板
    /// </summary>
    public void Clear()
    {
        Surface.Canvas.Clear(SKColors.White);
    }

    /// <summary>
    /// 绘制填充颜色
    /// </summary>
    /// <param name="color"></param>
    public void DrawFill(Color color)
    {
        Surface.Canvas.Clear(new SKColor(color.R, color.G, color.B, color.A));
    }

    /// <summary>
    /// 绘制矩形
    /// </summary>
    /// <param name="rectangle">矩形位置及大小</param>
    /// <param name="color">填充颜色</param>
    public void DrawRectangle(RectangleF rectangle, Color color)
    {
        Surface.Canvas.DrawRect(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, SkiaPaintHelper.GetFillPaint(new SKColor(color.R, color.G, color.B, color.A), SKBlendMode.Src));
    }

    /// <summary>
    /// 绘制圆形
    /// </summary>
    /// <param name="origin">中心点</param>
    /// <param name="radius">半径</param>
    /// <param name="color">填充颜色</param>
    public void DrawCircle(PointF origin, float radius, Color color)
    {
        Surface.Canvas.DrawCircle(origin.X, origin.Y, radius, SkiaPaintHelper.GetFillPaint(new SKColor(color.R, color.G, color.B, color.A), SKBlendMode.Src));
    }

    /// <summary>
    /// 绘制线段
    /// </summary>
    /// <param name="start">起始点</param>
    /// <param name="end">结束点</param>
    /// <param name="width">线宽</param>
    /// <param name="color">填充颜色</param>
    public void DrawLine(PointF start, PointF end, float width, Color color)
    {
        Surface.Canvas.DrawLine(start.X, start.Y, end.X, end.Y, SkiaPaintHelper.GetStrokePaint(new SKColor(color.R, color.G, color.B, color.A), width));
    }

    /// <summary>
    /// 绘制字符串
    /// </summary>
    /// <param name="point">起始点</param>
    /// <param name="text">字符串</param>
    /// <param name="size">字体大小</param>
    /// <param name="color">颜色</param>
    /// <param name="fontPath">字体文件</param>
    public void DrawString(Point point, string text, uint size, Color color, string fontPath)
    {
        Surface.Canvas.DrawText(text, point.X, point.Y, SkiaPaintHelper.GetTextPaint(new SKColor(color.R, color.G, color.B, color.A), size, fontPath));
    }

    public void Dispose()
    {
        Texture.Dispose();

        _gl.DeleteBuffer(VertexBuffer);
        _gl.DeleteBuffer(TexCoordBuffer);

        Surface.Dispose();

        GC.SuppressFinalize(this);
    }
}