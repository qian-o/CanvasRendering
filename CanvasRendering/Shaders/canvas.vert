#version 300 es

precision highp float;

layout(location = 0) in vec2 position;
layout(location = 1) in vec2 texCoord;

out vec2 fragTexCoord;

uniform mat4 projection;
uniform vec2 framePosition;

void main() {
   gl_Position = projection * vec4(position + framePosition, 0.0, 1.0);
   fragTexCoord = texCoord;
}