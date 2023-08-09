using Silk.NET.OpenGLES;

namespace CanvasRendering.Helpers;

public class ShaderProgram : IDisposable
{
    private readonly GL _gl;
    private readonly Dictionary<string, int> _attribLocations;
    private readonly Dictionary<string, int> _uniformLocations;

    public uint Id { get; }

    public Shader Vs { get; private set; }

    public Shader Fs { get; private set; }

    public ShaderProgram(GL gl)
    {
        _gl = gl;
        _attribLocations = new Dictionary<string, int>();
        _uniformLocations = new Dictionary<string, int>();

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

    public void Enable()
    {
        _gl.UseProgram(Id);
    }

    public void Disable()
    {
        _gl.UseProgram(0);
    }

    public int GetAttrib(string name)
    {
        if (!_attribLocations.TryGetValue(name, out int value))
        {
            value = _gl.GetAttribLocation(Id, name);

            _attribLocations[name] = value;
        }

        return value;
    }

    public int GetUniform(string name)
    {
        if (!_uniformLocations.TryGetValue(name, out int value))
        {
            value = _gl.GetUniformLocation(Id, name);

            _uniformLocations[name] = value;
        }

        return value;
    }

    public void Dispose()
    {
        _gl.DeleteProgram(Id);
        _attribLocations.Clear();
        _uniformLocations.Clear();

        GC.SuppressFinalize(this);
    }
}
