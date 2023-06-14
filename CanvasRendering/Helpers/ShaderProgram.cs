using Silk.NET.OpenGLES;

namespace CanvasRendering.Helpers;

public class ShaderProgram
{
    private readonly GL _gl;

    public uint Id { get; }

    public Shader Vs { get; private set; }

    public Shader Fs { get; private set; }

    public ShaderProgram(GL gl)
    {
        _gl = gl;

        Id = _gl.CreateProgram();
    }

    public void Attach(Shader vs, Shader fs)
    {
        if (vs != null && Vs != vs)
        {
            if (Vs != null)
            {
                _gl.DetachShader(Id, Vs.Id);
            }

            _gl.AttachShader(Id, vs.Id);

            Vs = vs;
        }

        if (fs != null && Fs != vs)
        {
            if (Fs != null)
            {
                _gl.DetachShader(Id, Fs.Id);
            }

            _gl.AttachShader(Id, fs.Id);

            Fs = fs;
        }

        _gl.LinkProgram(Id);

        string error = _gl.GetProgramInfoLog(Id);

        if (!string.IsNullOrEmpty(error))
        {
            throw new Exception($"Program:{Id}, Error:{error}");
        }
    }
}
