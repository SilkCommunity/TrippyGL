#version 330 core

uniform mat4 View;
uniform mat4 Projection;

in vec3 vPosition;
in vec3 vNormal;
in float vHumidity;
in float vVegetation;

out vec3 fPosition;
out vec3 fNormal;
out float fAltitude;
out float fHumidity;
out float fVegetation;

void main() {
    fPosition = vPosition;
    fNormal = vNormal;
    fAltitude = vPosition.y;
    fHumidity = vHumidity;
    fVegetation = vVegetation;

    gl_Position = Projection * View * vec4(vPosition, 1.0);
}