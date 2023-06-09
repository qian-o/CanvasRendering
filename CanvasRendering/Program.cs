using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using Silk.NET.Windowing;
using System.Drawing;
using System.Numerics;

namespace CanvasRendering;

internal unsafe class Program
{
    private static readonly float[] Vertices = { -0.5f, -0.5f, 0.0f, 0.5f, -0.5f, 0.0f, 0.5f, 0.5f, 0.0f, -0.5f, 0.5f, 0.0f };

    private static IWindow window;
    private static GL gl;
    private static uint vao;
    private static uint shaderProgram;

    static void Main(string[] args)
    {
        WindowOptions options = WindowOptions.Default;
        options.Title = "Texture Rendering";
        options.Size = new Vector2D<int>(800, 600);
        options.API = new GraphicsAPI(ContextAPI.OpenGLES, new APIVersion(3, 2));
        window = Window.Create(options);

        window.Load += Window_Load;
        window.Render += Window_Render;

        window.Run();
    }

    private static void Window_Load()
    {
        gl = GL.GetApi(window);

        gl.GenBuffers(1, out uint vbo);
        gl.BindBuffer(GLEnum.ArrayBuffer, vbo);
        gl.BufferData<float>(GLEnum.ArrayBuffer, (nuint)Vertices.Length * sizeof(float), Vertices, GLEnum.StaticDraw);

        gl.GenVertexArrays(1, out vao);
        gl.BindVertexArray(vao);
        gl.VertexAttribPointer(0, 3, GLEnum.Float, false, 3 * sizeof(float), null);
        gl.EnableVertexAttribArray(0);

        uint vertexShader = gl.CreateShader(GLEnum.VertexShader);
        gl.ShaderSource(vertexShader, "#version 330 core\nlayout (location = 0) in vec3 a_Position;\nuniform mat4 u_ModelViewProjectionMatrix;\nvoid main()\n{\n    gl_Position = u_ModelViewProjectionMatrix * vec4(a_Position, 1.0);\n}\n");
        gl.CompileShader(vertexShader);

        uint fragmentShader = gl.CreateShader(GLEnum.FragmentShader);
        gl.ShaderSource(fragmentShader, "#version 330 core\nout vec4 FragColor;\nvoid main()\n{\n    FragColor = vec4(1.0, 1.0, 1.0, 1.0);\n}\n");
        gl.CompileShader(fragmentShader);

        shaderProgram = gl.CreateProgram();
        gl.AttachShader(shaderProgram, vertexShader);
        gl.AttachShader(shaderProgram, fragmentShader);
        gl.LinkProgram(shaderProgram);

        gl.UseProgram(shaderProgram);

        // 定义变换矩阵并将其传递给顶点着色器
        float tx = 0.5f, ty = 0.5f, tz = 0.0f;
        float angle = 45.0f;
        float scale = 1.0f;

        float[] modelMatrix = new float[] {
                scale * (float)Math.Cos(angle), scale * (float)Math.Sin(angle), 0.0f, 0.0f,
                -scale * (float)Math.Sin(angle), scale * (float)Math.Cos(angle), 0.0f, 0.0f,
                0.0f, 0.0f, scale, 0.0f,
                tx, ty, tz, 1.0f
            };

        int modelMatrixLocation = gl.GetUniformLocation(shaderProgram, "u_ModelViewProjectionMatrix");
        gl.UniformMatrix4(modelMatrixLocation, 1, false, modelMatrix);

        gl.DeleteShader(vertexShader);
        gl.DeleteShader(fragmentShader);
    }

    private static void Window_Render(double obj)
    {
        gl.ClearColor(Color.White);
        gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.StencilBufferBit | ClearBufferMask.DepthBufferBit);

        gl.Viewport(0, 0, (uint)window.Size.X / 2, (uint)window.Size.Y / 2);

        gl.UseProgram(shaderProgram);
        gl.BindVertexArray(vao);
        gl.DrawArrays(GLEnum.TriangleFan, 0, 4);

        gl.Viewport(window.Size.X / 2, 0, (uint)window.Size.X / 2, (uint)window.Size.Y / 2);

        gl.UseProgram(shaderProgram);
        gl.BindVertexArray(vao);
        gl.DrawArrays(GLEnum.TriangleFan, 0, 4);
    }
}
