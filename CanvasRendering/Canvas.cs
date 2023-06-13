﻿using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using System.Drawing;
using System.Numerics;

namespace CanvasRendering;

public unsafe class Canvas
{
    private readonly GL _gl;
    private readonly Rectangle<int> _rectangle;

    private readonly Shader vs;
    private readonly Shader fs;
    private readonly ShaderProgram program;
    private Vector2D<int> actualSize;

    public uint Texture { get; private set; }

    public uint Fbo { get; private set; }

    public Canvas(GL gl, Rectangle<int> rectangle)
    {
        _gl = gl;
        _rectangle = rectangle;

        // 创建顶点着色器
        vs = new(gl);
        vs.LoadShader(GLEnum.VertexShader, "Shaders/canvas.vert");

        // 创建片段着色器
        fs = new(gl);
        fs.LoadShader(GLEnum.FragmentShader, "Shaders/solidColor.frag");

        program = new ShaderProgram(gl);
        program.AttachShader(vs, fs);

        LoadFrame();
    }

    public void Draw(Color color)
    {
        // 绑定FBO对象和纹理对象，以便将渲染结果存储到对应的纹理中。
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, Fbo);

        _gl.ClearColor(Color.White);
        _gl.Clear(ClearBufferMask.ColorBufferBit);

        program.Use();

        uint positionAttribute = (uint)program.GetAttribLocation("position");

        _gl.EnableVertexAttribArray(positionAttribute);

        // 创建正交投影矩阵
        Matrix4x4 projection = Matrix4x4.CreateOrthographicOffCenter(0.0f, (uint)actualSize.X, (uint)actualSize.Y, 0.0f, -1.0f, 1.0f);

        // 将正交投影矩阵传递给着色器
        _gl.UniformMatrix4(program.GetUniformLocation("projection"), 1, false, (float*)&projection);

        _gl.Uniform4(program.GetUniformLocation("solidColor"), ColorToVector4(color));

        float[] vertices = new float[] {
            0, 0,
            0, actualSize.Y,
            actualSize.X, actualSize.Y,
            actualSize.X,  0
        };

        uint vbo = _gl.GenBuffer();
        _gl.BindBuffer(GLEnum.ArrayBuffer, vbo);
        _gl.BufferData<float>(GLEnum.ArrayBuffer, (uint)(vertices.Length * sizeof(float)), vertices, GLEnum.StaticDraw);

        _gl.VertexAttribPointer(positionAttribute, 2, GLEnum.Float, false, 0, null);

        _gl.DrawArrays(GLEnum.TriangleFan, 0, 4);

        _gl.DeleteBuffer(vbo);

        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    private void LoadFrame()
    {
        _gl.GetInteger(GLEnum.ImplementationColorReadFormat, out int format);
        _gl.GetInteger(GLEnum.ImplementationColorReadType, out int type);
        _gl.GetInteger(GLEnum.MaxTextureSize, out int maxTextureSize);

        if (_rectangle.Size.X * _rectangle.Size.Y > maxTextureSize)
        {
            double scale = Math.Sqrt((double)maxTextureSize / (_rectangle.Size.X * _rectangle.Size.Y));

            actualSize = new Vector2D<int>(Convert.ToInt32(_rectangle.Size.X * scale), Convert.ToInt32(_rectangle.Size.Y * scale));
        }
        else
        {
            actualSize = new Vector2D<int>(_rectangle.Size.X, _rectangle.Size.Y);
        }

        Texture = _gl.GenTexture();
        _gl.BindTexture(TextureTarget.Texture2D, Texture);
        _gl.TexImage2D(GLEnum.Texture2D, 0, (int)InternalFormat.Rgba, (uint)actualSize.X, (uint)actualSize.Y, 0, (PixelFormat)format, (PixelType)type, IntPtr.Zero);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        _gl.BindTexture(TextureTarget.Texture2D, 0);

        Fbo = _gl.GenFramebuffer();
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, Fbo);
        _gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, Texture, 0);
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    private static Vector4 ColorToVector4(Color color)
    {
        return new Vector4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);
    }
}