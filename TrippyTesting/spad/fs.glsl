#version 400

uniform sampler2D samp;

in vec4 fColor;
in vec2 fTexCoords;

out vec4 FragColor;

void main() {
	FragColor = fColor * texture(samp, fTexCoords);
	//FragColor = clamp(FragColor, 0, 1) * 0.5 + vec4(0.5);
}