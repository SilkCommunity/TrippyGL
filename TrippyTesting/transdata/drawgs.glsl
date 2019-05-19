#version 400

layout (points) in;
layout (triangle_strip, max_vertices = 3) out;

uniform mat4 World;
uniform mat4 View;
uniform mat4 Projection;

in vec4 gColor[];

out vec4 fColor;

vec4 transform(vec4 pos) {
	return Projection * (View * (World * pos));
}

void main() {
	gl_Position = transform(gl_in[0].gl_Position + 0.5 * vec4(0.0, -0.04, 0.0, 0.0));
	fColor = gColor[0];
	EmitVertex();
	
	gl_Position = transform(gl_in[0].gl_Position + 0.5 * vec4(-0.03, 0.04, 0.0, 0.0));
	fColor = gColor[0];
	EmitVertex();

	gl_Position = transform(gl_in[0].gl_Position + 0.5 * vec4(0.03, 0.04, 0.0, 0.0));
	fColor = gColor[0];
	EmitVertex();

	EndPrimitive();
}