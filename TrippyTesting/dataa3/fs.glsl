#version 400

uniform sampler2D samp2d;
uniform sampler1D samp1d;

in VertexData {
	vec4 Color;
	vec2 TexCoords;
} input;

out vec4 FragColor;

void main() {
	FragColor = (texture(samp2d, input.TexCoords)) * texture(samp1d, input.TexCoords.x);
}