using Silk.NET.OpenGLES;

namespace CanvasRendering;

public class ShaderProgram
{
    private readonly GL _gl;

    private Shader last_vs, last_fs;

    public uint ProgramId { get; private set; }

    public event Action UpdateAttach;

    public ShaderProgram(GL gl)
    {
        _gl = gl;
    }

    public void AttachShader(Shader vs, Shader fs)
    {
        if (ProgramId == 0)
        {
            ProgramId = _gl.CreateProgram();
        }

        if (vs != null)
        {
            if (last_vs != null)
            {
                _gl.DetachShader(ProgramId, last_vs.ShaderId);
            }

            _gl.AttachShader(ProgramId, vs.ShaderId);

            last_vs = vs;
        }

        if (fs != null)
        {
            if (last_fs != null)
            {
                _gl.DetachShader(ProgramId, last_fs.ShaderId);
            }

            _gl.AttachShader(ProgramId, fs.ShaderId);

            last_fs = fs;
        }

        _gl.LinkProgram(ProgramId);

        string error = _gl.GetProgramInfoLog(ProgramId);

        if (!string.IsNullOrEmpty(error))
        {
            Console.WriteLine($"Program:{ProgramId}, Error:{error}");
        }

        UpdateAttach?.Invoke();
    }

    public void Use()
    {
        _gl.UseProgram(ProgramId);
    }

    public int GetAttribLocation(string name)
    {
        return _gl.GetAttribLocation(ProgramId, name);
    }

    public int GetUniformLocation(string name)
    {
        return _gl.GetUniformLocation(ProgramId, name);
    }
}
