#version 330 core

uniform mat4 Projection;

in float X;
in float Y;
in float Z;

in float colorR;
in float colorG;
in float colorB;

in mat4 matrix1;

in float nothing0;
in float sixtyFour;
in float sixtyThree;
in vec4 oneTwoThreeFour;
in int alwaysZero;
in float alsoZero;

out vec4 fColor;

void main() {
    gl_Position = Projection * matrix1 * vec4(X + float(alwaysZero), Y + alsoZero, Z, oneTwoThreeFour.x);
    fColor = vec4(
        colorR * (nothing0 + 1.0),
        colorG / 2048.0 * (oneTwoThreeFour.z - oneTwoThreeFour.y),
        colorB * (sixtyFour - sixtyThree),
        oneTwoThreeFour.w - oneTwoThreeFour.z
    );
}