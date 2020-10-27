#version 330 core

uniform vec3 cameraPos;
uniform float nearFog;
uniform float farFog;

in vec3 fPosition;
in vec3 fNormal;
in vec4 fColor;

out vec4 FragColor;

void main() {
    FragColor = fColor;
    FragColor.a = 1.0 - max((distance(cameraPos, fPosition) - nearFog) / (farFog - nearFog), 0.0);
    //FragColor = vec4(vec3(fPosition.y / 6.0), 1.0);
}