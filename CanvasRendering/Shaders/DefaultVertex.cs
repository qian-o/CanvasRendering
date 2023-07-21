namespace CanvasRendering.Shaders;

public static class DefaultVertex
{
    public const string Name = @"defaultVertex.vert";

    public const string Source = @"
#version 320 es

layout(location = 0) in vec3 position;
layout(location = 1) in vec2 texCoord;

uniform mat4 transform;
uniform mat4 view;
uniform mat4 perspective;

out vec2 fragTexCoord;

void main() {
   
   vec4 transformPosition = transform * vec4(position, 1.0);

   gl_Position = perspective * view * vec4(transformPosition.xy, position.z, 1.0);

   fragTexCoord = texCoord;
}";

    public const string PositionAttrib = @"position";

    public const string TexCoordAttrib = @"texCoord";

    public const string TransformUniform = @"transform";

    public const string ViewUniform = @"view";

    public const string PerspectiveUniform = @"perspective";
}
