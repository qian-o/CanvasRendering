using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using System.Drawing;
using System.Numerics;

namespace CanvasRendering.Helpers;

public unsafe class Canvas : IDisposable
{
    private readonly GL _gl;
    private readonly ShaderHelper _shaderHelper;
    private readonly Rectangle<int> _rectangle;
    private readonly float _scale;
    private readonly Vector2D<uint> _actualSize;

    public Framebuffer Framebuffer { get; }

    public uint VertexBuffer { get; }

    public uint TexCoordBuffer { get; }

    public Canvas(GL gl, ShaderHelper shaderHelper, Rectangle<int> rectangle)
    {
        _gl = gl;
        _shaderHelper = shaderHelper;
        _rectangle = rectangle;

        _gl.GetInteger(GLEnum.MaxTextureSize, out int maxTextureSize);

        if (_rectangle.Size.X * _rectangle.Size.Y > maxTextureSize)
        {
            _scale = MathF.Sqrt((float)maxTextureSize / (_rectangle.Size.X * _rectangle.Size.Y));

            _actualSize = new Vector2D<uint>(Convert.ToUInt32(_rectangle.Size.X * _scale), Convert.ToUInt32(_rectangle.Size.Y * _scale));
        }
        else
        {
            _scale = 1;

            _actualSize = new Vector2D<uint>((uint)_rectangle.Size.X, (uint)_rectangle.Size.Y);
        }

        Framebuffer = new Framebuffer(gl, _actualSize);

        float[] vertices = new float[] {
            _rectangle.Origin.X, _rectangle.Origin.Y, 0,
            _rectangle.Origin.X, _rectangle.Max.Y, 0,
            _rectangle.Max.X, _rectangle.Origin.Y, 0,
            _rectangle.Max.X, _rectangle.Max.Y, 0
        };

        VertexBuffer = _gl.GenBuffer();
        _gl.BindBuffer(GLEnum.ArrayBuffer, VertexBuffer);
        _gl.BufferData<float>(GLEnum.ArrayBuffer, (uint)(vertices.Length * sizeof(float)), vertices, GLEnum.StaticDraw);
        _gl.BindBuffer(GLEnum.ArrayBuffer, 0);

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

    public void Clear(Color color)
    {
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, Framebuffer.Fbo);

        _gl.Viewport(0, 0, _actualSize.X, _actualSize.Y);

        _gl.ClearColor(color);
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    public void DrawRectangle(RectangleF rectangle, Color color)
    {
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, Framebuffer.Fbo);

        _gl.Viewport(0, 0, _actualSize.X, _actualSize.Y);

        ShaderProgram shaderProgram = new(_gl);
        shaderProgram.Attach(_shaderHelper.GetShader("defaultVertex.vert"), _shaderHelper.GetShader("solidColor.frag"));

        uint positionAttrib = (uint)_gl.GetAttribLocation(shaderProgram.Id, "position");

        float[] vertices = new float[] {
            rectangle.Left, rectangle.Top, 0.0f,
            rectangle.Left, rectangle.Bottom, 0.0f,
            rectangle.Right, rectangle.Top, 0.0f,
            rectangle.Right, rectangle.Bottom, 0.0f
        };

        uint vbo = _gl.GenBuffer();

        _gl.BindBuffer(GLEnum.ArrayBuffer, vbo);
        _gl.BufferData<float>(GLEnum.ArrayBuffer, (uint)(vertices.Length * sizeof(float)), vertices, GLEnum.StaticDraw);
        _gl.BindBuffer(GLEnum.ArrayBuffer, 0);

        _gl.EnableVertexAttribArray(positionAttrib);

        _gl.BindBuffer(GLEnum.ArrayBuffer, vbo);
        _gl.VertexAttribPointer(positionAttrib, 3, GLEnum.Float, false, 0, null);

        _gl.UseProgram(shaderProgram.Id);

        Matrix4x4 projection = Matrix4x4.CreateOrthographicOffCenter(0.0f, _actualSize.X, 0.0f, _actualSize.Y, -1.0f, 1.0f);

        _gl.UniformMatrix4(_gl.GetUniformLocation(shaderProgram.Id, "projection"), 1, false, (float*)&projection);

        _gl.Uniform4(_gl.GetUniformLocation(shaderProgram.Id, "solidColor"), color.ToVector4());

        _gl.DrawArrays(GLEnum.TriangleStrip, 0, 4);

        _gl.UseProgram(0);

        _gl.BindBuffer(GLEnum.ArrayBuffer, 0);

        _gl.DisableVertexAttribArray(positionAttrib);

        _gl.DeleteBuffer(vbo);

        _gl.DeleteProgram(shaderProgram.Id);

        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    public void DrawCircle(PointF origin, float radius, uint points, Color color)
    {
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, Framebuffer.Fbo);

        _gl.Viewport(0, 0, _actualSize.X, _actualSize.Y);

        ShaderProgram shaderProgram = new(_gl);
        shaderProgram.Attach(_shaderHelper.GetShader("defaultVertex.vert"), _shaderHelper.GetShader("solidColor.frag"));

        uint positionAttrib = (uint)_gl.GetAttribLocation(shaderProgram.Id, "position");

        float[] vertices = new float[points * 3];

        for (int i = 0; i < points; i++)
        {
            vertices[i * 3] = origin.X + radius * MathF.Cos(2 * MathF.PI * i / points);
            vertices[i * 3 + 1] = origin.Y + radius * MathF.Sin(2 * MathF.PI * i / points);
            vertices[i * 3 + 2] = 0.0f;
        }

        uint vbo = _gl.GenBuffer();

        _gl.BindBuffer(GLEnum.ArrayBuffer, vbo);
        _gl.BufferData<float>(GLEnum.ArrayBuffer, (uint)(vertices.Length * sizeof(float)), vertices, GLEnum.StaticDraw);
        _gl.BindBuffer(GLEnum.ArrayBuffer, 0);

        _gl.EnableVertexAttribArray(positionAttrib);

        _gl.BindBuffer(GLEnum.ArrayBuffer, vbo);
        _gl.VertexAttribPointer(positionAttrib, 3, GLEnum.Float, false, 0, null);

        _gl.UseProgram(shaderProgram.Id);

        Matrix4x4 projection = Matrix4x4.CreateOrthographicOffCenter(0.0f, _actualSize.X, 0.0f, _actualSize.Y, -1.0f, 1.0f);

        _gl.UniformMatrix4(_gl.GetUniformLocation(shaderProgram.Id, "projection"), 1, false, (float*)&projection);

        _gl.Uniform4(_gl.GetUniformLocation(shaderProgram.Id, "solidColor"), color.ToVector4());

        _gl.DrawArrays(GLEnum.TriangleFan, 0, points);

        _gl.UseProgram(0);

        _gl.BindBuffer(GLEnum.ArrayBuffer, 0);

        _gl.DisableVertexAttribArray(positionAttrib);

        _gl.DeleteBuffer(vbo);

        _gl.DeleteProgram(shaderProgram.Id);

        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    public void Flush()
    {
        _gl.BindFramebuffer(GLEnum.Framebuffer, Framebuffer.Fbo);

        _gl.BindFramebuffer(GLEnum.ReadFramebuffer, Framebuffer.Fbo);
        _gl.BindFramebuffer(GLEnum.DrawFramebuffer, Framebuffer.TexFbo);
        _gl.BlitFramebuffer(0, 0, (int)_actualSize.X, (int)_actualSize.Y, 0, 0, (int)_actualSize.X, (int)_actualSize.Y, ClearBufferMask.ColorBufferBit, GLEnum.Nearest);

        _gl.BindFramebuffer(GLEnum.Framebuffer, 0);
    }

    public void Dispose()
    {
        Framebuffer.Dispose();

        _gl.DeleteBuffer(VertexBuffer);
        _gl.DeleteBuffer(TexCoordBuffer);

        GC.SuppressFinalize(this);
    }
}
