#version 330 core

uniform mat4 Projection;
uniform mat4 View;

in vec3 vPosition;

out vec3 fPosition;

void main() {
    fPosition = vPosition;
    gl_Position = Projection * View * vec4(vPosition, 1.0);
}