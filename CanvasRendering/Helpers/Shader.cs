using Silk.NET.OpenGLES;

namespace CanvasRendering.Helpers;

public class Shader
{
    private readonly GL _gl;

    public uint Id { get; }

    public Shader(GL gl, GLEnum type, string shaderSource)
    {
        _gl = gl;

        Id = _gl.CreateShader(type);
        _gl.ShaderSource(Id, shaderSource);
        _gl.CompileShader(Id);

        string error = _gl.GetShaderInfoLog(Id);

        if (!string.IsNullOrEmpty(error))
        {
            throw new Exception($"{type}: {error}");
        }
    }
}
