#version 400 core

uniform sampler2D reflectionSamp;
uniform sampler2D refractionSamp;
uniform sampler2D distortMap;
uniform sampler2D normalMap;
uniform float time;
uniform vec3 cameraPos;

in vec4 fColor;
in vec4 fClipSpace;
in vec2 distortMapCoords;
in vec3 pos;

out vec4 FragColor;

const vec3 sunlightDirection = normalize(-vec3(0.823, 0.385, -0.418));

void main() {
	vec2 distort = (texture(distortMap, distortMapCoords).xy * 2.0 - 1.0) * 0.01;
	vec3 nmcol = texture(normalMap, distortMapCoords).xyz;
	vec3 normal = normalize(vec3(nmcol.r * 2.0 - 1.0, nmcol.b, nmcol.g * 2.0 - 1.0));

	vec2 texCoords = (fClipSpace.xy / fClipSpace.w) * 0.5 + 0.5 + distort;

	vec4 reflectionCol = texture(reflectionSamp, vec2(texCoords.x, 1.0-texCoords.y));
	vec4 refractionCol = texture(refractionSamp, texCoords);

	vec3 tocam = normalize(cameraPos - pos);
	FragColor = fColor * mix(reflectionCol, refractionCol, pow(dot(tocam, vec3(0.0, 1.0, 0.0)), 1.2));

	vec3 reflectedLightDirection = reflect(sunlightDirection, normal);
	FragColor += max(0, pow(dot(reflectedLightDirection, tocam), 30.0) * 0.75);

	//FragColor += clamp(reflectionCol + refractionCol + distort.xyxy + normal.xyzx + time + cameraPos.xyzx, 0.0, 0.0001);
}