#version 330 core

uniform sampler2D samp;

in vec2 fTexCoords;

out vec4 FragColor;

void main() {
    //FragColor = texture(samp, fTexCoords) + vec4(fract(fTexCoords*8.0), 0.0, 1.0);
    FragColor = texture(samp, fTexCoords);
}