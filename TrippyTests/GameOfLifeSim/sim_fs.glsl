#version 330 core

uniform sampler2D previous;
uniform vec2 pixelDelta;

in vec2 fTexCoords;

out vec4 FragColor;

void main() {
    vec4 prev = texture(previous, fTexCoords);

    vec4 sum = texture(previous, fTexCoords + vec2(-pixelDelta.x, -pixelDelta.y));
    sum += texture(previous, fTexCoords + vec2(0, -pixelDelta.y));
    sum += texture(previous, fTexCoords + vec2(pixelDelta.x, -pixelDelta.y));
    sum += texture(previous, fTexCoords + vec2(-pixelDelta.x, 0));
    sum += texture(previous, fTexCoords + vec2(pixelDelta.x, 0));
    sum += texture(previous, fTexCoords + vec2(-pixelDelta.x, pixelDelta.y));
    sum += texture(previous, fTexCoords + vec2(0, pixelDelta.y));
    sum += texture(previous, fTexCoords + vec2(pixelDelta.x, pixelDelta.y));

    // Single color mode
    /*vec3 next = mix(
       vec3(step(2.8, sum.x) * step(sum.x, 3.2)),
       vec3(step(1.9, sum.x) * step(sum.x, 3.1)),
       step(0.5, prev.x)
    );*/

    // Multi color mode
    vec3 next = mix(
       vec3(step(vec3(2.8), sum.xyz) * step(sum.xyz, vec3(3.2))),
       vec3(step(vec3(1.9), sum.xyz) * step(sum.xyz, vec3(3.1))),
       step(0.5, prev.xyz)
    );
 
    FragColor = vec4(next, 1.0);
}