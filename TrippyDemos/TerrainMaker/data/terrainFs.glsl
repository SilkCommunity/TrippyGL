#version 330 core

in vec3 fPosition;
in vec3 fNormal;
in vec4 fColor;

out vec4 FragColor;

void main() {
    FragColor = fColor;
    //FragColor = vec4(vec3(fPosition.y/6.0), 1.0);
}