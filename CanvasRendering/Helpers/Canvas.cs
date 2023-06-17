using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using System.Drawing;
using System.Numerics;

namespace CanvasRendering.Helpers;

public unsafe class Canvas : IDisposable
{
    private readonly GL _gl;
    private readonly ShaderHelper _shaderHelper;

    public Rectangle<int> Rectangle { get; private set; }

    public Vector2D<uint> Size { get; private set; }

    public Framebuffer Framebuffer { get; private set; }

    public uint TexCoordBuffer { get; private set; }

    public uint VertexBuffer { get; private set; }

    public uint RectangleBuffer { get; private set; }

    public uint CircleBuffer { get; private set; }

    public ShaderProgram RectangleProgram { get; private set; }

    public ShaderProgram CircleProgram { get; private set; }

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
            float[] rectangleVertices = new float[12];

            RectangleBuffer = _gl.GenBuffer();
            _gl.BindBuffer(GLEnum.ArrayBuffer, RectangleBuffer);
            _gl.BufferData<float>(GLEnum.ArrayBuffer, (uint)(rectangleVertices.Length * sizeof(float)), rectangleVertices, GLEnum.DynamicDraw);
            _gl.BindBuffer(GLEnum.ArrayBuffer, 0);
        }

        // 圆形坐标系
        {
            float[] circleVertices = new float[360];

            CircleBuffer = _gl.GenBuffer();
            _gl.BindBuffer(GLEnum.ArrayBuffer, CircleBuffer);
            _gl.BufferData<float>(GLEnum.ArrayBuffer, (uint)(circleVertices.Length * sizeof(float)), circleVertices, GLEnum.DynamicDraw);
            _gl.BindBuffer(GLEnum.ArrayBuffer, 0);
        }

        RectangleProgram = new ShaderProgram(_gl);
        RectangleProgram.Attach(_shaderHelper.GetShader("defaultVertex.vert"), _shaderHelper.GetShader("solidColor.frag"));

        CircleProgram = new ShaderProgram(_gl);
        CircleProgram.Attach(_shaderHelper.GetShader("defaultVertex.vert"), _shaderHelper.GetShader("solidColor.frag"));
    }

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
    }

    public void Clear(Color color)
    {
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, Framebuffer.Fbo);

        _gl.Viewport(0, 0, Size.X, Size.Y);

        _gl.ClearColor(color);
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    public void DrawRectangle(RectangleF rectangle, Color color)
    {
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, Framebuffer.Fbo);

        _gl.Viewport(0, 0, Size.X, Size.Y);

        uint positionAttrib = (uint)RectangleProgram.GetAttribLocation("position");

        float[] vertices = new float[] {
            rectangle.Left, rectangle.Top, 0.0f,
            rectangle.Left, rectangle.Bottom, 0.0f,
            rectangle.Right, rectangle.Top, 0.0f,
            rectangle.Right, rectangle.Bottom, 0.0f
        };

        _gl.BindBuffer(GLEnum.ArrayBuffer, RectangleBuffer);
        _gl.BufferSubData<float>(GLEnum.ArrayBuffer, 0, (uint)(vertices.Length * sizeof(float)), vertices);

        _gl.EnableVertexAttribArray(positionAttrib);

        _gl.VertexAttribPointer(positionAttrib, 3, GLEnum.Float, false, 0, null);

        RectangleProgram.Enable();

        Matrix4x4 projection = Matrix4x4.CreateOrthographicOffCenter(0.0f, Size.X, 0.0f, Size.Y, -1.0f, 1.0f);

        _gl.UniformMatrix4(RectangleProgram.GetUniformLocation("projection"), 1, false, (float*)&projection);

        _gl.Uniform4(RectangleProgram.GetUniformLocation("solidColor"), color.ToVector4());

        _gl.DrawArrays(GLEnum.TriangleStrip, 0, 4);

        RectangleProgram.Disable();

        _gl.BindBuffer(GLEnum.ArrayBuffer, 0);

        _gl.DisableVertexAttribArray(positionAttrib);

        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    public void DrawCircle(PointF origin, float radius, Color color)
    {
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, Framebuffer.Fbo);

        _gl.Viewport(0, 0, Size.X, Size.Y);

        uint positionAttrib = (uint)CircleProgram.GetAttribLocation("position");

        float[] vertices = new float[360];

        for (int i = 0; i < 120; i++)
        {
            vertices[i * 3] = origin.X + radius * MathF.Cos(2 * MathF.PI * i / 120);
            vertices[i * 3 + 1] = origin.Y + radius * MathF.Sin(2 * MathF.PI * i / 120);
            vertices[i * 3 + 2] = 0.0f;
        }

        _gl.BindBuffer(GLEnum.ArrayBuffer, CircleBuffer);
        _gl.BufferSubData<float>(GLEnum.ArrayBuffer, 0, (uint)(vertices.Length * sizeof(float)), vertices);

        _gl.EnableVertexAttribArray(positionAttrib);

        _gl.VertexAttribPointer(positionAttrib, 3, GLEnum.Float, false, 0, null);

        CircleProgram.Enable();

        Matrix4x4 projection = Matrix4x4.CreateOrthographicOffCenter(0.0f, Size.X, 0.0f, Size.Y, -1.0f, 1.0f);

        _gl.UniformMatrix4(CircleProgram.GetUniformLocation("projection"), 1, false, (float*)&projection);

        _gl.Uniform4(CircleProgram.GetUniformLocation("solidColor"), color.ToVector4());

        _gl.DrawArrays(GLEnum.TriangleFan, 0, 120);

        CircleProgram.Disable();

        _gl.BindBuffer(GLEnum.ArrayBuffer, 0);

        _gl.DisableVertexAttribArray(positionAttrib);

        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    public void Flush()
    {
        _gl.BindFramebuffer(GLEnum.Framebuffer, Framebuffer.Fbo);

        _gl.BindFramebuffer(GLEnum.ReadFramebuffer, Framebuffer.Fbo);
        _gl.BindFramebuffer(GLEnum.DrawFramebuffer, Framebuffer.TexFbo);
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

        RectangleProgram.Dispose();
        CircleProgram.Dispose();

        GC.SuppressFinalize(this);
    }
}
