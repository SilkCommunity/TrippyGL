#version 400 core

uniform mat4 World;
uniform mat4 View;
uniform mat4 Projection;
uniform float time;

in vec3 vPosition;
in vec4 vColor;

out vec4 fColor;
out vec4 fClipSpace;
out vec2 distortMapCoords;
out vec3 pos;

void main() {
	vec4 worldPosition = World * vec4(vPosition, 1.0);
	vec4 projectionPosition = Projection * View * worldPosition;
	gl_Position = projectionPosition;
	distortMapCoords = worldPosition.xz + fract(time * 0.1);
	pos = worldPosition.xyz;
	fClipSpace = projectionPosition;
	fColor = vColor;
}