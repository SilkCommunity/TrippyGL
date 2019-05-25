#version 400

layout (triangles) in;
layout (triangle_strip, max_vertices = 36) out;
layout (invocations = 12) in;

uniform float time;
uniform mat4 World;
uniform mat4 View;
uniform mat4 Projection;

in vec4 gColor[];
in vec2 gTexCoords[];

out vec4 fColor;
out vec2 fTexCoords;
out float timeOffset;

void main() {
	for(int i=0; i<6; i++) {
		float jejee = float(gl_InvocationID) * 0.2;
		vec4 offset = vec4(cos(time + jejee), sin(time + jejee), -0.1, 0) * (0.1 * float(i));
		timeOffset = float(i);
		
		gl_Position = gl_in[0].gl_Position + offset;
		fColor = gColor[0];
		fTexCoords = gTexCoords[0];
		EmitVertex();
		
		gl_Position = gl_in[1].gl_Position + offset;
		fColor = gColor[1];
		fTexCoords = gTexCoords[1];
		EmitVertex();
		
		gl_Position = gl_in[2].gl_Position + offset;
		fColor = gColor[2];
		fTexCoords = gTexCoords[2];
		EmitVertex();
			
		gl_Position = Projection * (View * (World * vec4(0.5, 0.5, 0.0, 1.0))) + offset;
		fColor = vec4(1f);
		fTexCoords = vec2(1, 0);
		EmitVertex();
	
		EndPrimitive();
	}
}