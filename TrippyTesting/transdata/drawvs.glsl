#version 400

in vec3 vPosition;
in vec4 vColor;

out vec4 gColor;

void main() {
	gl_Position = vec4(fract(vPosition), 1.0);
	gColor = vColor;
}