namespace CanvasRendering.Shaders;

public static class SolidColorFragment
{
    public const string Name = @"solidColor.frag";

    public const string Source = @"
#version 320 es

precision highp float;

uniform vec4 solidColor;

out vec4 fragColor;

void main() {
    fragColor = solidColor;
}";

    public const string SolidColorUniform = @"solidColor";
}
