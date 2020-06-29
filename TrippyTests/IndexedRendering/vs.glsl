#version 330 core

uniform mat4 Projection;

in vec2 vPosition;

void main() {
    gl_Position = Projection * vec4(vPosition, 0.0, 1.0);
}