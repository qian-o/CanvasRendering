using CanvasRendering.Helpers;
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
    private static ShaderHelper shderHelper;
    private static ShaderProgram shaderProgram;
    private static uint positionAttrib;
    private static uint vbo;
    private static int width = 800, height = 600;

    static void Main(string[] args)
    {
        Console.WriteLine(args);

        WindowOptions options = WindowOptions.Default;
        options.Samples = 2;
        options.VSync = true;
        options.Title = "Texture Rendering";
        options.Size = new Vector2D<int>(width, height);
        options.API = new GraphicsAPI(ContextAPI.OpenGLES, new APIVersion(3, 2));
        window = Window.Create(options);

        window.Load += Window_Load;
        window.Resize += Window_Resize;
        window.Render += Window_Render;

        window.Run();
    }

    private static void Window_Load()
    {
        gl = window.CreateOpenGLES();

        shderHelper = new ShaderHelper(gl);

        shaderProgram = new ShaderProgram(gl);
        shaderProgram.Attach(shderHelper.GetShader("defaultVertex.vert"), shderHelper.GetShader("solidColor.frag"));

        positionAttrib = (uint)gl.GetAttribLocation(shaderProgram.Id, "position");

        float[] vertices = new float[] {
            10.0f, 10.0f, 0.0f,
            10.0f, 110.0f, 0.0f,
            110.0f, 10.0f, 0.0f,
            110.0f, 110.0f, 0.0f
        };

        vbo = gl.GenBuffer();

        gl.BindBuffer(GLEnum.ArrayBuffer, vbo);
        gl.BufferData<float>(GLEnum.ArrayBuffer, (uint)(vertices.Length * sizeof(float)), vertices, BufferUsageARB.StaticDraw);
        gl.BindBuffer(GLEnum.ArrayBuffer, 0);

        Canvas canvas = new(gl, shderHelper, new Rectangle<int>(0, 0, 200, 200));
    }

    private static void Window_Resize(Vector2D<int> obj)
    {
        width = obj.X;
        height = obj.Y;

        gl.Viewport(0, 0, (uint)obj.X, (uint)obj.Y);

        window.DoRender();
    }

    private static void Window_Render(double obj)
    {
        gl.ClearColor(Color.White);
        gl.Clear(ClearBufferMask.ColorBufferBit);

        gl.EnableVertexAttribArray(positionAttrib);

        gl.BindBuffer(GLEnum.ArrayBuffer, vbo);
        gl.VertexAttribPointer(positionAttrib, 3, GLEnum.Float, false, 0, null);

        gl.UseProgram(shaderProgram.Id);

        Matrix4x4 projection = Matrix4x4.CreateOrthographicOffCenter(0.0f, width, height, 0.0f, -1.0f, 1.0f);

        gl.UniformMatrix4(gl.GetUniformLocation(shaderProgram.Id, "projection"), 1, false, (float*)&projection);

        gl.Uniform4(gl.GetUniformLocation(shaderProgram.Id, "solidColor"), ColorToVector4(Color.MediumSlateBlue));

        gl.DrawArrays(GLEnum.TriangleStrip, 0, 4);

        gl.DisableVertexAttribArray(positionAttrib);

        window.SwapBuffers();
    }

    private static Vector4 ColorToVector4(Color color)
    {
        return new Vector4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);
    }
}