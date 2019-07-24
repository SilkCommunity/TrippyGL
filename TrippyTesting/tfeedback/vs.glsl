#version 400 core

in vec3 vPosition;
in vec3 vNormal;

out vec4 fColor;

out vec3 tPosition;
out vec3 tNormal;

void main() {
	gl_Position = vec4(vPosition, 1.0);
	fColor = vec4(vNormal, 1.0);
	tPosition = fract((vPosition+vec3(0.01, 0.01, 0.0))*0.5+0.5)*2.0-1.0;
	tNormal = fract(vNormal + 0.01);
}