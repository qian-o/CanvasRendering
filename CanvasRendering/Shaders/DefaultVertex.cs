namespace CanvasRendering.Shaders;

public static class DefaultVertex
{
    public const string Name = @"defaultVertex.vert";

    public const string Source = @"
#version 320 es

layout(location = 0) in vec3 position;
layout(location = 1) in vec2 texCoord;

uniform mat4 orthographic;
uniform mat4 begin;
uniform mat4 transform;
uniform mat4 view;
uniform mat4 perspective;
uniform mat4 end;

out vec2 fragTexCoord;

void main() {

   vec4 position1 = transform * vec4(position, 1.0);

   // Obtaining spatial coordinates using an orthogonal matrix.
   vec4 actualPosition = orthographic * position1;
   actualPosition.z = position1.z;

   // First, use the begin matrix for initial processing.
   actualPosition = begin * actualPosition;

   // Using transformation, camera, and perspective matrix.
   actualPosition = perspective * view * actualPosition;

   // Finally, use the end matrix for final processing.
   gl_Position = end * actualPosition;

   fragTexCoord = texCoord;
}";

    public const string PositionAttrib = @"position";

    public const string TexCoordAttrib = @"texCoord";

    public const string OrthographicUniform = @"orthographic";

    public const string BeginUniform = @"begin";

    public const string TransformUniform = @"transform";

    public const string ViewUniform = @"view";

    public const string PerspectiveUniform = @"perspective";

    public const string EndUniform = @"end";
}
