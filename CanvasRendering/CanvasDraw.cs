using Silk.NET.OpenGLES;
using System.Collections.Generic;
using System.Drawing;

namespace CanvasRendering;

public unsafe class CanvasDraw
{
    private readonly GL _gl;
    private readonly uint _shaderProgram;
    private readonly uint _positionLocation;

    public CanvasDraw(GL gl, uint shaderProgram, uint positionLocation)
    {
        _gl = gl;
        _shaderProgram = shaderProgram;
        _positionLocation = positionLocation;
    }

    public void DrawRectangle(RectangleF rectangle, Color color)
    {
        _gl.UseProgram(_shaderProgram);

        int useSolidColorLocation = _gl.GetUniformLocation(_shaderProgram, "useSolidColor");
        _gl.Uniform1(useSolidColorLocation, 1);

        int useGradientColorLocation = _gl.GetUniformLocation(_shaderProgram, "useGradientColor");
        _gl.Uniform1(useGradientColorLocation, 0);

        int solidColorLocation = _gl.GetUniformLocation(_shaderProgram, "solidColor");
        _gl.Uniform4(solidColorLocation, color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);

        float[] vertices = new float[] {
            rectangle.X, rectangle.Y, 0.0f,
            rectangle.X, rectangle.Bottom, 0.0f,
            rectangle.Right, rectangle.Bottom, 0.0f,
            rectangle.Right,  rectangle.Y, 0.0f,
        };

        uint vbo = _gl.GenBuffer();
        _gl.BindBuffer(GLEnum.ArrayBuffer, vbo);
        _gl.BufferData<float>(GLEnum.ArrayBuffer, (uint)(vertices.Length * sizeof(float)), vertices, GLEnum.StaticDraw);

        _gl.VertexAttribPointer(_positionLocation, 3, GLEnum.Float, false, 0, null);

        _gl.DrawArrays(GLEnum.TriangleFan, 0, 4);

        _gl.DeleteBuffer(vbo);
    }


    public void DrawRectangle(RectangleF rectangle, Color[] colors, float[] ratios, float angle)
    {
        _gl.UseProgram(_shaderProgram);

        int useSolidColorLocation = _gl.GetUniformLocation(_shaderProgram, "useSolidColor");
        _gl.Uniform1(useSolidColorLocation, 0);

        int useGradientColorLocation = _gl.GetUniformLocation(_shaderProgram, "useGradientColor");
        _gl.Uniform1(useGradientColorLocation, 1);

        int gradientColorsLocation = _gl.GetUniformLocation(_shaderProgram, "gradientColors");
        List<float> colorsF = new(colors.Length * 4);
        foreach (Color color in colors)
        {
            colorsF.Add(color.R / 255.0f);
            colorsF.Add(color.G / 255.0f);
            colorsF.Add(color.B / 255.0f);
            colorsF.Add(color.A / 255.0f);
        }
        float[] colorsA = colorsF.ToArray();
        fixed (float* c = colorsA)
        {
            _gl.Uniform4(gradientColorsLocation, (uint)colors.Length, c);
        }

        int gradientRatiosLocation = _gl.GetUniformLocation(_shaderProgram, "gradientRatios");
        fixed (float* r = ratios)
        {
            _gl.Uniform4(gradientRatiosLocation, (uint)ratios.Length, r);
        }

        int gradientAngleLocation = _gl.GetUniformLocation(_shaderProgram, "gradientAngle");
        _gl.Uniform1(gradientAngleLocation, angle);

        int gradientCountLocation = _gl.GetUniformLocation(_shaderProgram, "gradientCount");
        _gl.Uniform1(gradientCountLocation, colors.Length);


        float[] vertices = new float[] {
            rectangle.X, rectangle.Y, 0.0f,
            rectangle.X, rectangle.Bottom, 0.0f,
            rectangle.Right, rectangle.Bottom, 0.0f,
            rectangle.Right,  rectangle.Y, 0.0f,
        };

        uint vbo = _gl.GenBuffer();
        _gl.BindBuffer(GLEnum.ArrayBuffer, vbo);
        _gl.BufferData<float>(GLEnum.ArrayBuffer, (uint)(vertices.Length * sizeof(float)), vertices, GLEnum.StaticDraw);

        _gl.VertexAttribPointer(_positionLocation, 3, GLEnum.Float, false, 0, null);

        _gl.DrawArrays(GLEnum.TriangleFan, 0, 4);

        _gl.DeleteBuffer(vbo);
    }
}
