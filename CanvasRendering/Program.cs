using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using Silk.NET.Windowing;
using System.Drawing;
using System.Numerics;

namespace CanvasRendering;

internal unsafe class Program
{
    private static IWindow window;
    private static GL gl;
    private static uint shaderProgram;
    private static uint positionLocation;
    private static CanvasDraw canvasDraw;

    static void Main(string[] args)
    {
        WindowOptions options = WindowOptions.Default;
        options.Title = "Texture Rendering";
        options.Size = new Vector2D<int>(800, 600);
        options.API = new GraphicsAPI(ContextAPI.OpenGLES, new APIVersion(3, 2));
        window = Window.Create(options);

        window.Load += Window_Load;
        window.Resize += Window_Resize;
        window.Render += Window_Render;

        window.Run();
    }

    private static void Window_Resize(Vector2D<int> obj)
    {
        gl.Viewport(0, 0, (uint)obj.X, (uint)obj.Y);

        // 创建正交投影矩阵
        Matrix4x4 projection = Matrix4x4.CreateOrthographicOffCenter(0.0f, obj.X, obj.Y, 0.0f, -1.0f, 1.0f);

        // 使用着色器程序
        gl.UseProgram(shaderProgram);

        // 将正交投影矩阵传递给着色器
        int projectionLocation = gl.GetUniformLocation(shaderProgram, "projection");
        gl.UniformMatrix4(projectionLocation, 1, false, (float*)&projection);
    }

    private static void Window_Load()
    {
        gl = GL.GetApi(window);

        // 创建顶点着色器
        uint vs = gl.CreateShader(GLEnum.VertexShader);
        string vs_source = File.ReadAllText("Shaders/canvas.vs");
        gl.ShaderSource(vs, vs_source);
        gl.CompileShader(vs);

        Console.WriteLine($"VertexShader: {gl.GetShaderInfoLog(vs)}");

        // 创建片段着色器
        uint fs = gl.CreateShader(GLEnum.FragmentShader);
        string fs_source = File.ReadAllText("Shaders/canvas.fs");
        gl.ShaderSource(fs, fs_source);
        gl.CompileShader(fs);

        Console.WriteLine($"FragmentShader: {gl.GetShaderInfoLog(fs)}");

        // 创建着色器程序
        shaderProgram = gl.CreateProgram();
        gl.AttachShader(shaderProgram, vs);
        gl.AttachShader(shaderProgram, fs);
        gl.LinkProgram(shaderProgram);

        // 启用顶点属性
        positionLocation = (uint)gl.GetAttribLocation(shaderProgram, "aPosition");
        gl.EnableVertexAttribArray(positionLocation);

        canvasDraw = new CanvasDraw(gl, shaderProgram, positionLocation);

        Window_Resize(new Vector2D<int>(800, 600));
    }

    private static void Window_Render(double obj)
    {
        gl.ClearColor(Color.White);
        gl.Clear(ClearBufferMask.ColorBufferBit);

        canvasDraw.DrawRectangle(new RectangleF(0, 0, 200, 200), Color.Red);

        canvasDraw.DrawRectangle(new RectangleF(500, 0, 200, 200), new Color[] { Color.Red, Color.Blue, Color.PaleGreen }, new float[] { 0.0f, 0.1f, 1.0f }, 80);

        canvasDraw.DrawRectangle(new RectangleF(500, 400, 200, 100), Color.CadetBlue);
    }
}
