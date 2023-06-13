using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using Silk.NET.Windowing;
using System;
using System.Drawing;
using System.Numerics;

namespace CanvasRendering;

internal unsafe class Program
{
    private static IWindow window;
    private static GL gl;
    private static ShaderProgram shaderProgram;

    private static Canvas leftTop;
    private static Canvas rightBottom;

    static void Main(string[] args)
    {
        WindowOptions options = WindowOptions.Default;
        options.Samples = 2;
        options.VSync = true;
        options.Title = "Texture Rendering";
        options.Size = new Vector2D<int>(800, 600);
        options.API = new GraphicsAPI(ContextAPI.OpenGLES, new APIVersion(3, 2));
        window = Window.Create(options);

        window.Load += Window_Load;
        window.Resize += _ => UpdateCanvas();
        window.Render += Window_Render;

        window.Run();
    }

    private static void Window_Load()
    {
        gl = GL.GetApi(window);

        // 创建顶点着色器
        Shader vs = new(gl);
        vs.LoadShader(GLEnum.VertexShader, "Shaders/canvas.vert");

        // 创建片段着色器
        Shader fs = new(gl);
        fs.LoadShader(GLEnum.FragmentShader, "Shaders/texture.frag");

        // 创建着色器程序
        shaderProgram = new ShaderProgram(gl);
        shaderProgram.UpdateAttach += UpdateCanvas;
        shaderProgram.AttachShader(vs, fs);

        // 启用顶点属性
        gl.EnableVertexAttribArray((uint)shaderProgram.GetAttribLocation("position"));

        UpdateCanvas();
    }

    private static void Window_Render(double obj)
    {
        gl.ClearColor(Color.White);
        gl.Clear(ClearBufferMask.ColorBufferBit);

        // 左上角
        {
            leftTop ??= new(gl, new Rectangle<int>(100, 100, 200, 200));

            leftTop.Draw(Color.Indigo);

            shaderProgram.Use();

            uint positionAttribute = (uint)shaderProgram.GetAttribLocation("position");

            gl.Uniform2(shaderProgram.GetUniformLocation("framePosition"), 100.0f, 100.0f);
            gl.ActiveTexture(TextureUnit.Texture0);
            gl.BindTexture(TextureTarget.Texture2D, leftTop.Texture);
            gl.Uniform1(shaderProgram.GetUniformLocation("tex"), 0);

            float[] vertices = new float[] {
                0, 0,
                0, 200,
                200, 200,
                200,  0
            };

            uint vbo = gl.GenBuffer();
            gl.BindBuffer(GLEnum.ArrayBuffer, vbo);
            gl.BufferData<float>(GLEnum.ArrayBuffer, (uint)(vertices.Length * sizeof(float)), vertices, GLEnum.StaticDraw);

            gl.VertexAttribPointer(positionAttribute, 2, GLEnum.Float, false, 0, null);

            gl.DrawArrays(GLEnum.TriangleFan, 0, 4);
        }

        // 右下角
        {
            rightBottom ??= new(gl, new Rectangle<int>(window.Size.X - 200, window.Size.Y - 100, 200, 100));

            rightBottom.Draw(Color.Blue);

            shaderProgram.Use();

            uint positionAttribute = (uint)shaderProgram.GetAttribLocation("position");

            gl.Uniform2(shaderProgram.GetUniformLocation("framePosition"), Convert.ToSingle(window.Size.X - 200), Convert.ToSingle(window.Size.Y - 100));
            gl.ActiveTexture(TextureUnit.Texture0);
            gl.BindTexture(TextureTarget.Texture2D, rightBottom.Texture);
            gl.Uniform1(shaderProgram.GetUniformLocation("tex"), 0);

            float[] vertices = new float[] {
                0, 0,
                0, 200,
                200, 200,
                200,  0
            };

            uint vbo = gl.GenBuffer();
            gl.BindBuffer(GLEnum.ArrayBuffer, vbo);
            gl.BufferData<float>(GLEnum.ArrayBuffer, (uint)(vertices.Length * sizeof(float)), vertices, GLEnum.StaticDraw);

            gl.VertexAttribPointer(positionAttribute, 2, GLEnum.Float, false, 0, null);

            gl.DrawArrays(GLEnum.TriangleFan, 0, 4);
        }

        window.SwapBuffers();
    }

    private static void UpdateCanvas()
    {
        uint width = (uint)window.Size.X;
        uint height = (uint)window.Size.Y;

        gl.Viewport(0, 0, width, height);

        // 创建正交投影矩阵
        Matrix4x4 projection = Matrix4x4.CreateOrthographicOffCenter(0.0f, width, height, 0.0f, -1.0f, 1.0f);

        // 使用着色器程序
        shaderProgram.Use();

        // 将正交投影矩阵传递给着色器
        gl.UniformMatrix4(shaderProgram.GetUniformLocation("projection"), 1, false, (float*)&projection);
    }
}