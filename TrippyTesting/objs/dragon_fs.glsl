#version 330 core

uniform vec3 cameraPos;

uniform float reflectivity;

uniform vec3 lightDir;
uniform vec4 lightColor;
uniform float shineDamper;

in vec3 fNormal;
in vec3 fPosition;

out vec4 FragColor;

vec4 calcDirectionalLight(in vec3 norm, in vec3 ldir, in vec4 lcolor, in vec3 toCamVec, in float shDamper) {
    float brightness = max(0.0, dot(norm, -ldir));
    vec3 reflectedDir = reflect(ldir, norm);
    float specFactor = max(0.0, dot(reflectedDir, toCamVec));
    float dampedFactor = pow(specFactor, shDamper);
    return (brightness + (dampedFactor * reflectivity)) * lcolor;
}

void main() {
    vec3 unitNormal = normalize(fNormal);
    vec3 unitToCameraVec = normalize(cameraPos - fPosition);

    vec4 light = vec4(0.0);

    /*float brightness = max(0.0, dot(unitNormal, unitToLightVec));
    vec3 reflectedLightDir = reflect(unitNormal, -unitToLightVec);
    float specularFactor = max(0.0, dot(reflectedLightDir, unitToCameraVec));
    float dampedFactor = pow(specularFactor, shineDamper);
    light += (brightness + (dampedFactor * reflectivity)) * lightColor;*/

    light += calcDirectionalLight(unitNormal, lightDir, lightColor, unitToCameraVec, shineDamper);

    FragColor = light;
}