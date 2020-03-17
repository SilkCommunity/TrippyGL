#version 400 core

uniform mat4 World;
uniform mat4 View;
uniform mat4 Projection;

in float x;
in float y;
in float z;

in float x2;
in float y2;
in float z2;

in mat4 mat;

in float w;

in float cx;
in float cy;

out vec4 fColor;
out vec2 fTexCoords;

void main() {
	gl_Position = Projection * (View * (World * mat * vec4(vec3(x+x2, y+y2, z+z2), w)));
	fColor = vec4(w);
	fTexCoords = vec2(cx, cy);
}