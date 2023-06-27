using CanvasRendering;
using CanvasRendering.Helpers;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using Silk.NET.Windowing;

namespace WindowExample;

public class Program
{
    private static IWindow window;

    static void Main(string[] args)
    {
        _ = args;

        FileManager.SetLoadFileDelegate(File.OpenRead);
        CanvasDraw.FontPath = @"Resources/Founder_FW_S.ttf";

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

        window.Run();
    }
}
