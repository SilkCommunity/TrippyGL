#version 330 core

in vec3 vPosition;
in vec2 vTexCoords;

out vec2 fTexCoords;

void main() {
    gl_Position = vec4(vPosition, 1.0);
    fTexCoords = vec2(vTexCoords.x, 1.0 - vTexCoords.y);
}