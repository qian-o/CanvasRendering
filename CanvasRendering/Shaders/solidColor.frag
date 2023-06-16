#version 320 es

precision highp float;

uniform vec4 solidColor;

out vec4 fragColor;

void main() {
    fragColor = solidColor;
}