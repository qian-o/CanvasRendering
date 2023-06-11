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
    private static ShaderProgram shaderProgram;
    private static CanvasDraw canvasDraw;

    static void Main(string[] args)
    {
        WindowOptions options = WindowOptions.Default;
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
        fs.LoadShader(GLEnum.FragmentShader, "Shaders/red.frag");

        // 创建着色器程序
        shaderProgram = new ShaderProgram(gl);
        shaderProgram.UpdateAttach += UpdateCanvas;
        shaderProgram.AttachShader(vs, fs);

        // 启用顶点属性
        gl.EnableVertexAttribArray((uint)shaderProgram.GetAttribLocation("aPosition"));

        canvasDraw = new CanvasDraw(gl, shaderProgram);

        UpdateCanvas();
    }

    private static void Window_Render(double obj)
    {
        gl.ClearColor(Color.White);
        gl.Clear(ClearBufferMask.ColorBufferBit);

        canvasDraw.DrawRectangle(new RectangleF(0, 0, 200, 200), Color.Red);

        canvasDraw.DrawRectangle(new RectangleF(500, 0, 200, 200), new Color[] { Color.Red, Color.Blue, Color.PaleGreen }, new float[] { 0.0f, 0.1f, 1.0f }, 80);

        canvasDraw.DrawRectangle(new RectangleF(500, 400, 200, 100), Color.CadetBlue);
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