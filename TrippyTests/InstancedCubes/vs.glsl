#version 330 core

uniform mat4 World;
uniform mat4 View;
uniform mat4 Projection;

in vec3 vPosition;
in vec4 vColor;
in vec4 vOffset;

out vec4 fColor;

void main() {
    fColor = vColor;
    
    vec4 worldPosition = World * vec4(vPosition * vOffset.w, 1.0);
    worldPosition.xyz += vOffset.xyz;
    gl_Position = Projection * View * worldPosition;
}