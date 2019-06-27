#version 400 core

uniform samplerCube samp;
uniform vec3 cameraPos;

in vec3 fPosition;

out vec4 FragColor;

void main() {
	FragColor = texture(samp, fPosition);
}