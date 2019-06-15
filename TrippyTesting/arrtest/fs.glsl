#version 400 core

uniform sampler2DArray samp;
uniform float time;

in VertexData {
	vec4 Color;
	vec2 TexCoords;
} vsOutput;

out vec4 FragColor;

void main() {
	FragColor = /*vsOutput.Color * */texture(samp, vec3(vsOutput.TexCoords, fract((time * vsOutput.TexCoords.x)/4.0)*4.0-0.5));
	//FragColor.xy += vsOutput.TexCoords;
}