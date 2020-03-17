#version 400 core

uniform mat4 World;
uniform mat4 View;
uniform mat4 Projection;

in vec3 vPosition;
in vec4 vColor;

out VertexData {
	vec4 Color;
} vsOutput;

void main () {
	gl_Position = Projection * View * World * vec4(vPosition, 1.0);
	vsOutput.Color = vColor;
}