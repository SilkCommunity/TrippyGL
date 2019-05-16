#version 400

uniform mat4 World;
uniform mat4 View;
uniform mat4 Projection;

in vec3 vPosition;
in vec4 vColor;
in vec2 vTexCoords;

out vec4 fColor;
out vec2 fTexCoords;

void main() {
    gl_Position = (Projection * (View * (World * vec4(vPosition, 1.0))));
	fColor = vColor;
	fTexCoords = vTexCoords;
}