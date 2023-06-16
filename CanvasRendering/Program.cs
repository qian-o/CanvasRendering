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
    private static ShaderHelper shaderHelper;
    private static ShaderProgram shaderProgram;
    private static uint positionAttrib;
    private static uint texCoordAttrib;
    private static int width = 800, height = 600;

    static void Main(string[] args)
    {
        Console.WriteLine(args);

        WindowOptions options = WindowOptions.Default;
        options.ShouldSwapAutomatically = true;
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

        shaderHelper = new ShaderHelper(gl);

        shaderProgram = new ShaderProgram(gl);
        shaderProgram.Attach(shaderHelper.GetShader("defaultVertex.vert"), shaderHelper.GetShader("texture.frag"));

        positionAttrib = (uint)gl.GetAttribLocation(shaderProgram.Id, "position");
        texCoordAttrib = (uint)gl.GetAttribLocation(shaderProgram.Id, "texCoord");
    }

    private static void Window_Resize(Vector2D<int> obj)
    {
        width = obj.X;
        height = obj.Y;

        window.DoRender();
    }

    private static void Window_Render(double obj)
    {
        gl.ClearColor(Color.White);
        gl.Clear(ClearBufferMask.ColorBufferBit);

        Canvas canvas = new(gl, shaderHelper, new Rectangle<int>(0, 0, width, height));
        canvas.Clear(Color.White);

        float wSum = (float)width / 10;
        float hSum = (float)height / 10;

        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                canvas.DrawRectangle(new RectangleF(wSum * i + wSum / 4, hSum * j + hSum / 4, wSum / 2, hSum / 2), Color.Red);
            }
        }

        canvas.Flush();

        DrawCanvas(canvas);

        canvas.Dispose();
    }

    private static void DrawCanvas(Canvas canvas)
    {
        gl.Viewport(0, 0, (uint)width, (uint)height);

        gl.EnableVertexAttribArray(positionAttrib);
        gl.EnableVertexAttribArray(texCoordAttrib);

        gl.UseProgram(shaderProgram.Id);

        Matrix4x4 projection = Matrix4x4.CreateOrthographicOffCenter(0.0f, width, height, 0.0f, -1.0f, 1.0f);

        gl.UniformMatrix4(gl.GetUniformLocation(shaderProgram.Id, "projection"), 1, false, (float*)&projection);

        gl.BindBuffer(GLEnum.ArrayBuffer, canvas.VertexBuffer);
        gl.VertexAttribPointer(positionAttrib, 3, GLEnum.Float, false, 0, null);

        gl.BindBuffer(GLEnum.ArrayBuffer, canvas.TexCoordBuffer);
        gl.VertexAttribPointer(texCoordAttrib, 2, GLEnum.Float, false, 0, null);

        gl.ActiveTexture(GLEnum.Texture0);
        gl.BindTexture(GLEnum.Texture2D, canvas.Framebuffer.Texture);
        gl.Uniform1(gl.GetUniformLocation(shaderProgram.Id, "tex"), 0);

        gl.DrawArrays(GLEnum.TriangleStrip, 0, 4);

        gl.BindTexture(GLEnum.Texture2D, 0);

        gl.DisableVertexAttribArray(positionAttrib);
        gl.DisableVertexAttribArray(texCoordAttrib);
    }
}