using CanvasRendering.Contracts;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using SkiaSharp;
using System.Drawing;

namespace CanvasRendering.Helpers;

public unsafe class SkiaCanvas : ICanvas
{
    private static readonly Dictionary<string, SKTypeface> _typeface = new();

    private readonly GL _gl;

    /// <summary>
    /// 绘制状态
    /// </summary>
    public bool IsDrawing { get; private set; }

    /// <summary>
    /// 记录之前的帧缓冲区
    /// </summary>
    public uint RecordFbo { get; private set; }

    /// <summary>
    /// 记录之前的位置
    /// </summary>
    public Vector2D<int> RecordPosition { get; private set; }

    /// <summary>
    /// 记录之前的大小
    /// </summary>
    public Vector2D<uint> RecordSize { get; private set; }

    /// <summary>
    /// 画板大小
    /// </summary>
    public Vector2D<uint> Size { get; }

    /// <summary>
    /// 帧缓冲区
    /// </summary>
    public Framebuffer Framebuffer { get; private set; }

    /// <summary>
    /// 顶点坐标缓冲区
    /// </summary>
    public uint VertexBuffer { get; private set; }

    /// <summary>
    /// 纹理坐标缓冲区
    /// </summary>
    public uint TexCoordBuffer { get; private set; }

    public GRContext SkiaContext { get; private set; }

    public GRBackendRenderTarget RenderTarget { get; private set; }

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
        Framebuffer = new Framebuffer(_gl, Size);

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
            float[] texCoords = new float[8];

            TexCoordBuffer = _gl.GenBuffer();
            _gl.BindBuffer(GLEnum.ArrayBuffer, TexCoordBuffer);
            _gl.BufferData<float>(GLEnum.ArrayBuffer, (uint)(texCoords.Length * sizeof(float)), texCoords, GLEnum.DynamicDraw);
            _gl.BindBuffer(GLEnum.ArrayBuffer, 0);
        }

        SkiaContext = GRContext.CreateGl();
    }

    /// <summary>
    /// 开始绘制（记录之前的帧缓冲及视口大小）
    /// </summary>
    public void Begin()
    {
        if (!IsDrawing)
        {
            int[] viewport = new int[4];
            _gl.GetInteger(GLEnum.FramebufferBinding, out int fbo);
            _gl.GetInteger(GLEnum.Viewport, viewport);

            RecordFbo = (uint)fbo;
            RecordPosition = new Vector2D<int>(viewport[0], viewport[1]);
            RecordSize = new Vector2D<uint>((uint)viewport[2], (uint)viewport[3]);

            _gl.BindFramebuffer(GLEnum.Framebuffer, Framebuffer.DrawFbo);
            _gl.Viewport(0, 0, Size.X, Size.Y);

            if (RenderTarget == null)
            {
                _gl.GetInteger(GLEnum.Samples, out int samples);
                _gl.GetInteger(GLEnum.Stencil, out int stencil);

                RenderTarget = new GRBackendRenderTarget((int)Size.X, (int)Size.Y, samples, stencil, new GRGlFramebufferInfo(Framebuffer.DrawFbo, SKColorType.Rgba8888.ToGlSizedFormat()));
                Surface = SKSurface.Create(SkiaContext, RenderTarget, GRSurfaceOrigin.TopLeft, SKColorType.Rgba8888);
            }

            IsDrawing = true;
        }
    }

    /// <summary>
    /// 结束绘制（还原之前记录的帧缓冲及视口大小）
    /// </summary>
    public void End()
    {
        if (IsDrawing)
        {
            SkiaContext.Flush();
            SkiaContext.ResetContext();

            _gl.BindFramebuffer(GLEnum.Framebuffer, RecordFbo);
            _gl.Viewport(RecordPosition.X, RecordPosition.Y, RecordSize.X, RecordSize.Y);

            IsDrawing = false;
        }
    }

    /// <summary>
    /// 清空画板
    /// </summary>
    public void Clear()
    {
        _gl.ClearColor(Color.Transparent);
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
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
        using SKPaint paint = new()
        {
            IsAntialias = true,
            IsDither = true,
            FilterQuality = SKFilterQuality.High,
            Color = new SKColor(color.R, color.G, color.B, color.A),
            BlendMode = SKBlendMode.Src
        };

        Surface.Canvas.DrawRect(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, paint);
    }

    /// <summary>
    /// 绘制圆形
    /// </summary>
    /// <param name="origin">中心点</param>
    /// <param name="radius">半径</param>
    /// <param name="color">填充颜色</param>
    public void DrawCircle(PointF origin, float radius, Color color)
    {
        using SKPaint paint = new()
        {
            IsAntialias = true,
            IsDither = true,
            FilterQuality = SKFilterQuality.High,
            Color = new SKColor(color.R, color.G, color.B, color.A)
        };

        Surface.Canvas.DrawCircle(origin.X, origin.Y, radius, paint);
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
        using SKPaint paint = new()
        {
            IsAntialias = true,
            IsDither = true,
            FilterQuality = SKFilterQuality.High,
            Color = new SKColor(color.R, color.G, color.B, color.A),
            StrokeWidth = width,
            Style = SKPaintStyle.Stroke
        };

        Surface.Canvas.DrawLine(start.X, start.Y, end.X, end.Y, paint);
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
        if (!_typeface.TryGetValue(fontPath, out SKTypeface typeface))
        {
            typeface = SKTypeface.FromStream(FileManager.LoadFile(fontPath));

            _typeface.Add(fontPath, typeface);
        }

        using SKPaint paint = new()
        {
            IsAntialias = true,
            IsDither = true,
            FilterQuality = SKFilterQuality.High,
            Color = new SKColor(color.R, color.G, color.B, color.A),
            TextSize = size,
            Typeface = typeface
        };

        Surface.Canvas.DrawText(text, point.X, point.Y, paint);
    }

    /// <summary>
    /// 更新当前画板的顶点缓冲区
    /// </summary>
    /// <param name="rectangle">绘制该画板时的坐标及大小</param>
    public void UpdateVertexBuffer(Rectangle<float> rectangle)
    {
        Vector3D<float> point1 = new(rectangle.Origin.X, rectangle.Origin.Y, 0.0f);

        Vector3D<float> point2 = new(rectangle.Origin.X, rectangle.Max.Y, 0.0f);

        Vector3D<float> point3 = new(rectangle.Max.X, rectangle.Origin.Y, 0.0f);

        Vector3D<float> point4 = new(rectangle.Max.X, rectangle.Max.Y, 0.0f);

        float[] vertices = new float[] {
            point1.X, point1.Y, point1.Z,
            point2.X, point2.Y, point2.Z,
            point3.X, point3.Y, point3.Z,
            point4.X, point4.Y, point4.Z
        };

        _gl.BindBuffer(GLEnum.ArrayBuffer, VertexBuffer);
        _gl.BufferSubData<float>(GLEnum.ArrayBuffer, 0, (uint)(vertices.Length * sizeof(float)), vertices);
        _gl.BindBuffer(GLEnum.ArrayBuffer, 0);
    }

    /// <summary>
    /// 更新当前画板的纹理缓冲区
    /// </summary>
    public void UpdateTexCoordBuffer()
    {
        Vector2D<float> point1 = new(0, 0);

        Vector2D<float> point2 = new(0, 1);

        Vector2D<float> point3 = new(1, 0);

        Vector2D<float> point4 = new(1, 1);

        float[] texCoords = new float[] {
            point1.X, point1.Y,
            point2.X, point2.Y,
            point3.X, point3.Y,
            point4.X, point4.Y
        };

        _gl.BindBuffer(GLEnum.ArrayBuffer, TexCoordBuffer);
        _gl.BufferSubData<float>(GLEnum.ArrayBuffer, 0, (uint)(texCoords.Length * sizeof(float)), texCoords);
        _gl.BindBuffer(GLEnum.ArrayBuffer, 0);
    }

    public void Dispose()
    {
        Framebuffer.Dispose();

        _gl.DeleteBuffer(VertexBuffer);
        _gl.DeleteBuffer(TexCoordBuffer);

        SkiaContext.AbandonContext(true);
        SkiaContext.Dispose();
        RenderTarget?.Dispose();
        Surface?.Dispose();

        GC.SuppressFinalize(this);
    }
}