#version 400

uniform sampler2D samp2d;
uniform sampler1D samp1d;

uniform float time;

in VertexData {
	vec4 Color;
	vec2 TexCoords;
} vsOutput;

out vec4 FragColor;

void main() {
	FragColor = (vsOutput.Color*0.7+0.3) * (textureLod(samp2d, vsOutput.TexCoords, fract(time/7.0)*7.0)) * texture(samp1d, vsOutput.TexCoords.x + time);
	//FragColor = clamp(FragColor, 0, 0) + texture(samp1d, vsOutput.TexCoords.x);
}