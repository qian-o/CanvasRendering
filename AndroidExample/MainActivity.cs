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
        string version = GetOpenGLESVersion();

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
        options.Samples = 4;
        view = Silk.NET.Windowing.Window.GetView(options);

        view.Load += View_Load;
        view.FramebufferResize += (d) => { CanvasDraw.FramebufferResize(d); view.DoUpdate(); view.DoRender(); };
        view.Render += CanvasDraw.Render;
        view.Update += CanvasDraw.Update;

        view.Run();
    }

    private void View_Load()
    {
        CanvasDraw.Load(view.CreateOpenGLES(), view.Size.X, view.Size.Y);

        IInputContext inputContext = view.CreateInput();

        CanvasDraw.Mouse = inputContext.Mice[0];
        CanvasDraw.Keyboard = inputContext.Keyboards[0];
    }

    public string GetOpenGLESVersion()
    {
        ActivityManager manager = (ActivityManager)GetSystemService(ActivityService);

        return manager.DeviceConfigurationInfo.GlEsVersion;
    }
}