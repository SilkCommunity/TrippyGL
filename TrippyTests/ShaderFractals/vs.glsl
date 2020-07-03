#version 330 core

uniform mat4 Projection;
uniform mat4 Transform;

in vec3 vPosition;

out vec2 fCoords;

void main() {
    gl_Position = Projection * vec4(vPosition, 1.0);
    fCoords = (Transform * vec4(vPosition, 1.0)).xy;
}