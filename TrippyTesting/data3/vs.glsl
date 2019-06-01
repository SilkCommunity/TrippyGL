#version 400

uniform mat4 World, View, Projection;

layout (std140) uniform MatrixBlock
{
	mat4 moar;
} pijas;

in vec3 vPosition;
in vec4 vColor;

out vec4 fColor;

void main() {
	gl_Position = Projection * (View * (World * pijas.moar * vec4(vPosition, 1.0)));
	fColor = vColor;
}