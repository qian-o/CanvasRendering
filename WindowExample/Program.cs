using CanvasRendering;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using Silk.NET.Windowing;

namespace WindowExample;

public class Program
{
    private static readonly List<int> _fpsSample = new();

    private static IWindow window;

    static void Main(string[] args)
    {
        _ = args;

        WindowOptions options = WindowOptions.Default;
        options.Title = "Texture Rendering";
        options.Size = new Vector2D<int>(800, 600);
        options.API = new GraphicsAPI(ContextAPI.OpenGLES, new APIVersion(3, 2));
        options.ShouldSwapAutomatically = true;
        options.Samples = 2;
        options.VSync = true;
        window = Window.Create(options);

        window.Load += () => CanvasDraw.Load(window.CreateOpenGLES(), 800, 600);
        window.Resize += (d) => { CanvasDraw.Resize(d); window.DoRender(); };
        window.Render += CanvasDraw.Render;
        window.Update += Window_Update;

        window.Run();
    }

    private static void Window_Update(double obj)
    {
        if (_fpsSample.Count == 30)
        {
            window.Title = $"Texture Rendering - FPS: {Convert.ToInt32(_fpsSample.Average())}";

            _fpsSample.Clear();
        }

        _fpsSample.Add(Convert.ToInt32(1 / obj));
    }
}
