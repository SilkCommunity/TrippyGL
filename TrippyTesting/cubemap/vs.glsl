#version 400 core

layout(std140) uniform MatrixBlock
{
	mat4 World;
	mat4 View;
	mat4 Projection;
};

uniform vec3 cameraPos;

in vec3 vPosition;
in vec4 vColor;

out vec4 fColor;
out vec4 worldPosition;

void main() {
	worldPosition = World * vec4(vPosition + cameraPos, 1.0);
	gl_Position = Projection * View * worldPosition;
	fColor = vColor;
}