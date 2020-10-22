#version 330 core

uniform mat4 View;
uniform mat4 Projection;

in vec3 vPosition;
in vec3 vNormal;
in vec4 vColor;

out vec3 fPosition;
out vec3 fNormal;
out vec4 fColor;

void main() {
    fPosition = vPosition;
    fNormal = vNormal;
    fColor = vColor;

    gl_Position = Projection * View * vec4(vPosition, 1.0);
}