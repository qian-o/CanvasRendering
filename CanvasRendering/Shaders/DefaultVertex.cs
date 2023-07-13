namespace CanvasRendering.Shaders;

public static class DefaultVertex
{
    public const string Name = @"defaultVertex.vert";

    public const string Source = @"
#version 320 es

layout(location = 0) in vec3 position;
layout(location = 1) in vec2 texCoord;

uniform mat4 orthographic;
uniform mat4 renderTransform;
uniform mat4 view;
uniform mat4 perspective;
uniform mat4 layoutTransform;

out vec2 fragTexCoord;

void main() {
   gl_Position = layoutTransform * perspective * view * renderTransform * vec4((orthographic * vec4(position, 1.0)).xy, position.z, 1.0);

   fragTexCoord = texCoord;
}";

    public const string PositionAttrib = @"position";

    public const string TexCoordAttrib = @"texCoord";

    public const string OrthographicUniform = @"orthographic";

    public const string RenderTransformUniform = @"renderTransform";

    public const string ViewUniform = @"view";

    public const string PerspectiveUniform = @"perspective";

    public const string LayoutTransformUniform = @"layoutTransform";
}
