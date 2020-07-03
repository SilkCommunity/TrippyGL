#version 330 core
const int loops = 300;

uniform vec2 c;

in vec2 fCoords;

out vec4 FragColor;

vec2 CMult(vec2 a, vec2 b) {
    return vec2(a.x*b.x - a.y*b.y, a.x*b.y + a.y*b.x);
}

vec3 Jul(vec2 v) {
    for (int i=0; i<loops; i++) {
        v = CMult(v, v) + CMult(vec2(0.1, -0.5), v) + c;
        if (v.x > 2.0 || v.y > 2.0)
            return vec3(float(i)/100.0, float(i)/150.0, float(i)/50.0);
    }

    return vec3(0, 0, 0);
}

void main() {
    FragColor = vec4(Jul(fCoords), 1.0);
}