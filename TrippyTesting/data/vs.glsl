#version 410

uniform mat4 World;
uniform mat4 View;
uniform vec4 haha[5];
uniform mat4 Projection;


in vec3 vPosition;
in vec4 vColor;
in mat3 vMat;
in dvec4 vDou;
in vec2 vTexCoords;
in ivec4 vInts;

out vec4 fColor;
out vec2 fTexCoords;

void main() {
	gl_Position = Projection * (View * (World * vec4(vMat * vPosition, 1.0)));
	fColor = vColor + vec4(haha[0].x, haha[1].y, haha[2].z, haha[3].w);
	fTexCoords = vTexCoords + vec2(float(vDou.x+vDou.y), float(vDou.z+vDou.w)) + 0.05 * vec2(float(vInts.x+vInts.y), float(vInts.z+vInts.w));
}