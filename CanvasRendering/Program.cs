using CanvasRendering.Helpers;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using Silk.NET.Windowing;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;

namespace CanvasRendering;

internal unsafe class Program
{
    private static IWindow window;
    private static GL gl;
    private static ShaderHelper shaderHelper;
    private static ShaderProgram shaderProgram;
    private static uint positionAttrib;
    private static uint texCoordAttrib;
    private static int width = 800, height = 600;
    private static Canvas canvas;
    private static Stopwatch stopwatch;

    static void Main(string[] args)
    {
        Console.WriteLine(args);

        WindowOptions options = WindowOptions.Default;
        options.Title = "Texture Rendering";
        options.Size = new Vector2D<int>(width, height);
        options.API = new GraphicsAPI(ContextAPI.OpenGLES, new APIVersion(3, 2));
        options.ShouldSwapAutomatically = true;
        options.Samples = 2;
        options.VSync = true;
        window = Window.Create(options);

        window.Load += Window_Load;
        window.Resize += Window_Resize;
        window.Render += Window_Render;

        window.Run();
    }

    private static void Window_Load()
    {
        gl = window.CreateOpenGLES();

        shaderHelper = new ShaderHelper(gl);

        shaderProgram = new ShaderProgram(gl);
        shaderProgram.Attach(shaderHelper.GetShader("defaultVertex.vert"), shaderHelper.GetShader("texture.frag"));

        positionAttrib = (uint)shaderProgram.GetAttribLocation("position");
        texCoordAttrib = (uint)shaderProgram.GetAttribLocation("texCoord");

        canvas = new(gl, shaderHelper, new Rectangle<int>(0, 0, width, height));
        stopwatch = Stopwatch.StartNew();
    }

    private static void Window_Resize(Vector2D<int> obj)
    {
        width = obj.X;
        height = obj.Y;

        canvas.Resize(new Rectangle<int>(0, 0, width, height));

        window.DoRender();
    }

    private static void Window_Render(double obj)
    {
        gl.ClearColor(Color.White);
        gl.Clear(ClearBufferMask.ColorBufferBit);

        canvas.Clear(Color.White);

        float wSum = (float)width / 30;
        float hSum = (float)height / 30;

        for (int i = 0; i < 30; i++)
        {
            for (int j = 0; j < 30; j++)
            {
                canvas.DrawLine(new PointF(wSum * i, hSum * j), new PointF(wSum * i, hSum * j + hSum), 1, Color.Black);
                canvas.DrawLine(new PointF(wSum * i, hSum * j + hSum), new PointF(wSum * i + wSum, hSum * j + hSum), 1, Color.Black);
                canvas.DrawLine(new PointF(wSum * i + wSum, hSum * j + hSum), new PointF(wSum * i + wSum, hSum * j), 1, Color.Black);
                canvas.DrawLine(new PointF(wSum * i + wSum, hSum * j), new PointF(wSum * i, hSum * j), 1, Color.Black);

                float hue = (float)stopwatch.Elapsed.TotalSeconds * 0.15f % 1;

                canvas.DrawRectangle(new RectangleF(wSum * i + wSum / 4, hSum * j + hSum / 4, wSum / 2, hSum / 2), new Vector4(1.0f * hue, 1.0f * 0.75f, 1.0f * 0.75f, 1.0f).ToColor());

                hue = (float)stopwatch.Elapsed.TotalSeconds * 0.30f % 1;

                canvas.DrawCircle(new PointF(wSum * i + wSum / 2, hSum * j + hSum / 2), 10, new Vector4(1.0f * hue, 1.0f * 0.75f, 1.0f * 0.75f, 1.0f).ToColor());
            }
        }

        canvas.Flush();

        DrawCanvas(canvas);
    }

    private static void DrawCanvas(Canvas canvas)
    {
        gl.Viewport(0, 0, (uint)width, (uint)height);

        gl.EnableVertexAttribArray(positionAttrib);
        gl.EnableVertexAttribArray(texCoordAttrib);

        shaderProgram.Enable();

        Matrix4x4 projection = Matrix4x4.CreateOrthographicOffCenter(0.0f, width, height, 0.0f, -1.0f, 1.0f);

        gl.UniformMatrix4(shaderProgram.GetUniformLocation("projection"), 1, false, (float*)&projection);

        gl.BindBuffer(GLEnum.ArrayBuffer, canvas.VertexBuffer);
        gl.VertexAttribPointer(positionAttrib, 3, GLEnum.Float, false, 0, null);

        gl.BindBuffer(GLEnum.ArrayBuffer, canvas.TexCoordBuffer);
        gl.VertexAttribPointer(texCoordAttrib, 2, GLEnum.Float, false, 0, null);

        gl.ActiveTexture(GLEnum.Texture0);
        gl.BindTexture(GLEnum.Texture2D, canvas.Framebuffer.DrawTexture);
        gl.Uniform1(shaderProgram.GetUniformLocation("tex"), 0);

        gl.DrawArrays(GLEnum.TriangleStrip, 0, 4);

        gl.BindTexture(GLEnum.Texture2D, 0);

        shaderProgram.Disable();

        gl.DisableVertexAttribArray(positionAttrib);
        gl.DisableVertexAttribArray(texCoordAttrib);
    }
}