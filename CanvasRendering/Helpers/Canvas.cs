using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using System.Drawing;
using System.Numerics;

namespace CanvasRendering.Helpers;

public unsafe class Canvas : IDisposable
{
    private readonly uint CirclePoints = 120;

    private readonly GL _gl;
    private readonly ShaderHelper _shaderHelper;

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
    /// 当前画板的位置及大小
    /// </summary>
    public Rectangle<int> Rectangle { get; private set; }

    /// <summary>
    /// 画板大小
    /// </summary>
    public Vector2D<uint> Size { get; private set; }

    /// <summary>
    /// 帧缓冲区
    /// </summary>
    public Framebuffer Framebuffer { get; private set; }

    /// <summary>
    /// 纹理坐标缓冲区
    /// </summary>
    public uint TexCoordBuffer { get; private set; }

    /// <summary>
    /// 顶点坐标缓冲区
    /// </summary>
    public uint VertexBuffer { get; private set; }

    /// <summary>
    /// 矩形坐标缓冲区
    /// </summary>
    public uint RectangleBuffer { get; private set; }

    /// <summary>
    /// 圆形坐标缓冲区
    /// </summary>
    public uint CircleBuffer { get; private set; }

    /// <summary>
    /// 线段坐标缓冲区
    /// </summary>
    public uint LineBuffer { get; private set; }

    /// <summary>
    /// 纯色着色器程序
    /// </summary>
    public ShaderProgram SolidColorProgram { get; private set; }

    public Canvas(GL gl, ShaderHelper shaderHelper, Rectangle<int> rectangle)
    {
        _gl = gl;
        _shaderHelper = shaderHelper;

        Initialization();

        Resize(rectangle);
    }

    /// <summary>
    /// 初始化
    /// </summary>
    private void Initialization()
    {
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

        // 顶点坐标系
        {
            float[] vertices = new float[12];

            VertexBuffer = _gl.GenBuffer();
            _gl.BindBuffer(GLEnum.ArrayBuffer, VertexBuffer);
            _gl.BufferData<float>(GLEnum.ArrayBuffer, (uint)(vertices.Length * sizeof(float)), vertices, GLEnum.DynamicDraw);
            _gl.BindBuffer(GLEnum.ArrayBuffer, 0);
        }

        // 矩形坐标系
        {
            float[] vertices = new float[12];

            RectangleBuffer = _gl.GenBuffer();
            _gl.BindBuffer(GLEnum.ArrayBuffer, RectangleBuffer);
            _gl.BufferData<float>(GLEnum.ArrayBuffer, (uint)(vertices.Length * sizeof(float)), vertices, GLEnum.DynamicDraw);
            _gl.BindBuffer(GLEnum.ArrayBuffer, 0);
        }

        // 圆形坐标系
        {
            float[] vertices = new float[CirclePoints * 3];

            CircleBuffer = _gl.GenBuffer();
            _gl.BindBuffer(GLEnum.ArrayBuffer, CircleBuffer);
            _gl.BufferData<float>(GLEnum.ArrayBuffer, (uint)(vertices.Length * sizeof(float)), vertices, GLEnum.DynamicDraw);
            _gl.BindBuffer(GLEnum.ArrayBuffer, 0);
        }

        // 线段坐标系
        {
            float[] vertices = new float[12];

            LineBuffer = _gl.GenBuffer();
            _gl.BindBuffer(GLEnum.ArrayBuffer, LineBuffer);
            _gl.BufferData<float>(GLEnum.ArrayBuffer, (uint)(vertices.Length * sizeof(float)), vertices, GLEnum.DynamicDraw);
            _gl.BindBuffer(GLEnum.ArrayBuffer, 0);
        }

        SolidColorProgram = new ShaderProgram(_gl);
        SolidColorProgram.Attach(_shaderHelper.GetShader("defaultVertex.vert"), _shaderHelper.GetShader("solidColor.frag"));
    }

    /// <summary>
    /// 调整画板大小
    /// </summary>
    /// <param name="rectangle">位置及大小</param>
    public void Resize(Rectangle<int> rectangle)
    {
        Rectangle = rectangle;

        Size = new Vector2D<uint>((uint)Rectangle.Size.X, (uint)Rectangle.Size.Y);

        Framebuffer?.Dispose();
        Framebuffer = new Framebuffer(_gl, Size);

        float[] vertices = new float[] {
            Rectangle.Origin.X, Rectangle.Origin.Y, 0,
            Rectangle.Origin.X, Rectangle.Max.Y, 0,
            Rectangle.Max.X, Rectangle.Origin.Y, 0,
            Rectangle.Max.X, Rectangle.Max.Y, 0
        };

        _gl.BindBuffer(GLEnum.ArrayBuffer, VertexBuffer);
        _gl.BufferSubData<float>(GLEnum.ArrayBuffer, 0, (uint)(vertices.Length * sizeof(float)), vertices);
        _gl.BindBuffer(GLEnum.ArrayBuffer, 0);

        // SolidColorProgram projectionUniform
        {
            SolidColorProgram.Enable();

            Matrix4x4 projectionMatrix = Matrix4x4.CreateOrthographicOffCenter(0.0f, Size.X, 0.0f, Size.Y, -1.0f, 1.0f);
            _gl.UniformMatrix4(SolidColorProgram.GetUniformLocation("projection"), 1, false, (float*)&projectionMatrix);

            SolidColorProgram.Disable();
        }
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

            _gl.BindFramebuffer(GLEnum.Framebuffer, Framebuffer.MultisampleFbo);
            _gl.Viewport(0, 0, Size.X, Size.Y);

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
            _gl.BindFramebuffer(GLEnum.ReadFramebuffer, Framebuffer.MultisampleFbo);
            _gl.BindFramebuffer(GLEnum.DrawFramebuffer, Framebuffer.DrawFbo);
            _gl.BlitFramebuffer(0, 0, (int)Size.X, (int)Size.Y, 0, 0, (int)Size.X, (int)Size.Y, ClearBufferMask.ColorBufferBit, GLEnum.Nearest);

            _gl.BindFramebuffer(GLEnum.Framebuffer, RecordFbo);
            _gl.Viewport(RecordPosition.X, RecordPosition.Y, RecordSize.X, RecordSize.Y);

            IsDrawing = false;
        }
    }

    /// <summary>
    /// 清空画板（清空颜色由外部控制）
    /// </summary>
    public void Clear()
    {
        Draw(null, () =>
        {
            _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
        });
    }

    /// <summary>
    /// 绘制矩形
    /// </summary>
    /// <param name="rectangle">矩形位置及大小</param>
    /// <param name="color">填充颜色</param>
    public void DrawRectangle(RectangleF rectangle, Color color)
    {
        Draw(SolidColorProgram, () =>
        {
            _gl.Uniform4(SolidColorProgram.GetUniformLocation("solidColor"), color.ToVector4());

            float[] vertices = new float[] {
                rectangle.Left, rectangle.Top, 0.0f,
                rectangle.Left, rectangle.Bottom, 0.0f,
                rectangle.Right, rectangle.Top, 0.0f,
                rectangle.Right, rectangle.Bottom, 0.0f
            };

            _gl.BindBuffer(GLEnum.ArrayBuffer, RectangleBuffer);

            _gl.BufferSubData<float>(GLEnum.ArrayBuffer, 0, (uint)(vertices.Length * sizeof(float)), vertices);
            _gl.VertexAttribPointer((uint)SolidColorProgram.GetAttribLocation("position"), 3, GLEnum.Float, false, 0, null);

            _gl.BindBuffer(GLEnum.ArrayBuffer, 0);

            _gl.DrawArrays(GLEnum.TriangleStrip, 0, 4);
        });
    }

    /// <summary>
    /// 绘制圆形
    /// </summary>
    /// <param name="origin">中心点</param>
    /// <param name="radius">半径</param>
    /// <param name="color">填充颜色</param>
    public void DrawCircle(PointF origin, float radius, Color color)
    {
        Draw(SolidColorProgram, () =>
        {
            _gl.Uniform4(SolidColorProgram.GetUniformLocation("solidColor"), color.ToVector4());

            float[] vertices = new float[CirclePoints * 3];
            for (int i = 0; i < CirclePoints; i++)
            {
                vertices[i * 3] = origin.X + radius * MathF.Cos(2 * MathF.PI * i / CirclePoints);
                vertices[i * 3 + 1] = origin.Y + radius * MathF.Sin(2 * MathF.PI * i / CirclePoints);
                vertices[i * 3 + 2] = 0.0f;
            }

            _gl.BindBuffer(GLEnum.ArrayBuffer, CircleBuffer);

            _gl.BufferSubData<float>(GLEnum.ArrayBuffer, 0, (uint)(vertices.Length * sizeof(float)), vertices);
            _gl.VertexAttribPointer((uint)SolidColorProgram.GetAttribLocation("position"), 3, GLEnum.Float, false, 0, null);

            _gl.BindBuffer(GLEnum.ArrayBuffer, 0);

            _gl.DrawArrays(GLEnum.TriangleFan, 0, CirclePoints);
        });
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
        Draw(SolidColorProgram, () =>
        {
            _gl.Uniform4(SolidColorProgram.GetUniformLocation("solidColor"), color.ToVector4());

            float[] vertices = new float[12];

            float dx = end.X - start.X;
            float dy = end.Y - start.Y;
            float length = MathF.Sqrt(dx * dx + dy * dy);
            float nx = dy / length;
            float ny = -dx / length;

            vertices[0] = start.X + nx * width / 2;
            vertices[1] = start.Y + ny * width / 2;
            vertices[2] = 0.0f;

            vertices[3] = start.X - nx * width / 2;
            vertices[4] = start.Y - ny * width / 2;
            vertices[5] = 0.0f;

            vertices[6] = end.X + nx * width / 2;
            vertices[7] = end.Y + ny * width / 2;
            vertices[8] = 0.0f;

            vertices[9] = end.X - nx * width / 2;
            vertices[10] = end.Y - ny * width / 2;
            vertices[11] = 0.0f;

            _gl.BindBuffer(GLEnum.ArrayBuffer, LineBuffer);

            _gl.BufferSubData<float>(GLEnum.ArrayBuffer, 0, (uint)(vertices.Length * sizeof(float)), vertices);
            _gl.VertexAttribPointer((uint)SolidColorProgram.GetAttribLocation("position"), 3, GLEnum.Float, false, 0, null);

            _gl.BindBuffer(GLEnum.ArrayBuffer, 0);

            _gl.DrawArrays(GLEnum.TriangleStrip, 0, 4);
        });
    }

    /// <summary>
    /// 回收资源
    /// </summary>
    public void Dispose()
    {
        Framebuffer.Dispose();

        _gl.DeleteBuffer(VertexBuffer);
        _gl.DeleteBuffer(TexCoordBuffer);
        _gl.DeleteBuffer(RectangleBuffer);
        _gl.DeleteBuffer(CircleBuffer);

        SolidColorProgram.Dispose();

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 内部绘制使用
    /// 用于判断是否正在绘制，如果正在绘制则使用指定的着色器程序进行绘制。
    /// </summary>
    /// <param name="program">着色器程序</param>
    /// <param name="drawAction">绘制函数</param>
    private void Draw(ShaderProgram program, Action drawAction)
    {
        if (IsDrawing)
        {
            if (program == null)
            {
                drawAction?.Invoke();
            }
            else
            {
                uint positionAttrib = (uint)program.GetAttribLocation("position");

                _gl.EnableVertexAttribArray(positionAttrib);

                program.Enable();

                drawAction?.Invoke();

                program.Disable();

                _gl.DisableVertexAttribArray(positionAttrib);
            }
        }
    }
}
