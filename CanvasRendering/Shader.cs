using Silk.NET.OpenGLES;

namespace CanvasRendering;

public class Shader
{
    private readonly GL _gl;

    public uint ShaderId { get; private set; }

    public Shader(GL gl)
    {
        _gl = gl;
    }

    public void LoadShader(GLEnum type, string filePath)
    {
        uint s = _gl.CreateShader(type);
        string vs_source = File.ReadAllText(filePath);
        _gl.ShaderSource(s, vs_source);
        _gl.CompileShader(s);

        string error = _gl.GetShaderInfoLog(s);

        if (!string.IsNullOrEmpty(error))
        {
            Console.WriteLine($"{type}: {error}");
        }
        else
        {
            ShaderId = s;
        }
    }

    public void DeleteShader()
    {
        _gl.DeleteShader(ShaderId);
    }
}
