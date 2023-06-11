#version 300 es

precision highp float;

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexCoord;

out vec2 texCoord;

uniform mat4 projection;

void main() {
   gl_Position = projection * vec4(aPosition, 1.0);
   texCoord = aTexCoord;
}