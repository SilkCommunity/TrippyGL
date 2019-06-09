#version 400

uniform sampler2D samp;

in vec4 fColor;
in vec2 fTexCoords;

out vec4 FragColor;

void main() {
	FragColor = fColor * texture(samp, fTexCoords);
}