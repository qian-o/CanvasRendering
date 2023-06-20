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
        Dictionary<string, Shader> pairs = new()
        {
            { "defaultVertex.vert", new Shader(_gl, GLEnum.VertexShader, @"
#version 320 es

layout(location = 0) in vec3 position;
layout(location = 1) in vec2 texCoord;

uniform mat4 projection;

out vec2 fragTexCoord;

void main() {
   gl_Position = projection * vec4(position, 1.0);

   fragTexCoord = texCoord;
}") },
            { "solidColor.frag", new Shader(_gl, GLEnum.FragmentShader, @"
#version 320 es

precision highp float;

uniform vec4 solidColor;

out vec4 fragColor;

void main() {
    fragColor = solidColor;
}") },
            { "texture.frag", new Shader(_gl, GLEnum.FragmentShader, @"
#version 320 es

precision highp float;

in vec2 fragTexCoord;

uniform sampler2D tex;

out vec4 fragColor;

void main() {
    fragColor = texture(tex, fragTexCoord);
}") }
        };

        return pairs;
    }
}
