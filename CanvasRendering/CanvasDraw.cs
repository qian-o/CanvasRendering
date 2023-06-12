using Silk.NET.OpenGLES;
using System.Drawing;
using System.Numerics;

namespace CanvasRendering;

public unsafe class CanvasDraw
{
    public const string Position = "aPosition";

    private readonly GL _gl;
    private readonly ShaderProgram _shaderProgram;

    private Shader currentShader;
    private Shader solidColorShader;

    public CanvasDraw(GL gl, ShaderProgram shaderProgram)
    {
        _gl = gl;
        _shaderProgram = shaderProgram;
    }

    public void DrawRectangle(RectangleF rectangle, Color color)
    {
        if (solidColorShader == null)
        {
            solidColorShader = new Shader(_gl);
            solidColorShader.LoadShader(GLEnum.FragmentShader, "Shaders/solidColor.frag");
        }

        UseShader(solidColorShader);

        _gl.Uniform4(_shaderProgram.GetUniformLocation("solidColor"), ColorToVector4(color));

        float[] vertices = new float[] {
            rectangle.X, rectangle.Y, 0.0f,
            rectangle.X, rectangle.Bottom, 0.0f,
            rectangle.Right, rectangle.Bottom, 0.0f,
            rectangle.Right,  rectangle.Y, 0.0f,
        };

        uint vbo = _gl.GenBuffer();
        _gl.BindBuffer(GLEnum.ArrayBuffer, vbo);
        _gl.BufferData<float>(GLEnum.ArrayBuffer, (uint)(vertices.Length * sizeof(float)), vertices, GLEnum.StaticDraw);

        _gl.VertexAttribPointer((uint)_shaderProgram.GetAttribLocation(Position), 3, GLEnum.Float, false, 0, null);

        _gl.DrawArrays(GLEnum.TriangleFan, 0, 4);

        _gl.DeleteBuffer(vbo);
    }

    public void DrawRectangle(RectangleF rectangle, Color[] colors, float[] ratios, float angle)
    {
    }

    private static Vector4 ColorToVector4(Color color)
    {
        return new Vector4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);
    }

    private void UseShader(Shader shader)
    {
        if (currentShader != shader)
        {
            _shaderProgram.AttachShader(null, shader);

            currentShader = shader;
        }

        _shaderProgram.Use();
    }
}
