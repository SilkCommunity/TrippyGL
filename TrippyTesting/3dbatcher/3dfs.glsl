#version 400 core

in VertexData {
	vec4 Color;
} vsOutput;

out vec4 FragColor;

void main() {
	FragColor = vsOutput.Color;
}