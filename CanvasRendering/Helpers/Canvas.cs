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

    public Rectangle<int> Rectangle { get; private set; }

    public Vector2D<uint> Size { get; private set; }

    public Matrix4x4 ProjectionMatrix { get; private set; }

    public Framebuffer Framebuffer { get; private set; }

    public uint TexCoordBuffer { get; private set; }

    public uint VertexBuffer { get; private set; }

    public uint RectangleBuffer { get; private set; }

    public uint CircleBuffer { get; private set; }

    public uint LineBuffer { get; private set; }

    public ShaderProgram SolidColorProgram { get; private set; }

    public Canvas(GL gl, ShaderHelper shaderHelper, Rectangle<int> rectangle)
    {
        _gl = gl;
        _shaderHelper = shaderHelper;

        Initialization();

        Resize(rectangle);
    }

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

    public void Resize(Rectangle<int> rectangle)
    {
        Rectangle = rectangle;

        Size = new Vector2D<uint>((uint)Rectangle.Size.X, (uint)Rectangle.Size.Y);

        ProjectionMatrix = Matrix4x4.CreateOrthographicOffCenter(0.0f, Size.X, 0.0f, Size.Y, -1.0f, 1.0f);

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
    }

    public void Clear(Color color)
    {
        _gl.BindFramebuffer(GLEnum.Framebuffer, Framebuffer.MultisampleFbo);

        _gl.Viewport(0, 0, Size.X, Size.Y);

        _gl.ClearColor(color);
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

        _gl.BindFramebuffer(GLEnum.Framebuffer, 0);
    }

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

    public void Flush()
    {
        _gl.BindFramebuffer(GLEnum.Framebuffer, Framebuffer.MultisampleFbo);

        _gl.BindFramebuffer(GLEnum.ReadFramebuffer, Framebuffer.MultisampleFbo);
        _gl.BindFramebuffer(GLEnum.DrawFramebuffer, Framebuffer.DrawFbo);
        _gl.BlitFramebuffer(0, 0, (int)Size.X, (int)Size.Y, 0, 0, (int)Size.X, (int)Size.Y, ClearBufferMask.ColorBufferBit, GLEnum.Nearest);

        _gl.BindFramebuffer(GLEnum.Framebuffer, 0);
    }

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

    private void Draw(ShaderProgram program, Action action)
    {
        uint positionAttrib = (uint)program.GetAttribLocation("position");

        _gl.BindFramebuffer(GLEnum.Framebuffer, Framebuffer.MultisampleFbo);

        _gl.Viewport(0, 0, Size.X, Size.Y);

        _gl.EnableVertexAttribArray(positionAttrib);

        program.Enable();

        Matrix4x4 projectionMatrix = ProjectionMatrix;
        _gl.UniformMatrix4(program.GetUniformLocation("projection"), 1, false, (float*)&projectionMatrix);

        action?.Invoke();

        program.Disable();

        _gl.DisableVertexAttribArray(positionAttrib);

        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }
}
