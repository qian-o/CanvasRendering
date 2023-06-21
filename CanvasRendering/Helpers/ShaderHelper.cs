using CanvasRendering.Shaders;
using Silk.NET.OpenGLES;

namespace CanvasRendering.Helpers;

public class ShaderHelper
{
    private readonly GL _gl;
    private readonly Dictionary<string, Shader> _pairs;

    public ShaderHelper(GL gl)
    {
        _gl = gl;
        _pairs = LoadShader();
    }

    public Shader GetShader(string fileName)
    {
        return _pairs[fileName];
    }

    private Dictionary<string, Shader> LoadShader()
    {
        Dictionary<string, Shader> pairs = new();

        // VertexShader
        {
            pairs.Add(DefaultVertex.Name, new Shader(_gl, GLEnum.VertexShader, DefaultVertex.Source));
        }

        // FragmentShader
        {
            pairs.Add(SolidColorFragment.Name, new Shader(_gl, GLEnum.FragmentShader, SolidColorFragment.Source));
            pairs.Add(TextureFragment.Name, new Shader(_gl, GLEnum.FragmentShader, TextureFragment.Source));
        }

        return pairs;
    }
}
