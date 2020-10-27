#version 330 core

uniform mat4 View;
uniform mat4 Projection;

uniform vec3 cameraPos;

in vec3 vPosition;

out vec3 fPosition;
out vec4 clipSpace;
out vec2 distortCoords;
out vec3 toCameraVector;
out float waterDepth;
out float aboveWater;

void main() {
	fPosition = vPosition;
	distortCoords = (vPosition.xz * 0.5) * 0.04;
	clipSpace = Projection * View * vec4(vPosition.x, 0.0, vPosition.z, 1.0);
	toCameraVector = cameraPos - vPosition;
    waterDepth = -vPosition.y;
	aboveWater = sign(cameraPos.y);
	gl_Position = clipSpace;
}