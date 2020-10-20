#version 330 core

in vec3 fPosition;
in vec3 fNormal;
in float fAltitude;
in float fHumidity;
in float fVegetation;

out vec4 FragColor;

vec3 getTerrainColor(float height, float humidity) {
    if (height <= 0.0)
        return vec3(0.0, 0.0, 1.0);

    if (height <= 0.2)
        return vec3(0.7, 0.6, 0.4);

    if (height <= 0.6)
        return vec3(0.1, 0.7, 0.0);

    if (height <= 0.8)
        return vec3(0.5, 0.55, 0.52);

    return vec3(0.84, 0.9, 1.0);
}

void main() {
    //FragColor = vec4(getTerrainColor(fPosition.y/2.0, fHumidity), 1.0);
    FragColor = vec4(vec3(fHumidity), 1.0);
    //FragColor = vec4(0.0, fPosition.y, fHumidity, 1.0);
}