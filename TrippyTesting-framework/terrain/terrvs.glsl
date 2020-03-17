#version 400 core

uniform mat4 World;
uniform mat4 View;
uniform mat4 Projection;

uniform float clipOffset;
uniform float clipMultiplier;

in vec3 vPosition;

out vec2 coord;

vec2 rand2d(vec2 c) {
  return fract(sin(vec2(
    dot(c, vec2(52.9258, 76.3911)),
    dot(c, vec2(66.7943, 33.1674))
    )) * vec2(49164.7641, 69761.6413));
}

vec2 noise2d(vec2 c) {
  vec2 fc = fract(c);
  vec2 ic = floor(c);
  vec2 bl = rand2d(ic);
  vec2 br = rand2d(ic + vec2(1.0, 0.0));
  vec2 tl = rand2d(ic + vec2(0.0, 1.0));
  vec2 tr = rand2d(ic + vec2(1.0, 1.0));
  return mix(mix(bl, br, fc.x), mix(tl, tr, fc.x), fc.y);
}

vec2 fbm2d(vec2 c) {
  float amp = 0.5;
  float freq = 1.0;
  vec2 v = vec2(0.0);
  for(int i=0; i<8; i++) {
    v += noise2d(c * freq) * amp;
    amp *= 0.5;
    freq *= 2.0;
  }
  return v;
}

float transheight(float h) {
	return pow(h, 1.75) * 2.5;
}

void main() {
	vec4 worldPosition = World * vec4(vPosition, 1.0);

	coord = worldPosition.xz;
	worldPosition.y += transheight(fbm2d(worldPosition.xz).x);

	gl_ClipDistance[0] = (worldPosition.y - clipOffset) * clipMultiplier;
	gl_Position = Projection * View * worldPosition;
}