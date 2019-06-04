#version 400

uniform sampler2D samp2d;
uniform sampler1D samp1d;

uniform float time;

in VertexData {
	vec4 Color;
	vec2 TexCoords;
} input;

out vec4 FragColor;

void main() {
	FragColor = (textureLod(samp2d, input.TexCoords, fract(time/7.0)*7.0)) * texture(samp1d, input.TexCoords.x);
}