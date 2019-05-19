#version 400

uniform float time;
uniform float deltaTime;

in vec3 vPosition;
in vec4 vColor;

out vec3 outPosition;
out vec4 outColor;

void main() {
	outPosition = vPosition + vec3(0.2, 0.2, 0.0) * deltaTime;
	//outPosition.xy += vec2(cos(time*20.0), sin(time*20.0)) * 0.006;
	outColor = vColor;
}