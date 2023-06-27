using CanvasRendering.Shaders;
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
    /// 字符串坐标缓冲区
    /// </summary>
    public uint StringBuffer { get; private set; }

    /// <summary>
    /// 纯色着色器程序
    /// </summary>
    public ShaderProgram SolidColorProgram { get; private set; }

    /// <summary>
    /// 纹理着色器程序
    /// </summary>
    public ShaderProgram TextureProgram { get; private set; }

    public Canvas(GL gl, ShaderHelper shaderHelper, Vector2D<uint> size)
    {
        _gl = gl;
        _shaderHelper = shaderHelper;
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
            float[] vertices = new float[6];

            LineBuffer = _gl.GenBuffer();
            _gl.BindBuffer(GLEnum.ArrayBuffer, LineBuffer);
            _gl.BufferData<float>(GLEnum.ArrayBuffer, (uint)(vertices.Length * sizeof(float)), vertices, GLEnum.DynamicDraw);
            _gl.BindBuffer(GLEnum.ArrayBuffer, 0);
        }

        // 字符串坐标系
        {
            StringBuffer = _gl.GenBuffer();
        }

        // SolidColorProgram
        {
            SolidColorProgram = new ShaderProgram(_gl);
            SolidColorProgram.Attach(_shaderHelper.GetShader(DefaultVertex.Name), _shaderHelper.GetShader(SolidColorFragment.Name));

            SolidColorProgram.Enable();

            Matrix4x4 projectionMatrix = Matrix4x4.CreateOrthographicOffCenter(0.0f, Size.X, 0.0f, Size.Y, -1.0f, 1.0f);
            _gl.UniformMatrix4(SolidColorProgram.GetUniformLocation(DefaultVertex.ProjectionUniform), 1, false, (float*)&projectionMatrix);

            SolidColorProgram.Disable();
        }

        // TextureProgram
        {
            TextureProgram = new ShaderProgram(_gl);
            TextureProgram.Attach(_shaderHelper.GetShader(DefaultVertex.Name), _shaderHelper.GetShader(TextureFragment.Name));

            TextureProgram.Enable();

            Matrix4x4 projectionMatrix = Matrix4x4.CreateOrthographicOffCenter(0.0f, Size.X, 0.0f, Size.Y, -1.0f, 1.0f);
            _gl.UniformMatrix4(TextureProgram.GetUniformLocation(DefaultVertex.ProjectionUniform), 1, false, (float*)&projectionMatrix);

            TextureProgram.Disable();
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
            _gl.Uniform4(SolidColorProgram.GetUniformLocation(SolidColorFragment.SolidColorUniform), color.ToVector4());

            float[] vertices = new float[] {
                rectangle.Left, rectangle.Top, 0.0f,
                rectangle.Left, rectangle.Bottom, 0.0f,
                rectangle.Right, rectangle.Top, 0.0f,
                rectangle.Right, rectangle.Bottom, 0.0f
            };

            _gl.BindBuffer(GLEnum.ArrayBuffer, RectangleBuffer);

            _gl.BufferSubData<float>(GLEnum.ArrayBuffer, 0, (uint)(vertices.Length * sizeof(float)), vertices);
            _gl.VertexAttribPointer((uint)SolidColorProgram.GetAttribLocation(DefaultVertex.PositionAttrib), 3, GLEnum.Float, false, 0, null);

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
            _gl.Uniform4(SolidColorProgram.GetUniformLocation(SolidColorFragment.SolidColorUniform), color.ToVector4());

            float[] vertices = new float[CirclePoints * 3];
            for (int i = 0; i < CirclePoints; i++)
            {
                vertices[i * 3] = origin.X + radius * MathF.Cos(2 * MathF.PI * i / CirclePoints);
                vertices[i * 3 + 1] = origin.Y + radius * MathF.Sin(2 * MathF.PI * i / CirclePoints);
                vertices[i * 3 + 2] = 0.0f;
            }

            _gl.BindBuffer(GLEnum.ArrayBuffer, CircleBuffer);

            _gl.BufferSubData<float>(GLEnum.ArrayBuffer, 0, (uint)(vertices.Length * sizeof(float)), vertices);
            _gl.VertexAttribPointer((uint)SolidColorProgram.GetAttribLocation(DefaultVertex.PositionAttrib), 3, GLEnum.Float, false, 0, null);

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
            _gl.Uniform4(SolidColorProgram.GetUniformLocation(SolidColorFragment.SolidColorUniform), color.ToVector4());

            float[] vertices = new float[6];

            vertices[0] = start.X;
            vertices[1] = start.Y;
            vertices[2] = 0.0f;

            vertices[3] = end.X;
            vertices[4] = end.Y;
            vertices[5] = 0.0f;

            _gl.BindBuffer(GLEnum.ArrayBuffer, LineBuffer);

            _gl.BufferSubData<float>(GLEnum.ArrayBuffer, 0, (uint)(vertices.Length * sizeof(float)), vertices);
            _gl.VertexAttribPointer((uint)SolidColorProgram.GetAttribLocation(DefaultVertex.PositionAttrib), 3, GLEnum.Float, false, 0, null);

            _gl.BindBuffer(GLEnum.ArrayBuffer, 0);

            _gl.LineWidth(width);
            _gl.DrawArrays(GLEnum.Lines, 0, 2);
        });
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
        Draw(SolidColorProgram, () =>
        {
            _gl.Viewport(point.X, point.Y, Size.X, Size.Y);

            _gl.Uniform4(SolidColorProgram.GetUniformLocation(SolidColorFragment.SolidColorUniform), color.ToVector4());

            _gl.BindBuffer(GLEnum.ArrayBuffer, StringBuffer);

            foreach ((float[] vertices, uint vertexCount) in GlyphHelper.GetVboData(text, size, fontPath))
            {
                _gl.BufferData<float>(GLEnum.ArrayBuffer, (uint)(vertices.Length * sizeof(float)), vertices, GLEnum.DynamicDraw);
                _gl.VertexAttribPointer((uint)SolidColorProgram.GetAttribLocation(DefaultVertex.PositionAttrib), 2, GLEnum.Float, false, 0, null);

                _gl.DrawArrays(GLEnum.Triangles, 0, vertexCount);
            }

            _gl.BindBuffer(GLEnum.ArrayBuffer, 0);

            _gl.Viewport(0, 0, Size.X, Size.Y);
        });
    }

    /// <summary>
    /// 绘制画板
    /// </summary>
    /// <param name="canvas">画板</param>
    public void DrawCanvas(Canvas canvas, Rectangle<int> rectangle, bool clipToBounds)
    {
        Draw(TextureProgram, () =>
        {
            uint width, height;
            if (clipToBounds)
            {
                width = (uint)rectangle.Size.X;
                height = (uint)rectangle.Size.Y;
            }
            else
            {
                width = canvas.Size.X;
                height = canvas.Size.Y;
            }

            _gl.Enable(GLEnum.ScissorTest);

            _gl.Scissor(rectangle.Origin.X, rectangle.Origin.X, width, height);

            canvas.UpdateVertexBuffer(new Rectangle<float>(rectangle.Origin.X, rectangle.Origin.Y, canvas.Size.X, canvas.Size.Y));

            _gl.BindBuffer(GLEnum.ArrayBuffer, canvas.VertexBuffer);
            _gl.VertexAttribPointer((uint)TextureProgram.GetAttribLocation(DefaultVertex.PositionAttrib), 3, GLEnum.Float, false, 0, null);

            _gl.BindBuffer(GLEnum.ArrayBuffer, canvas.TexCoordBuffer);
            _gl.VertexAttribPointer((uint)TextureProgram.GetAttribLocation(DefaultVertex.TexCoordAttrib), 2, GLEnum.Float, false, 0, null);

            _gl.ActiveTexture(GLEnum.Texture0);
            _gl.BindTexture(GLEnum.Texture2D, canvas.Framebuffer.DrawTexture);
            _gl.Uniform1(TextureProgram.GetUniformLocation(TextureFragment.TexUniform), 0);

            _gl.DrawArrays(GLEnum.TriangleStrip, 0, 4);

            _gl.BindTexture(GLEnum.Texture2D, 0);

            _gl.Disable(GLEnum.ScissorTest);
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
        _gl.DeleteBuffer(LineBuffer);

        SolidColorProgram.Dispose();
        TextureProgram.Dispose();

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
            else if (program == SolidColorProgram)
            {
                uint positionAttrib = (uint)program.GetAttribLocation(DefaultVertex.PositionAttrib);

                _gl.EnableVertexAttribArray(positionAttrib);

                program.Enable();

                drawAction?.Invoke();

                program.Disable();

                _gl.DisableVertexAttribArray(positionAttrib);
            }
            else if (program == TextureProgram)
            {
                uint positionAttrib = (uint)program.GetAttribLocation(DefaultVertex.PositionAttrib);
                uint texCoordAttrib = (uint)program.GetAttribLocation(DefaultVertex.TexCoordAttrib);

                _gl.EnableVertexAttribArray(positionAttrib);
                _gl.EnableVertexAttribArray(texCoordAttrib);

                program.Enable();

                drawAction?.Invoke();

                program.Disable();

                _gl.DisableVertexAttribArray(positionAttrib);
                _gl.DisableVertexAttribArray(texCoordAttrib);
            }
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
    }

    /// <summary>
    /// 在窗口上绘制画板
    /// </summary>
    /// <param name="gl">gl上下文</param>
    /// <param name="textureProgram">纹理着色器程序</param>
    /// <param name="canvas">画板</param>
    public static void DrawOnWindow(GL gl, ShaderProgram textureProgram, Canvas canvas)
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
        gl.BindTexture(GLEnum.Texture2D, canvas.Framebuffer.DrawTexture);
        gl.Uniform1(textureProgram.GetUniformLocation(TextureFragment.TexUniform), 0);

        gl.DrawArrays(GLEnum.TriangleStrip, 0, 4);

        gl.BindTexture(GLEnum.Texture2D, 0);

        gl.BindBuffer(GLEnum.ArrayBuffer, 0);

        textureProgram.Disable();

        gl.DisableVertexAttribArray(positionAttrib);
        gl.DisableVertexAttribArray(texCoordAttrib);
    }
}
