#version 400

uniform mat4 World, View, Projection;

in vec3 vPosition;
in vec4 vColor;

out vec4 fColor;

void main() {
	gl_Position = Projection * View * World * vec4(vPosition, 1.0);
	fColor = vColor;
}