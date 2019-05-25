#version 400

uniform mat4 World;
uniform mat4 View;
uniform mat4 Projection;

in vec3 vPosition;
in vec3 vColor;
in vec2 vTexCoords;

out vec4 gColor;
out vec2 gTexCoords;

void main () {
	gl_Position = Projection * (View * (World * vec4(vPosition, 1.0)));
	gColor = vec4(vColor, 1.0);
	gTexCoords = vTexCoords;
}