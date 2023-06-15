using Silk.NET.OpenGLES;

namespace CanvasRendering.Helpers;

public class ShaderHelper
{
    private const string VsExtension = ".vert";
    private const string FsExtension = ".frag";

    private readonly GL _gl;
    private readonly Dictionary<string, Shader> _pairs;

    public ShaderHelper(GL gl)
    {
        _gl = gl;
        _pairs = new Dictionary<string, Shader>();

        foreach (string item in Directory.GetFiles("Shaders"))
        {
            FileInfo fileInfo = new(item);

            if (fileInfo.Extension.ToLower() == VsExtension)
            {
                _pairs.Add(fileInfo.Name, new Shader(_gl, GLEnum.VertexShader, File.ReadAllText(item)));
            }
            else if (fileInfo.Extension.ToLower() == FsExtension)
            {
                _pairs.Add(fileInfo.Name, new Shader(_gl, GLEnum.FragmentShader, File.ReadAllText(item)));
            }
        }
    }

    public Shader GetShader(string fileName)
    {
        return _pairs[fileName];
    }
}
