#version 400

uniform mat4 World, View, Projection;

in vec3 vPosition;
in vec4 vColor;
in vec2 vTexCoords;

out VertexData {
	vec4 Color;
	vec2 TexCoords;
} vsOutput;

void main() {
	gl_Position = Projection * View * World * vec4(vPosition, 1.0);
	vsOutput.Color = vColor;
	vsOutput.TexCoords = vTexCoords;
}