#version 400 core

uniform mat4 View;
uniform mat4 Projection;

in mat4 World;
in vec3 vPosition;
in vec4 vColor;

out vec4 fColor;

void main() {
	vec4 worldPosition = World * vec4(vPosition, 1.0);
	//worldPosition.x += float(gl_InstanceID) * 20.0;
	gl_Position = Projection * View * worldPosition;
	fColor = vColor;
}