#version 330 core

uniform vec3 sunDirection;
uniform vec3 sunColor;

in vec3 fPosition;

out vec4 FragColor;

const float Pi = 3.1415926535897932;

void main() {
    vec3 vec = normalize(fPosition);
    float rotX = asin(vec.y);
    float rotY = acos(clamp(vec.x / cos(rotX), -1, 1));
    rotY *= sign(vec.z);
    
    float texCoord = rotX / Pi + 0.5;
    vec3 color = texCoord > 0.5 ? 
        mix(vec3(0.42, 0.76, 0.73), vec3(0.056, 0.27, 0.63), (texCoord-0.5)*2.0)
      : mix(vec3(0.75, 0.95, 1.0), vec3(0.42, 0.76, 0.73), texCoord*2.0);

    float sunStrenght = pow(clamp(dot(sunDirection, vec)+0.005, 0, 1), 32.0);
    FragColor.rgb = mix(color, sunColor, sunStrenght);
    FragColor.a = 1.0;
}