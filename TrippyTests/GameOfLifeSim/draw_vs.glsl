#version 330 core

uniform mat3x2 Transform;
uniform mat4 Projection;

in vec3 vPosition;
in vec2 vTexCoords;

out vec2 fTexCoords;

void main() {
    vec2 pp = (Transform * vec3(vPosition.xy, 1.0)).xy;
    gl_Position = Projection * vec4(pp, 0.0, 1.0);
    fTexCoords = vTexCoords;
}