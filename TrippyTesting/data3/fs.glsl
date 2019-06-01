#version 400

in vec4 fColor;

out vec4 FragColor;

void main() {
	FragColor = fColor;
	//FragColor = vec4(gl_FragCoord.xy / vec2(1280.0, 720.0), 0.0, 1.0);
}