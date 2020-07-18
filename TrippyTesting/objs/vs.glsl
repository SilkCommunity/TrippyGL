#version 330 core

layout(std140) uniform MatrixBlock {
	mat4 World;
	mat4 View;
	mat4 Projection;
};

in vec3 vPosition;
in vec3 vNormal;
in vec2 vTexCoords;

out vec3 fNormal;
out vec2 fTexCoords;

void main() {
	gl_Position = Projection * View * World * vec4(vPosition, 1.0);
	fNormal = (World * vec4(vNormal, 0.0)).xyz;
	fTexCoords = vTexCoords;
}