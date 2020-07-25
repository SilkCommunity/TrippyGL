#version 330 core

uniform mat4 World;
uniform mat4 View;
uniform mat4 Projection;

in vec3 vPosition;
in vec3 vNormal;

out vec3 fNormal;
out vec3 fPosition;

void main() {
    vec4 worldPos = World * vec4(vPosition, 1.0);
    gl_Position = Projection * View * worldPos;

    fNormal = (World * vec4(vNormal, 0.0)).xyz;
    fPosition = worldPos.xyz;
}