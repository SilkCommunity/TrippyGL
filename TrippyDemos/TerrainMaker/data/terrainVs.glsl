#version 330 core

uniform vec3 cameraPos;
uniform vec3 sunDirection;

uniform mat4 View;
uniform mat4 Projection;

in vec3 vPosition;
in vec3 vNormal;
in vec4 vColor;
in vec2 vLightingConfig;

out vec3 fPosition;
out vec3 fNormal;
out vec4 fColor;

void main() {
    
    float d = 1.0; //max(dot(sunDirection, vNormal)*0.2+0.8, 0.775f);
    vec3 r = reflect(-sunDirection, vNormal);

    float specular = max(dot(r, normalize(cameraPos - vPosition)), 0);
    d += vLightingConfig.x * pow(specular, vLightingConfig.y);
    
    fPosition = vPosition;
    fNormal = vNormal;
    fColor = vec4(vColor.rgb * d, vColor.a);

    gl_Position = Projection * View * vec4(vPosition, 1.0);
}