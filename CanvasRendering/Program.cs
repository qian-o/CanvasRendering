using CanvasRendering.Helpers;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using Silk.NET.Windowing;
using System.Drawing;
using System.Numerics;
using Shader = CanvasRendering.Helpers.Shader;

namespace CanvasRendering;

internal unsafe class Program
{
    private static IWindow window;
    private static GL gl;
    private static Shader defaultVertex;
    private static Shader solidColor;
    private static ShaderProgram shaderProgram;
    private static uint positionAttrib;
    private static uint vbo;

    static void Main(string[] args)
    {
        Console.WriteLine(args);

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

        defaultVertex = new Shader(gl, GLEnum.VertexShader, File.ReadAllText("Shaders/defaultVertex.vert"));
        solidColor = new Shader(gl, GLEnum.FragmentShader, File.ReadAllText("Shaders/solidColor.frag"));

        shaderProgram = new ShaderProgram(gl);
        shaderProgram.Attach(defaultVertex, solidColor);

        positionAttrib = (uint)gl.GetAttribLocation(shaderProgram.Id, "position");

        float[] vertices = new float[] {
            -0.5f, -0.5f, 0.0f,
            0.5f, -0.5f, 0.0f,
            -0.5f,  0.5f, 0.0f,
            0.5f,  0.5f, 0.0f
        };

        vbo = gl.GenBuffer();

        gl.BindBuffer(GLEnum.ArrayBuffer, vbo);
        gl.BufferData<float>(GLEnum.ArrayBuffer, (uint)(vertices.Length * sizeof(float)), vertices, BufferUsageARB.StaticDraw);
        gl.BindBuffer(GLEnum.ArrayBuffer, 0);

        UpdateCanvas();
    }

    private static void Window_Render(double obj)
    {
        gl.ClearColor(Color.White);
        gl.Clear(ClearBufferMask.ColorBufferBit);

        gl.EnableVertexAttribArray(positionAttrib);

        gl.BindBuffer(GLEnum.ArrayBuffer, vbo);
        gl.VertexAttribPointer(positionAttrib, 3, GLEnum.Float, false, 0, null);

        gl.UseProgram(shaderProgram.Id);

        gl.Uniform4(gl.GetUniformLocation(shaderProgram.Id, "solidColor"), ColorToVector4(Color.MediumSlateBlue));

        gl.DrawArrays(GLEnum.TriangleStrip, 0, 3);

        gl.DisableVertexAttribArray(positionAttrib);

        window.SwapBuffers();
    }

    private static void UpdateCanvas()
    {
        uint width = (uint)window.Size.X;
        uint height = (uint)window.Size.Y;

        gl.Viewport(0, 0, width, height);
    }

    private static Vector4 ColorToVector4(Color color)
    {
        return new Vector4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);
    }
}