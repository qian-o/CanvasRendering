namespace CanvasRendering.Shaders;

public static class TextureFragment
{
    public const string Name = @"texture.frag";

    public const string Source = @"
#version 320 es

precision highp float;

in vec2 fragTexCoord;

uniform sampler2D tex;

out vec4 fragColor;

void main() {
    fragColor = texture(tex, fragTexCoord);
}";

    public const string TexUniform = @"tex";
}
