namespace CanvasRendering.Shaders;

public static class DefaultVertex
{
    public const string Name = @"defaultVertex.vert";

    public const string Source = @"
#version 320 es

layout(location = 0) in vec3 position;
layout(location = 1) in vec2 texCoord;

uniform mat4 orthographic;
uniform mat4 layoutTransform;
uniform mat4 view;
uniform mat4 perspective;

out vec2 fragTexCoord;

void main() {
   gl_Position = perspective * view * layoutTransform * vec4((orthographic * vec4(position, 1.0)).xy, position.z, 1.0);

   fragTexCoord = texCoord;
}";

    public const string PositionAttrib = @"position";

    public const string TexCoordAttrib = @"texCoord";

    public const string OrthographicUniform = @"orthographic";

    public const string LayoutTransformUniform = @"layoutTransform";

    public const string ViewUniform = @"view";

    public const string PerspectiveUniform = @"perspective";
}
