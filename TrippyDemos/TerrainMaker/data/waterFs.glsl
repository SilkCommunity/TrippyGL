#version 330 core

in float waterDepth;

out vec4 FragColor;

void main() {
	FragColor = vec4(vec3(0.4, 0.4, 1.0) * max(1 - waterDepth*0.07, 0), 0.5);
}