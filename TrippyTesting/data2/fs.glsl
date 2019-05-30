#version 400

uniform sampler2D samp[5];
uniform float time;

in vec4 fColor;
in vec2 fTexCoords;
in float timeOffset;

out vec4 FragColor;

vec4 yes(){
	float t = fract((timeOffset) / 5.0) * 5.0;
	vec4 haha = vec4(0.0, 0.0, 0.0, 0.0);

	haha += texture(samp[0], fTexCoords) * max(clamp(1.0 - abs(t-0.0), 0.0, 1.0), clamp(1.0 - abs(t-5.0), 0.0, 1.0));
	haha += texture(samp[1], fTexCoords) * clamp(1.0 - abs(t-1.0), 0.0, 1.0);
	haha += texture(samp[2], fTexCoords) * clamp(1.0 - abs(t-2.0), 0.0, 1.0);
	haha += texture(samp[3], fTexCoords) * clamp(1.0 - abs(t-3.0), 0.0, 1.0);
	haha += texture(samp[4], fTexCoords) * clamp(1.0 - abs(t-4.0), 0.0, 1.0);

	return haha;// * 0.001 + vec4(vec3(fract(time+timeOffset*0.17)), 1.0);
}

void main() {
	FragColor = (fColor*0.7+0.3) * yes();
}