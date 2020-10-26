#version 330 core

uniform sampler2D textureSamp;
uniform sampler2D depthSamp;

uniform float nearPlane;
uniform float farPlane;

uniform float maxDistance;

uniform vec3 waterColor;

in vec4 fColor;
in vec2 fTexCoords;

out vec4 FragColor;

float depthToDistance(in float depth) {
    return 2.0 * nearPlane * farPlane / (farPlane + nearPlane - (2.0 * depth - 1.0) * (farPlane - nearPlane));
}

void main() {
	float dist = depthToDistance(texture(depthSamp, fTexCoords).x);
	FragColor = mix(texture(textureSamp, fTexCoords), vec4(waterColor, 1.0), min(dist / maxDistance, 1.0));
}