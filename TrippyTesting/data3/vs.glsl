#version 400

layout(std140) uniform MatrixBlock
{
	mat4 World;
	mat4 View;
	mat4 Projection;
};

uniform float time, amp;

in vec3 vPosition;
in vec4 vColor;

out vec4 fColor;

void main() {
	vec3 offset = vec3(
		sin(time + vPosition.y - vPosition.x),
		cos(vPosition.z - time - vPosition.y),
		sin(time + vPosition.x + vPosition.z)
	);
	gl_Position = Projection * (View * (World * vec4(vPosition + offset * amp, 1.0)));
	fColor = vColor;
}