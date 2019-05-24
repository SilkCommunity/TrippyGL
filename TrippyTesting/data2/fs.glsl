#version 400

uniform sampler2D samp[5];
uniform float time;

in vec4 fColor;
in vec2 fTexCoords;

out vec4 FragColor;

vec4 yes(){
	float t = fract(time / 5.0) * 5.0;
	vec4 haha = vec4(0.0, 0.0, 0.0, 0.0);

	for(int i=0; i<samp.length(); i++) {
		haha += texture2D(samp[i], fTexCoords) * clamp(1.0 - abs(t-float(i)), 0.0, 1.0);
	}
	return haha;
}

void main() {
	FragColor = fColor * yes();
}