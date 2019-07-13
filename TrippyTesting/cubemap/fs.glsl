#version 400 core

uniform samplerCube samp;
uniform vec3 cameraPos;
uniform float time;

in vec3 fPosition;

out vec4 FragColor;


mat4 rotationX( in float angle ) {
	return mat4(	1.0,		0,			0,			0,
			 		0, 	cos(angle),	-sin(angle),		0,
					0, 	sin(angle),	 cos(angle),		0,
					0, 			0,			  0, 		1);
}

mat4 rotationY( in float angle ) {
	return mat4(	cos(angle),		0,		sin(angle),	0,
			 				0,		1.0,			 0,	0,
					-sin(angle),	0,		cos(angle),	0,
							0, 		0,				0,	1);
}

mat4 rotationZ( in float angle ) {
	return mat4(	cos(angle),		-sin(angle),	0,	0,
			 		sin(angle),		cos(angle),		0,	0,
							0,				0,		1,	0,
							0,				0,		0,	1);
}

float wave(float amp, float timemul, float offset) {
	return (sin(time*timemul+offset)*0.5+0.5)*amp;
}

void main() {
	const float amp = 0.05;
	const float jej = 16.0;
	vec3 dir = (rotationX(wave(amp, 3.14, fPosition.x*jej)) * rotationY(wave(amp, 3.14, fPosition.y*jej)) * rotationZ(wave(amp, 3.14, fPosition.z*jej)) * vec4(fPosition, 1.0)).xyz + vec3(time * 0.0001, 0.0, 0.0);
	FragColor = texture(samp, dir);
}
