using CanvasRendering;
using CanvasRendering.Helpers;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using Silk.NET.Windowing;
using Window = Silk.NET.Windowing.Window;

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
        options.Samples = 16;
        options.VSync = true;
        window = Window.Create(options);

        window.Load += Window_Load;
        window.Resize += (d) => { CanvasDraw.Resize(d); window.DoRender(); };
        window.Render += CanvasDraw.Render;
        window.Update += CanvasDraw.Update;

        window.Run();
    }

    private static void Window_Load()
    {
        CanvasDraw.Load(window.CreateOpenGLES(), 800, 600);

        IInputContext inputContext = window.CreateInput();

        inputContext.Mice[0].MouseDown += (_, _) => CanvasDraw.PointerDown();
        inputContext.Mice[0].MouseUp += (_, _) => CanvasDraw.PointerUp();
    }
}
