#version 300 es

precision highp float;

in vec2 fragTexCoord;

uniform vec4 solidColor;

out vec4 fragColor;

void main() {
    fragColor = vec4(1.0f, 0.0f, 0.0f, 1.0f);
}