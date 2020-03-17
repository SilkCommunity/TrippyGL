#version 400 core

in vec2 coord;

out vec4 FragColor;

//The top levels of each layer
const float WATER_TOP = 0.45;
const float SAND_TOP = 0.5;
const float GREEN_TOP = 0.65;
const float STONE_TOP = 0.75;
const float MOUNTAIN_TOP = 1.0;
const float LAVA_MIN = 0.77;

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

vec3 terr(vec2 c) {
  vec2 fbmval = fbm2d(c);
  float moistval = fbmval.y;
  float heightval = fbmval.x;

  /*if(heightval < WATER_TOP) {
    //Make water
    float w = heightval / WATER_TOP;
    return vec3(0.14,0.4, 1.0) * vec3(pow(w*0.5*2.0, 1.2));
  }*/

  if (heightval < SAND_TOP) {
    //Make sand
    float s = (heightval - WATER_TOP) / (SAND_TOP - WATER_TOP);
    float m = floor((moistval*1.8)*4.0)/4.0;
    return mix(vec3(0.7, 0.6, 0.4), vec3(0.94, 0.87, 0.56), m) * vec3(s*0.2+0.8);
  }

  if (heightval < GREEN_TOP) {
    float g = (heightval - SAND_TOP) / (GREEN_TOP - SAND_TOP);
    float m = floor((moistval*1.8)*4.0)/4.0;
    return mix(vec3(0.1, 0.7, 0.0), vec3(0.05, 0.4, 0.0), m);
    return vec3(0.0, 1.0, 0.2);
  }

  if (heightval > STONE_TOP && heightval < MOUNTAIN_TOP) {
    float s = (heightval - STONE_TOP) / (MOUNTAIN_TOP - STONE_TOP);

    if (heightval > LAVA_MIN && abs(moistval-0.13) < 0.25) {
      //LAVA
      return vec3(1.0, 0.1, 0.2) * clamp((pow(s+0.79, 6.0)+0.2), 0.1, 1.3);
    }

    if(abs(moistval-0.72) < 0.25) {
      //SNOW
      return vec3(0.84, 0.9, 1.0) * (s+0.95);
    }
  }

  if (heightval < MOUNTAIN_TOP) { //STONE_TOP) {
    float s = (heightval - GREEN_TOP) / (STONE_TOP - GREEN_TOP);
    return vec3(0.5, 0.55, 0.52) * (s*0.35+0.8);
  }

  return vec3(1,0,1);
}

void main() {
	FragColor = vec4(terr(coord), 1.0);
}