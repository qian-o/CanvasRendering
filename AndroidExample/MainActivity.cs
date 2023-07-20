using CanvasRendering;
using CanvasRendering.Helpers;
using Silk.NET.Input;
using Silk.NET.OpenGLES;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Sdl.Android;

namespace AndroidExample;

[Activity(Label = "@string/app_name", MainLauncher = true)]
public class MainActivity : SilkActivity
{
    private static IView view;

    protected override void OnRun()
    {
        FileManager.SetLoadFileDelegate((path) =>
        {
            using Stream s = Assets.Open(path);
            using MemoryStream ms = new();
            s.CopyTo(ms);

            return new MemoryStream(ms.ToArray());
        });
        CanvasDraw.FontPath = @"Founder_FW_S.ttf";

        ViewOptions options = ViewOptions.Default;
        options.API = new GraphicsAPI(ContextAPI.OpenGLES, new APIVersion(3, 2));
        view = Silk.NET.Windowing.Window.GetView(options);

        view.Load += View_Load;
        view.Resize += (d) => { CanvasDraw.Resize(d); view.DoRender(); };
        view.Render += CanvasDraw.Render;
        view.Update += CanvasDraw.Update;

        view.Run();
    }

    private void View_Load()
    {
        CanvasDraw.Load(view.CreateOpenGLES(), view.Size.X, view.Size.Y);

        IInputContext inputContext = view.CreateInput();

        inputContext.Mice[0].MouseDown += (_, _) => CanvasDraw.PointerDown();
        inputContext.Mice[0].MouseUp += (_, _) => CanvasDraw.PointerUp();
    }
}