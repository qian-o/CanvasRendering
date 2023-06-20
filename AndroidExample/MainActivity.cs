using CanvasRendering;
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
        ViewOptions options = ViewOptions.Default;
        options.API = new GraphicsAPI(ContextAPI.OpenGLES, new APIVersion(3, 2));
        view = Silk.NET.Windowing.Window.GetView(options);
        
        view.Load += () => CanvasDraw.Load(view.CreateOpenGLES(), view.Size.X, view.Size.Y);
        view.Resize += (d) => { CanvasDraw.Resize(d); view.DoRender(); };
        view.Render += CanvasDraw.Render;

        view.Run();
    }
}