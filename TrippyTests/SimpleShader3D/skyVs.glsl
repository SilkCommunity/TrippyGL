#version 330 core

uniform mat4 View;
uniform mat4 Projection;

in vec3 vPosition;

out vec3 fPosition;

void main() {
    gl_Position = Projection * View * vec4(vPosition, 1.0);
    fPosition = vPosition;
}