#version 330 core

uniform float time;
uniform vec3 sunDirection;
uniform vec3 sunColor;

in vec3 fPosition;

out vec4 FragColor;

float rand(float x, float y) {
    return fract(sin(dot(vec2(x, y), vec2(46.49791, -77.58153))) * 839.441956);
}

float waves(float rx, float ry) {
    return (sin(rx*2.0 - ry + time*1.1 + 1.3) * 0.5 + 0.5)
         * (sin(ry*3.0 - rx + time*0.5 + 2.7) * 0.5 + 0.5);
}

void main() {
    vec3 norm = normalize(fPosition);
    float rotX = asin(norm.y);
    float rotY = acos(clamp(norm.x / cos(rotX), -1, 1));
    rotY *= sign(norm.z);

    float rx = rotX + sin(rotY*3.0 + time*4.4 - 0.7) * 0.2;
    float ry = rotY + sin(rotX*2.0 - time*2.3 + 1.5) * 0.2;

    float w = waves(rx, ry);

    float p = pow(min(w+0.3, 1), 3)*0.15;
    float s = sqrt(w);
    vec3 skyMix = vec3(p + s*0.2, p, p + s);
    skyMix.xyz += rand(rotX, rotY)*0.1;

    float sunStrenght = max(pow(dot(norm, sunDirection)+0.01, 64), 0);

    FragColor = vec4(skyMix * (1.0-clamp(sunStrenght, -0.2, 0.2)) + sunColor * sunStrenght, 1.0);
}