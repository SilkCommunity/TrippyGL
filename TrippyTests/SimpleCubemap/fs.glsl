#version 330 core

uniform samplerCube cubemap;

in vec3 fPosition;

out vec4 FragColor;

void main() {
    FragColor = texture(cubemap, fPosition);
}