#version 330 core

uniform mat4 View;
uniform mat4 Projection;

in vec3 vPosition;

out float waterDepth;

void main() {
    waterDepth = -vPosition.y;
	gl_Position = Projection * View * vec4(vPosition.x, 0.0, vPosition.z, 1.0);
}