#version 400 core

uniform mat4 View;
uniform mat4 Projection;

uniform vec3 cameraPos;

in vec3 vPosition;

out vec3 fPosition;

void main() {
	fPosition = vPosition;
	gl_Position = Projection * View * vec4(vPosition + cameraPos, 1.0);
}