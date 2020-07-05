#version 330 core

uniform mat4 Projection;
uniform mat3x2 Transform;

in vec3 vPosition;

out vec2 fCoords;

void main() {
    gl_Position = Projection * vec4(vPosition, 1.0);
    fCoords = (Transform * vec3(vPosition.xy, 1.0)).xy;
}