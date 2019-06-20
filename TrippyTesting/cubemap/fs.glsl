#version 400 core

uniform samplerCube samp;
uniform vec3 cameraPos;

in vec4 fColor;
in vec4 worldPosition;

out vec4 FragColor;

void main() {
	FragColor = clamp(fColor, 1, 1) * texture(samp, worldPosition.xyz - cameraPos);
}