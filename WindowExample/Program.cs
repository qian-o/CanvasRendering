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
        options.Samples = 8;
        window = Window.Create(options);

        window.Load += Window_Load;
        window.FramebufferResize += (d) => { CanvasDraw.FramebufferResize(d); window.DoUpdate(); window.DoRender(); };
        window.Render += CanvasDraw.Render;
        window.Update += CanvasDraw.Update;

        window.Run();
    }

    private static void Window_Load()
    {
        CanvasDraw.Load(window.CreateOpenGLES(), 800, 600);

        IInputContext inputContext = window.CreateInput();

        CanvasDraw.Mouse = inputContext.Mice[0];
        CanvasDraw.Keyboard = inputContext.Keyboards[0];
    }
}
