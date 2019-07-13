#version 400 core

uniform mat4 mat;

in vec3 vPosition;
in vec4 vColor;

out vec4 fColor;

void main() {
	gl_Position = mat * vec4(vPosition, 1.0);
	fColor = vColor;
}